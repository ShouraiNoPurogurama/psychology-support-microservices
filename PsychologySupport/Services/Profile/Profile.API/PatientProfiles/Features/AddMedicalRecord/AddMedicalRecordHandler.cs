using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Profile.API.Data;
using Profile.API.PatientProfiles.ValueObjects;

namespace Profile.API.PatientProfiles.Features.AddMedicalRecord
{
    public record AddMedicalRecordCommand(
        Guid PatientProfileId,
        Guid DoctorId,
        Guid? MedicalHistoryId,
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

        public async Task<AddMedicalRecordResult> Handle(AddMedicalRecordCommand request, CancellationToken cancellationToken)
        {
            var patientProfile = await _context.PatientProfiles.FindAsync(request.PatientProfileId);
            
            if (patientProfile == null)
                throw new ArgumentException($"PatientProfile with ID {request.PatientProfileId} not found.");

            var existingDisorders = await _context.SpecificMentalDisorders
                .Where(s => request.ExistingDisorderIds.Contains(s.Id))
                .ToListAsync(cancellationToken);
            
            var medicalRecord = patientProfile.AddMedicalRecord(
                request.PatientProfileId,
                request.DoctorId,
                request.MedicalHistoryId,
                request.Notes,
                request.Status,
                existingDisorders
            );

            _context.MedicalRecords.Add(medicalRecord);
            
            var result = await _context.SaveChangesAsync(cancellationToken);
            
            return new AddMedicalRecordResult(result > 0);
        }
    }
}
