namespace Test.Application.Dtos;

public record TestQuestionDto(Guid Id ,int Order, string Content, List<QuestionOptionDto> Options);