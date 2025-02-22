using MediatR;
using Profile.API.PatientProfiles.Models;

public record GetMedicalHistoryQuery(Guid PatientId) : IRequest<MedicalHistory?>;

public class GetMedicalHistoryHandler : IRequestHandler<GetMedicalHistoryQuery, MedicalHistory?>
{
    private readonly ProfileDbContext _context;

    public GetMedicalHistoryHandler(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<MedicalHistory?> Handle(GetMedicalHistoryQuery request, CancellationToken cancellationToken)
    {
        var patient = await _context.PatientProfiles
            .Include(p => p.MedicalHistory) 
            .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

        if (patient is null)
        {
            throw new KeyNotFoundException("Patient not found.");
        }

        return patient.GetMedicalHistory(request.PatientId);
    }
}
