using BuildingBlocks.CQRS;
using OpenAI.Chat;
using Scheduling.API.Utils;

namespace Scheduling.API.Features.GeneratePlan;

public record GeneratePlanCommand(string ScheduleJson) : ICommand<GeneratePlanResult>;

public record GeneratePlanResult(string Plan);

public class GeneratePlanCommandHandler : ICommandHandler<GeneratePlanCommand, GeneratePlanResult>
{
    private readonly IConfiguration _configuration;

    public GeneratePlanCommandHandler(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        var apiKey = _configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("API Key is missing");
    }

    public async Task<GeneratePlanResult> Handle(GeneratePlanCommand request, CancellationToken cancellationToken)
    {
        var clusteringPattern = await File.ReadAllTextAsync("clustering_patterns.csv", cancellationToken);
        var clusters = await File.ReadAllTextAsync("clusters.json", cancellationToken);

        var prompt = PromptUtils.GetCreateScheduleTemplate(request.ScheduleJson, clusteringPattern, clusters);

        ChatClient client = new(
            model: "gpt-4o-mini",
            // model: "gpt-4o",
            apiKey: _configuration["OpenAI:ApiKey"]
        );

        ChatCompletion completion = await client.CompleteChatAsync(prompt);

        var responseString = completion.Content[0].Text;

        return new GeneratePlanResult(responseString);
    }
}