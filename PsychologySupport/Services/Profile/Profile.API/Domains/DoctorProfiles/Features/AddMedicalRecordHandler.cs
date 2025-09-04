using Profile.API.Domains.PatientProfiles.Exceptions;
using Profile.API.Models.Public;

namespace Profile.API.Domains.DoctorProfiles.Features;

public record AddMedicalRecordCommand(MedicalRecord MedicalRecord)
    : ICommand<AddMedicalRecordResult>;

public record AddMedicalRecordResult(bool IsSuccess);

public class AddMedicalRecordHandler : ICommandHandler<AddMedicalRecordCommand, AddMedicalRecordResult>
{
    private readonly ProfileDbContext _context;

    public AddMedicalRecordHandler(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<AddMedicalRecordResult> Handle(AddMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        var doctorProfile = await _context.DoctorProfiles.AsNoTracking()
                                .Include(d => d.MedicalRecords)
                                .FirstOrDefaultAsync(d => d.Id.Equals(request.MedicalRecord.DoctorProfileId), cancellationToken)
                            ?? throw new ProfileNotFoundException("Doctor Profile", request.MedicalRecord.DoctorProfileId);

        doctorProfile.AddMedicalRecord(request.MedicalRecord);

        var result = await _context.SaveChangesAsync(cancellationToken);

        return new AddMedicalRecordResult(result > 0);
    }
}