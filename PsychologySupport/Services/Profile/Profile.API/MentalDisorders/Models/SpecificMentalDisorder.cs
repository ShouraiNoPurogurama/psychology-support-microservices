using Profile.API.PatientProfiles.Models;
using System;
using System.Collections.Generic;

namespace Profile.API.MentalDisorders.Models
{
    public class SpecificMentalDisorder
    {
        private readonly List<MedicalHistory> _medicalHistories = [];
        private readonly List<MedicalRecord> _medicalRecords = [];

        public Guid Id { get; init; }

        public Guid MentalDisorderId { get; init; }

        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public MentalDisorder MentalDisorder { get; init; } = null!;

        public IReadOnlyList<MedicalHistory> MedicalHistories => _medicalHistories.AsReadOnly();
        public IReadOnlyList<MedicalRecord> MedicalRecords => _medicalRecords.AsReadOnly();
    }
}
