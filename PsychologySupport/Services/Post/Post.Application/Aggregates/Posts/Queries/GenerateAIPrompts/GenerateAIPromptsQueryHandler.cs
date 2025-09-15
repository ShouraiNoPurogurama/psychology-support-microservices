using BuildingBlocks.CQRS;
using Post.Application.Data;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Posts.Queries.GenerateAIPrompts;

internal sealed class GenerateAIPromptsQueryHandler : IQueryHandler<GenerateAIPromptsQuery, GenerateAIPromptsResult>
{
    private readonly IQueryDbContext _queryContext;
    private static readonly List<AIPromptDto> _defaultPrompts = new()
    {
        new AIPromptDto(
            "Share your daily mood",
            "How are you feeling today? What's affecting your emotional state?",
            ["mood", "daily-check-in", "emotions"],
            "question"
        ),
        new AIPromptDto(
            "Mindfulness moment",
            "Share a moment when you felt truly present and mindful today.",
            ["mindfulness", "present-moment", "awareness"],
            "topic"
        ),
        new AIPromptDto(
            "Gratitude practice",
            "What are three things you're grateful for right now?",
            ["gratitude", "positive-thinking", "appreciation"],
            "challenge"
        ),
        new AIPromptDto(
            "Stress management",
            "What's your go-to strategy when feeling overwhelmed?",
            ["stress", "coping-strategies", "self-care"],
            "question"
        ),
        new AIPromptDto(
            "Personal growth",
            "Share something new you learned about yourself recently.",
            ["self-discovery", "growth", "reflection"],
            "topic"
        ),
        new AIPromptDto(
            "Support someone today",
            "How did you help someone or show kindness today?",
            ["kindness", "support", "community"],
            "challenge"
        ),
        new AIPromptDto(
            "Anxiety check-in",
            "If you're feeling anxious, what helps you feel more grounded?",
            ["anxiety", "grounding", "mental-health"],
            "question"
        ),
        new AIPromptDto(
            "Weekly reflection",
            "Looking back at this week, what was your biggest win?",
            ["reflection", "achievements", "weekly-review"],
            "topic"
        )
    };

    public GenerateAIPromptsQueryHandler(IQueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<GenerateAIPromptsResult> Handle(GenerateAIPromptsQuery request, CancellationToken cancellationToken)
    {
        List<AIPromptDto> prompts;

        if (request.CategoryTagId.HasValue)
        {
            // Get category-specific prompts based on emotion tags
            var categoryTag = await _queryContext.EmotionTagReplicas
                .FirstOrDefaultAsync(et => et.Id == request.CategoryTagId.Value, cancellationToken);

            if (categoryTag != null)
            {
                prompts = GetCategorySpecificPrompts(categoryTag.Code, request.Count);
            }
            else
            {
                prompts = GetRandomPrompts(request.Count);
            }
        }
        else
        {
            prompts = GetRandomPrompts(request.Count);
        }

        return new GenerateAIPromptsResult(prompts);
    }

    private static List<AIPromptDto> GetCategorySpecificPrompts(string categoryCode, int count)
    {
        var categoryPrompts = categoryCode.ToLower() switch
        {
            "anxiety" or "stress" => _defaultPrompts.Where(p => 
                p.SuggestedTags.Any(tag => 
                    tag.Contains("anxiety") || tag.Contains("stress") || tag.Contains("coping"))).ToList(),
            
            "depression" or "sad" => _defaultPrompts.Where(p => 
                p.SuggestedTags.Any(tag => 
                    tag.Contains("mood") || tag.Contains("gratitude") || tag.Contains("support"))).ToList(),
            
            "mindfulness" or "meditation" => _defaultPrompts.Where(p => 
                p.SuggestedTags.Any(tag => 
                    tag.Contains("mindfulness") || tag.Contains("present") || tag.Contains("awareness"))).ToList(),
            
            _ => _defaultPrompts
        };

        return categoryPrompts.OrderBy(_ => Random.Shared.Next()).Take(count).ToList();
    }

    private static List<AIPromptDto> GetRandomPrompts(int count)
    {
        return _defaultPrompts.OrderBy(_ => Random.Shared.Next()).Take(count).ToList();
    }
}
