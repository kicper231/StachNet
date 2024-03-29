﻿using Domain.Abstractions;
using Domain.DTO;
using Domain.Model;



namespace Api.Service;

public class UserService : IUserService
{
    private readonly IUserRepository repository;

    public UserService(IUserRepository repository)
    {
        this.repository = repository;
    }

    public OperationResult<int> NumberOfLogins()
    {
        var result = repository.NumberOfUserLogins();
        if (!result.Success)
        {
        }

        return result;
    }


    public ServiceResult AddUser(DTO_UserFromAuth0 user)
    {
        try
        {
            var existingUser = repository.GetByAuth0Id(user.Auth0Id);
            if (existingUser != null)
            {
                existingUser.NumberOfLogins++;
                repository.SaveChanges();
                return new ServiceResult(false, "Użytkownik już istnieje.", 409);
            }

            var AddUser = new User();
            AddUser.Auth0Id = user.Auth0Id;
            AddUser.FirstName = user.FirstName;
            AddUser.Email = user.Email;
            AddUser.LastName = user.LastName;
            AddUser.CreatedAt = DateTime.Now;
            AddUser.NumberOfLogins = 1;
            repository.Add(AddUser);

            //if (!success)
            //{
            //    return new ServiceResult(false, "Wystąpił problem podczas zapisywania użytkownika.", 500);
            //}

            return new ServiceResult(true, "Użytkownik został dodany.", 201, AddUser);
        }
        catch (Exception ex)
        {
            return new ServiceResult(false, "Wystąpił błąd: " + ex.Message, 500);
        }
    }

    public async Task<ServiceResult> AddUserAsync(DTO_UserFromAuth0 user)
    {
        try
        {
            
            var existingUser = await repository.GetByAuth0IdAsync(user.Auth0Id);
            if (existingUser != null)
            {
                existingUser.NumberOfLogins++;
                await repository.SaveChangesAsync(); 
                return new ServiceResult(false, "Użytkownik już istnieje.", 409);
            }

            var AddUser = new User
            {
                Auth0Id = user.Auth0Id,
                FirstName = user.FirstName,
                Email = user.Email,
                LastName = user.LastName,
                CreatedAt = DateTime.Now,
                NumberOfLogins = 1
            };

            await repository.AddAsync(AddUser); 

            return new ServiceResult(true, "Użytkownik został dodany.", 201, AddUser);
        }
        catch (Exception ex)
        {
            return new ServiceResult(false, "Wystąpił błąd: " + ex.Message, 500);
        }
    }
}

public class ServiceResult
{
    public ServiceResult(bool success, string message, int statusCode, User data = null)
    {
        Success = success;
        Message = message;
        StatusCode = statusCode;
        user = data;
    }

    public bool Success { get; }
    public string Message { get; }
    public int StatusCode { get; }
    public User user { get; }
}