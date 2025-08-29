using Profile.API.Data.Public;

namespace Profile.API.Domains.PatientProfiles.Features.UpdateMedicalHistory;

public record UpdateMedicalHistoryCommand(
    Guid PatientProfileId,
    string Description,
    DateTimeOffset DiagnosedAt,
    List<Guid> DisorderIds,
    List<Guid> PhysicalSymptomIds) : ICommand<UpdateMedicalHistoryResult>;

public record UpdateMedicalHistoryResult(bool IsSuccess);

public class UpdateMedicalHistoryHandler : ICommandHandler<UpdateMedicalHistoryCommand, UpdateMedicalHistoryResult>
{
    private readonly ProfileDbContext _context;

    public UpdateMedicalHistoryHandler(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateMedicalHistoryResult> Handle(UpdateMedicalHistoryCommand request, CancellationToken cancellationToken)
    {
        var patientProfile = await _context.PatientProfiles
            .Include(pp => pp.MedicalHistory)
            .FirstOrDefaultAsync(pp => pp.Id == request.PatientProfileId, cancellationToken);

        if (patientProfile == null) return new UpdateMedicalHistoryResult(false);

        var disorders = await _context.SpecificMentalDisorders
            .Where(d => request.DisorderIds.Contains(d.Id))
            .ToListAsync(cancellationToken);

        var symptoms = await _context.PhysicalSymptoms
            .Where(s => request.PhysicalSymptomIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        patientProfile.UpdateMedicalHistory(request.Description, request.DiagnosedAt, disorders, symptoms);
        await _context.SaveChangesAsync(cancellationToken);
        return new UpdateMedicalHistoryResult(true);
    }
}