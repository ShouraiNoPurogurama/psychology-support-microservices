using BuildingBlocks.Enums;
using Profile.API.DoctorProfiles.Models;

namespace Profile.API.DoctorProfiles.Dtos;

public record CreateDoctorProfileDto(
    Guid UserId,
    string FullName,
    UserGender Gender,
    ContactInfo ContactInfo,
    List<Specialty> Specialties,
    string Qualifications,
    int YearsOfExperience,
    string? Bio);