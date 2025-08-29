﻿using BuildingBlocks.Data.Common;
using BuildingBlocks.Enums;

namespace Profile.API.Domains.DoctorProfiles.Dtos;

public record UpdateDoctorProfileDto(
    string? FullName,
    UserGender? Gender,
    ContactInfo? ContactInfo,
    List<Guid>? SpecialtyIds,
    string? Qualifications,
    int? YearsOfExperience,
    string? Bio
);