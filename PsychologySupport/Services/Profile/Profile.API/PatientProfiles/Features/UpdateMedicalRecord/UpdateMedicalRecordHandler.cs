using Profile.API.PatientProfiles.ValueObjects;

namespace Profile.API.PatientProfiles.Features.UpdateMedicalRecord;

public record UpdateMedicalRecordCommand(
    Guid PatientProfileId,
    Guid DoctorId,
    Guid MedicalHistoryId,
    string Notes,
    MedicalRecordStatus Status,
    List<Guid> DisorderIds) : ICommand<UpdateMedicalRecordResult>;

public record UpdateMedicalRecordResult(bool IsSuccess);

public class UpdateMedicalRecordHandler : ICommandHandler<UpdateMedicalRecordCommand, UpdateMedicalRecordResult>
{
    private readonly ProfileDbContext _context;

    public UpdateMedicalRecordHandler(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateMedicalRecordResult> Handle(UpdateMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        var patientProfile = await _context.PatientProfiles
            .Include(pp => pp.MedicalRecords)
            .ThenInclude(mr => mr.SpecificMentalDisorders)
            .FirstOrDefaultAsync(pp => pp.Id == request.PatientProfileId, cancellationToken);

        if (patientProfile == null) return new UpdateMedicalRecordResult(false);

        var disorders = await _context.SpecificMentalDisorders
            .Where(d => request.DisorderIds.Contains(d.Id))
            .ToListAsync(cancellationToken);

        try
        {
            patientProfile.UpdateMedicalRecord(request.MedicalHistoryId, request.DoctorId, request.Notes, request.Status,
                disorders);
            await _context.SaveChangesAsync(cancellationToken);
            return new UpdateMedicalRecordResult(true);
        }
        catch (KeyNotFoundException)
        {
            return new UpdateMedicalRecordResult(false);
        }
    }
}