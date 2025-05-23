﻿using BuildingBlocks.Enums;

namespace LifeStyles.API.Dtos;

public record TherapeuticActivityDto(
    Guid Id,
    string TherapeuticTypeName,
    string Name,
    string Description,
    string Instructions,
    IntensityLevel IntensityLevel,
    ImpactLevel ImpactLevel
);