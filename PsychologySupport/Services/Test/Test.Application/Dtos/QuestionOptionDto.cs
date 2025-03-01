using Test.Domain.Enums;

namespace Test.Application.Dtos;

public record QuestionOptionDto(string Content, OptionValue OptionValue);