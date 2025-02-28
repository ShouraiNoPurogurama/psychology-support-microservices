using BuildingBlocks.Pagination;
using Profile.API.DoctorProfiles.Dtos;

namespace Profile.API.DoctorProfiles.Features.GetDoctorProfileBySpecialities;

public record GetDoctorProfileBySpecialitiesQuery(PaginationRequest PaginationRequest)
    : IQuery<GetDoctorProfileBySpecialitiesResult>;

public record GetDoctorProfileBySpecialitiesResult(DoctorProfileDto DoctorProfile);

public class GetDoctorProfileBySpecialitiesHandler
{
}