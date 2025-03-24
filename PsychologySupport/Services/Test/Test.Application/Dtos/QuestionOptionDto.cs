using Test.Domain.Enums;

namespace Test.Application.Dtos;

public record QuestionOptionDto(Guid Id, string Content, OptionValue OptionValue);