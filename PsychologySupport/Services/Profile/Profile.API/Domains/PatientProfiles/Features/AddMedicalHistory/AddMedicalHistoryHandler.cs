using Profile.API.Data.Public;
using Profile.API.Domains.PatientProfiles.Models;

namespace Profile.API.Domains.PatientProfiles.Features.AddMedicalHistory;

public record AddMedicalHistoryCommand(
    Guid PatientProfileId,
    string Description,
    DateTimeOffset DiagnosedAt,
    List<Guid> SpecificMentalDisorderIds,
    List<Guid> PhysicalSymptomIds) : ICommand<AddMedicalHistoryResult>;

public record AddMedicalHistoryResult(bool IsSuccess);

public class AddMedicalHistoryHandler : ICommandHandler<AddMedicalHistoryCommand, AddMedicalHistoryResult>
{
    private readonly ProfileDbContext _context;

    public AddMedicalHistoryHandler(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<AddMedicalHistoryResult> Handle(AddMedicalHistoryCommand request, CancellationToken cancellationToken)
    {
        var patientProfile = await _context.PatientProfiles
                                 .Include(p => p.MedicalHistory)
                                 .FirstOrDefaultAsync(p => p.Id.Equals(request.PatientProfileId), cancellationToken)
                             ?? throw new NotFoundException("Patient Profile", request.PatientProfileId);

        var specificMentalDisorders = await _context.SpecificMentalDisorders
            .Where(s => request.SpecificMentalDisorderIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        var physicalSymptoms = await _context.PhysicalSymptoms
            .Where(s => request.PhysicalSymptomIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        var newMedicalHistory = MedicalHistory.Create(request.PatientProfileId, request.Description, request.DiagnosedAt,
            specificMentalDisorders, physicalSymptoms);

        _context.MedicalHistories.Add(newMedicalHistory);

        patientProfile.ReplaceMedicalHistory(newMedicalHistory);

        var result = await _context.SaveChangesAsync(cancellationToken);

        return new AddMedicalHistoryResult(result > 0);
    }
}