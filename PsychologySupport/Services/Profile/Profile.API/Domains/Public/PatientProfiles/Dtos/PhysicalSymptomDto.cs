﻿namespace Profile.API.Domains.PatientProfiles.Dtos;

public record PhysicalSymptomDto(
    Guid Id,
    string Name,
    string Description
);