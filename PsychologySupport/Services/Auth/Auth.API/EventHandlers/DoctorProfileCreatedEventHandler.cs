using Auth.API.Models;
using BuildingBlocks.Constants;
using BuildingBlocks.Messaging.Events.Auth;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.EventHandlers;

public class DoctorProfileCreatedEventHandler : IConsumer<DoctorProfileCreatedIntegrationEvent>
{
    private readonly UserManager<User> _userManager;

    public DoctorProfileCreatedEventHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task Consume(ConsumeContext<DoctorProfileCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        var existingUser = await _userManager.FindByEmailAsync(message.Email);
        existingUser ??= await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == message.PhoneNumber);

        if (existingUser is not null)
        {
            throw new InvalidDataException("Email or phone number already exists in the system");
        }

        // Generate a random password if not provided
        var password = string.IsNullOrEmpty(message.Password) ? GenerateRandomPassword() : message.Password;

        // Create a new user
        var user = new User
        {
            Id = message.UserId,
            FullName = message.FullName,
            Gender = message.Gender,
            Email = message.Email,
            UserName = message.Email,
            PhoneNumber = message.PhoneNumber,
            EmailConfirmed = true, 
            PhoneNumberConfirmed = true 
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidDataException($"User registration failed: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, Roles.DoctorRole);
        if (!roleResult.Succeeded)
        {
            throw new InvalidDataException("Role assignment failed");
        }

 
        await context.Publish(new DoctorProfileCreatedIntegrationEvent(
            message.UserId,
            message.FullName,
            message.Gender,
            message.Email,
            message.PhoneNumber,
            password 
        ));
    }

    private string GenerateRandomPassword()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 8) + "Ab@1"; 
    }
}
