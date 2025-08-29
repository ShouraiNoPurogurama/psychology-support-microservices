using Profile.API.Data.Public;
using Profile.API.Domains.PatientProfiles.Enum;
using Profile.API.Domains.PatientProfiles.Exceptions;

namespace Profile.API.Domains.PatientProfiles.Features.AddMedicalRecord;

public record AddMedicalRecordCommand(
    Guid PatientProfileId,
    Guid DoctorId,
    string Notes,
    MedicalRecordStatus Status,
    List<Guid> ExistingDisorderIds) : ICommand<AddMedicalRecordResult>;

public record AddMedicalRecordResult(bool IsSuccess);

public class AddMedicalRecordHandler : ICommandHandler<AddMedicalRecordCommand, AddMedicalRecordResult>
{
    private readonly ProfileDbContext _context;

    public AddMedicalRecordHandler(ProfileDbContext context)
    {
        _context = context;
    }

    //TODO apply Outbox & Saga pattern here
    public async Task<AddMedicalRecordResult> Handle(AddMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        var patientProfile = await _context.PatientProfiles.FindAsync(request.PatientProfileId);

        if (patientProfile == null)
            throw new ProfileNotFoundException($"Không tìm thấy hồ sơ người dùng với ID {request.PatientProfileId}");

        var existingDisorders = await _context.SpecificMentalDisorders
            .Where(s => request.ExistingDisorderIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        var medicalRecord = patientProfile.AddMedicalRecord(
            request.DoctorId,
            request.Notes,
            request.Status,
            existingDisorders
        );

        _context.MedicalRecords.Add(medicalRecord);

        var result = await _context.SaveChangesAsync(cancellationToken);

        return new AddMedicalRecordResult(result > 0);
    }
}