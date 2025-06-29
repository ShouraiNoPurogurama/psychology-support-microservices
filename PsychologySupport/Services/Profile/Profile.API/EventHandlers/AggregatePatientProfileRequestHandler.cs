using BuildingBlocks.Messaging.Events.Profile;
using Profile.API.PatientProfiles.Features.GetPatientProfile;

namespace Profile.API.EventHandlers;

public class AggregatePatientProfileRequestHandler(ISender sender, ILogger<AggregatePatientProfileRequestHandler> logger) : IConsumer<AggregatePatientProfileRequest>
{
    public async Task Consume(ConsumeContext<AggregatePatientProfileRequest> context)
    {
        logger.LogInformation("Processing AggregatePatientProfileRequest for PatientProfileId: {PatientProfileId}", context.Message.PatientProfileId);
        
        var query = new GetPatientProfileQuery(context.Message.PatientProfileId);
        
        var result = await sender.Send(query, context.CancellationToken);
        
        var response = MapToAggregatePatientProfileResponse(result);
        
        await context.RespondAsync(response);
        
        logger.LogInformation("Successfully aggregated patient profile for PatientProfileId: {PatientProfileId}", context.Message.PatientProfileId);
    }
    
    private static AggregatePatientProfileResponse MapToAggregatePatientProfileResponse(GetPatientProfileResult result)
    {
        var dto = result.PatientProfileDto;

        var response = new AggregatePatientProfileResponse(
            Id: dto.Id,
            FullName: dto.FullName,
            Gender: dto.Gender,
            Allergies: dto.Allergies,
            PersonalityTraits: dto.PersonalityTraits,
            Address: dto.ContactInfo.Address,
            PhoneNumber: dto.ContactInfo.PhoneNumber,
            Email: dto.ContactInfo.Email,
            BirthDate: dto.BirthDate,
            JobTitle: dto.Job?.JobTitle ?? string.Empty,
            EducationLevel: dto.Job?.EducationLevel.ToString() ?? string.Empty,
            IndustryName: dto.Job?.Industry.IndustryName ?? string.Empty
        );
        
        return response;
    }
}