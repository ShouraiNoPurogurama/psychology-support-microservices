using BuildingBlocks.Data.Common;
using BuildingBlocks.DDD;
using BuildingBlocks.Enums;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.DoctorProfiles.Models;

public class DoctorProfile : AggregateRoot<Guid>
{
    private readonly List<MedicalRecord> _medicalRecords = [];

    public DoctorProfile()
    {
    }

    public DoctorProfile(
        Guid userId,
        string fullName,
        UserGender gender,
        ContactInfo contactInfo,
        List<Specialty> specialties,
        string qualifications,
        int yearsOfExperience,
        string? bio,
        float rating = 0,
        int totalReviews = 0)

    {
        UserId = userId;
        FullName = fullName;
        Gender = gender;
        ContactInfo = contactInfo;
        Specialties = specialties;
        Qualifications = qualifications;
        YearsOfExperience = yearsOfExperience;
        Bio = bio;
        Rating = rating;
        TotalReviews = totalReviews;
    }

    public Guid UserId { get; set; }

    public string FullName { get; set; } = default!;

    public UserGender Gender { get; set; }
    public ContactInfo ContactInfo { get; set; } = default!;
    public string? Qualifications { get; set; }

    public int YearsOfExperience { get; set; }

    public string? Bio { get; set; }

    public float Rating { get; set; }

    public int TotalReviews { get; set; }
    public ICollection<Specialty> Specialties { get; set; } = [];
    public IReadOnlyList<MedicalRecord> MedicalRecords => _medicalRecords.AsReadOnly();

    // public decimal BookingPrice;

    public static DoctorProfile Create(
        Guid userId,
        string fullName,
        UserGender gender,
        ContactInfo contactInfo,
        List<Specialty> specialties,
        string qualifications,
        int yearsOfExperience,
        string? bio,
        float rating = 0,
        int totalReviews = 0)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty.", nameof(fullName));

        if (contactInfo == null)
            throw new ArgumentNullException(nameof(contactInfo), "Contact info cannot be null.");

        return new DoctorProfile(
            userId,
            fullName,
            gender,
            contactInfo,
            specialties,
            qualifications,
            yearsOfExperience,
            bio,
            rating,
            totalReviews
        );
    }

    public void AddMedicalRecord(MedicalRecord medicalRecord)
    {
        if (medicalRecord == null)
            throw new ArgumentNullException(nameof(medicalRecord), "Medical record cannot be null.");

        _medicalRecords.Add(medicalRecord);
    }

    public void Update(
        string fullName,
        UserGender gender,
        ContactInfo contactInfo,
        List<Specialty> specialties,
        string qualifications,
        int yearsOfExperience,
        string bio)
    {
        FullName = fullName;
        Gender = gender;
        ContactInfo = contactInfo;
        Specialties = specialties;
        Qualifications = qualifications;
        YearsOfExperience = yearsOfExperience;
        Bio = bio;
    }
}