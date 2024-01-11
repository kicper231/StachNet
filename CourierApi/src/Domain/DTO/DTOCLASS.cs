﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO;

public class DeliveryRequestDTO
{
    //public string UserAuth0 { get; set; }
    
    public PackageDTO Package { get; set; }
    public AddressDTO SourceAddress { get; set; }
    public AddressDTO DestinationAddress { get; set; }
    public DateTime DeliveryDate { get; set; }
    public bool Priority { get; set; }
    public bool WeekendDelivery { get; set; }
}

public class UserDetailsDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string CompanyName { get; set; }
}

public class AddressDTO
{
    public string HouseNumber { get; set; }  // New property
    public string ApartmentNumber { get; set; }  // New property
    public string Street { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }

}

public class PackageDTO
{
    public double Width { get; set; }  // New property
    public double Height { get; set; }  // New property
    public double Length { get; set; }  // New property
    public double Weight { get; set; }
   
}




/// odebranie inquires od api 
/// 

public class InquiryDTO
{
    public string InquiryId { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; }
    public DateTime ExpiringAt { get; set; }
    public List<PriceBreakdown> PriceBreakDown { get; set; }
}

public class PriceBreakdown
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Description { get; set; }
}