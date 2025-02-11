using BuildingBlocks.CQRS;
using FluentValidation;
using Profile.API.Data;
using Profile.API.Exceptions;
using Profile.API.Models;

namespace Profile.API.Features.DoctorProfiles.GetDoctorProfile;

public record GetDoctorProfileQuery(Guid Id) : IQuery<GetDoctorProfileResult>;

public record GetDoctorProfileResult(DoctorProfile DoctorProfile);

public class GetDoctorProfileQueryValidator : AbstractValidator<GetDoctorProfileQuery>
{
    public GetDoctorProfileQueryValidator()
    {
        RuleFor(q => q.Id).NotEmpty().WithMessage("Id bác sĩ không được để trống");
    }
}

public class GetDoctorProfileHandler : IQueryHandler<GetDoctorProfileQuery, GetDoctorProfileResult>
{
    private readonly ProfileDbContext _context;

    public GetDoctorProfileHandler(ProfileDbContext context)
    {
        _context = context;
    }
    
    public async Task<GetDoctorProfileResult> Handle(GetDoctorProfileQuery query, CancellationToken cancellationToken)
    {
        var doctorProfile = await _context.DoctorProfiles.FindAsync(query.Id)
                            ?? throw new ProfileNotFoundException(query.Id.ToString());

        return new GetDoctorProfileResult(doctorProfile);
    }
}