namespace Test.Application.Dtos;

public record TestResultOptionsDto(
    TestResultDto TestResult, 
    List<SelectedOptionDto> SelectedOptions
);

public record SelectedOptionDto(TestQuestionDto Question, QuestionOptionDto SelectedOption);