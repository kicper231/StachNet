﻿using Domain.Abstractions;
using Domain.DTO;
using Domain.Model;
using Infrastructure;
using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.VisualBasic;
using System.Reflection.Metadata.Ecma335;
using Domain.Adapters;
using Microsoft.AspNetCore.Mvc;


namespace Api.Service;

public class DeliveryRequestService : IDeliveryRequestService
{
    private readonly IAddressRepository _addressRepository;
    private readonly ICourierCompanyRepository _courierCompanyRepository;
    private readonly IEmailService _emailService;
    private readonly IPackageRepository _packageRepository;
    private readonly IDeliveryRequestRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IOfferService _offerService;
    private readonly IOfferRepository _offerRepository;

    private readonly IInquiryServiceFactory _inquiryServiceFactory;
   

    public DeliveryRequestService(IDeliveryRequestRepository repository, IUserRepository repositoryuser,
        IPackageRepository repositorypackage, IAddressRepository repositoryaddress, 
        ICourierCompanyRepository courierCompanyRepository, IOfferService offerService, IOfferRepository offerRepository, IInquiryServiceFactory inquiryServiceFactory,IEmailService Iemail)
    {
        _repository = repository;
        _userRepository = repositoryuser;
        _packageRepository = repositorypackage;
        _addressRepository = repositoryaddress;
      
        _courierCompanyRepository = courierCompanyRepository;
        _offerService = offerService;
        _offerRepository = offerRepository;
        _inquiryServiceFactory = inquiryServiceFactory;
        _emailService = Iemail;
    }

    public List<UserInquiryDTO> GetUserDeliveryRequests(string userId)
    {

        var user = _userRepository.GetByAuth0Id(userId);
        
        var deliveryRequests = _repository.GetDeliveryRequestsByUserId(userId);
        SzymonApiAdapter _adapter = new SzymonApiAdapter();



        return deliveryRequests.Select(dr => new UserInquiryDTO
        {
            Package = _adapter.ConvertToPackageDTO(dr.Package),
            SourceAddress = _adapter.ConvertToAddressDTO(dr.SourceAddress),
            DestinationAddress = _adapter.ConvertToAddressDTO(dr.DestinationAddress),
            DeliveryDate = dr.DeliveryDate,
            CreatedTime = dr.CreatedAt,
            WeekendDelivery = dr.WeekendDelivery,
            Priority = dr.Priority
        }).ToList();
    }

    


    public async Task<List<InquiryRespondDTO?>> GetOffers(InquiryDTO deliveryRequestDTO)
    {
        // dodanie do bazy danych requestu
        var addedRequestDelivery = CreateDeliveryRequest(deliveryRequestDTO);
        if (deliveryRequestDTO.UserAuth0 != null && UserExists(deliveryRequestDTO.UserAuth0))
         {
            User? user = GetUser(deliveryRequestDTO.UserAuth0);
        _emailService.AfterInquiry(deliveryRequestDTO, user.FirstName,user.Email);
        }
        //obsluga rownoległosci i zapytań wykorzystujac serwis offersservice
        var offersToSend = new List<InquiryRespondDTO?>();
        var tasks = new List<Task<InquiryRespondDTO?>>();
        tasks.Add(SafeGetOffer(_inquiryServiceFactory.CreateService("SzymonCompany").GetOffers(deliveryRequestDTO)));//zabezpieczenie przed 404 
        tasks.Add(SafeGetOffer(_inquiryServiceFactory.CreateService("StachnetCompany").GetOffers(deliveryRequestDTO)));//zabezpieczenie przed 404 
        var responseparrarel = await Task.WhenAll(tasks);
        
        offersToSend.AddRange(responseparrarel);
        offersToSend.RemoveAll(offer => offer == null);
        //dodaje przyjete oferty do bazy danych moze uzytkownik je zaakceptuje
        AddOffersToDatabase(responseparrarel, addedRequestDelivery);

        //3 oferta na razie przykladowa
        offersToSend.Add(new InquiryRespondDTO
        {
            CompanyName = "Company C",
            totalPrice = 120,
            Currency="PLN",
            expiringAt = DateTime.Now.AddDays(1),
            InquiryId = "SomeInquiryId2",
            PriceBreakDown = new List<PriceBreakdown>
            {
                new() { Amount = 90, Currency = "PLN", Description = "Podstawowa cena" },
                new() { Amount = 15, Currency = "PLN", Description = "Podatek VAT" },
                new() { Amount = 10, Currency = "PLN", Description = "Opłata za dostawę" }
            }
        });


        return offersToSend;
    }


    public async Task<OfferRespondDTO?> acceptoffer(OfferDTO offerDTO)
    {
         //przypisanie uzytkownika jesli zalogowal sie w czasie lub po porostu podeslal 
        if(offerDTO.Auth0Id!=null&& _userRepository.GetByAuth0Id(offerDTO.Auth0Id) != null)
        {
           _offerRepository.GetByInquiryId(offerDTO.InquiryId).DeliveryRequest.User=_userRepository.GetByAuth0Id(offerDTO.Auth0Id);
        }
        
        
        OfferRespondDTO? respond = default;

        // jako ze są tylk 3 firmy zaimplementuje to bez adapter i fabryki (jak starczy czas) 
        switch (offerDTO.CourierCompany)
        {
            case "SzymonCompany":
                {
                   respond = await _offerService.GetOfferSzymonID(offerDTO);
                    break;
                }

            case "StachnetCompany":
                {
                   respond = await _offerService.GetOfferOurID(offerDTO);
                    break;
                }

            default:
                throw new KeyNotFoundException("Nie znaleziono firmy: " + offerDTO.CourierCompany);

        }
       
      



        return respond;
    }

    // otoczka dla odpowiedzi gdyby 404 nie wylapuje teog 
    private async Task<InquiryRespondDTO?> SafeGetOffer(Task<InquiryRespondDTO> task)
    {
        try
        {
            return await task;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Błąd podczas wykonywania żądania: {e.Message} {e.Data}");
            return null;
        }
    }


    public DeliveryRequest CreateDeliveryRequest(InquiryDTO deliveryRequestDTO)
    {
        var package = new Package
        {
            Height = deliveryRequestDTO.Package.Height,
            Width = deliveryRequestDTO.Package.Width,
            Length = deliveryRequestDTO.Package.Length,

            Weight = deliveryRequestDTO.Package.Weight
        };
        _packageRepository.Add(package);

        // adress
        var sourceAddress = new Address
        {
            ApartmentNumber = deliveryRequestDTO.SourceAddress.ApartmentNumber,
            HouseNumber = deliveryRequestDTO.SourceAddress.HouseNumber,
            Street = deliveryRequestDTO.SourceAddress.Street,
            City = deliveryRequestDTO.SourceAddress.City,
            zipCode = deliveryRequestDTO.SourceAddress.zipCode,
            Country = deliveryRequestDTO.SourceAddress.Country
        };
        _addressRepository.Add(sourceAddress);


        var destinationAddress = new Address
        {
            ApartmentNumber = deliveryRequestDTO.DestinationAddress.ApartmentNumber,
            HouseNumber = deliveryRequestDTO.DestinationAddress.HouseNumber,
            Street = deliveryRequestDTO.DestinationAddress.Street,
            City = deliveryRequestDTO.DestinationAddress.City,
            zipCode = deliveryRequestDTO.DestinationAddress.zipCode,
            Country = deliveryRequestDTO.DestinationAddress.Country
        };
        _addressRepository.Add(destinationAddress);

        // create deliveryrequest with reference (it will work??) 
        var deliveryRequest = new DeliveryRequest
        {
            UserAuth0 = deliveryRequestDTO.UserAuth0,
            User = deliveryRequestDTO.UserAuth0 != null ? _userRepository.GetByAuth0Id(deliveryRequestDTO.UserAuth0) : null,
            DeliveryDate = deliveryRequestDTO.DeliveryDate,
            Status = DeliveryRequestStatus.Pending,

            Package = package,
            SourceAddress = sourceAddress,
            DestinationAddress = destinationAddress,
            Priority = deliveryRequestDTO.Priority ? PackagePriority.High : PackagePriority.Low,
            WeekendDelivery = deliveryRequestDTO.WeekendDelivery
        };
        deliveryRequest.CreatedAt = DateTime.Now;

        _repository.Add(deliveryRequest);

        return deliveryRequest;
    }

    // dodawnanie oferty do bazy danych 
    public void AddOffersToDatabase(InquiryRespondDTO?[] InquiryRespondDTO, DeliveryRequest lastRequest)
    {
        foreach (var respond in InquiryRespondDTO)
        {
            if (respond == null) continue;

            var offer = new Offer
            {
                OfferStatus = OfferStatus.Available,
                InquiryId = respond.InquiryId,
                CourierCompany = _courierCompanyRepository.GetByName($"{respond.CompanyName}"),
                totalPrice = respond.totalPrice,
                OfferValidity = respond.expiringAt,
                DeliveryRequest = lastRequest,
                
            };
            _offerRepository.Add(offer);

        }
    }




    public async void SendMailToLoggedUser(string userid)
    {
        // api do wysylania mail send grid nie chce dac nam konta xddd
        //var client = new SendGridClient("twój_klucz_api");
        //var from = new EmailAddress("test@example.com", "Example User");
        //var subject = "Sending with SendGrid is Fun";
        //var to = new EmailAddress("test@example.com", "Example User");
        //var plainTextContent = "and easy to do anywhere, even with C#";
        //var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
        //var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        //var response = await client.SendEmailAsync(msg);
    }



    public bool UserExists(string idAuth0)
    {
        var user = _userRepository.GetByAuth0Id(idAuth0);
        return user != null;
    }
    public User? GetUser(string idAuth0)
    {
        var user = _userRepository.GetByAuth0Id(idAuth0);
        return user;
    }

}