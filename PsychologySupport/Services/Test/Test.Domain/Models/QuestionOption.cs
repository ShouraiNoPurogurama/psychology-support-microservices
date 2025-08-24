﻿using BuildingBlocks.DDD;
using Test.Domain.Enums;

namespace Test.Domain.Models;

public class QuestionOption : AuditableEntity<Guid>
{
    private QuestionOption()
    {
    }

    public QuestionOption(Guid id, Guid questionId, string content, OptionValue optionValue)
    {
        Id = id;
        QuestionId = questionId;
        Content = content;
        OptionValue = optionValue;
    }

    public Guid QuestionId { get; private set; }
    public string Content { get; private set; }
    public OptionValue OptionValue { get; private set; }
    
    public virtual ICollection<TestResult> TestResults { get; private set; } = new List<TestResult>();
    
}