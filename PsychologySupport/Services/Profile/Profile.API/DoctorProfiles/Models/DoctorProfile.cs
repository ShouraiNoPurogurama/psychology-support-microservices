using BuildingBlocks.Data.Enums;
using BuildingBlocks.DDD;
using Profile.API.Common.ValueObjects;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.DoctorProfiles.Models;

public class DoctorProfile : AggregateRoot<Guid>
{
    public Guid UserId { get; set; }
    
    public string FullName { get; set; }
    
    public UserGender Gender { get; set; }
    public ContactInfo ContactInfo { get; set; } = default!;

    public string? Specialty { get; set; }
    
    public string? Qualifications { get; set; }
    
    public int YearsOfExperience { get; set; }
    
    public string? Bio { get; set; }
    
    public float Rating { get; set; }
    
    public int TotalReviews { get; set; }

    private readonly List<MedicalRecord> _medicalRecords = [];
    public IReadOnlyList<MedicalRecord> MedicalRecords => _medicalRecords.AsReadOnly();

    public DoctorProfile()
    {
        
    }

    public DoctorProfile(
        Guid userId,
        string? fullName,
        string? gender,
        ContactInfo contactInfo,
        string specialty,
        string qualifications,
        int yearsOfExperience,
        string bio,
        float rating = 0,
        int totalReviews = 0)

    {
        UserId = userId;
        FullName = fullName;

        if (!Enum.TryParse<UserGender>(gender, true, out var parsedGender))
            throw new ArgumentException("Invalid gender value. Allowed: Male, Female, Else.", nameof(gender));

        Gender = parsedGender; 

        ContactInfo = contactInfo;
        Specialty = specialty;
        Qualifications = qualifications;
        YearsOfExperience = yearsOfExperience;
        Bio = bio;
        Rating = rating;
        TotalReviews = totalReviews;
    }

    public static DoctorProfile Create(
       Guid userId,
       string fullName,
       string gender,
       ContactInfo contactInfo,
       string specialty,
       string qualifications,
       int yearsOfExperience,
       string bio,
       float rating = 0,
       int totalReviews = 0)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty.", nameof(fullName));

        if (contactInfo == null)
            throw new ArgumentNullException(nameof(contactInfo), "Contact info cannot be null.");

        if (!Enum.TryParse<UserGender>(gender, true, out var parsedGender))
            throw new ArgumentException("Invalid gender value. Allowed: Male, Female, Else.", nameof(gender));

        return new DoctorProfile(
            userId,
            fullName,
            gender,
            contactInfo,
            specialty,
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
    string gender,
    ContactInfo contactInfo,
    string specialty,
    string qualifications,
    int yearsOfExperience,
    string bio,
    float rating,
    int totalReviews)
    {

        FullName = fullName;

        if (!Enum.TryParse<UserGender>(gender, true, out var parsedGender))
            throw new ArgumentException("Invalid gender value. Allowed: Male, Female, Else.", nameof(gender));

        Gender = parsedGender; 

        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo), "Contact info cannot be null.");
        Specialty = specialty;
        Qualifications = qualifications;
        YearsOfExperience = yearsOfExperience;
        Bio = bio;
        Rating = rating;
        TotalReviews = totalReviews;
    }


}
