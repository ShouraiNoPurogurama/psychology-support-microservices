﻿using System.ComponentModel.DataAnnotations.Schema;
using BuildingBlocks.DDD;
using Test.Domain.Enums;
using Test.Domain.ValueObjects;

namespace Test.Domain.Models;

public class TestResult : AggregateRoot<Guid>
{
    public TestResult(Guid id, Guid patientId, Guid testId, Score depressionScore,
        Score anxietyScore, Score stressScore, SeverityLevel severityLevel, Recommendation recommendation,
        string profileNickname, string profileDescription) 
    {
        Id = id;
        PatientId = patientId;
        TestId = testId;
        TakenAt = DateTime.UtcNow;
        DepressionScore = depressionScore;
        AnxietyScore = anxietyScore;
        StressScore = stressScore;
        SeverityLevel = severityLevel;
        Recommendation = recommendation;
        ProfileNickname = profileNickname;
        ProfileDescription = profileDescription;
    }

    private TestResult() { }
    
    public Guid PatientId { get; private set; }
    public Guid TestId { get; private set; }
    public DateTime TakenAt { get; private set; }
    public Score DepressionScore { get; private set; }
    public Score AnxietyScore { get; private set; }
    public Score StressScore { get; private set; }
    public SeverityLevel SeverityLevel { get; private set; }
    public Recommendation Recommendation { get; private set; }
    [NotMapped]
    public string ProfileNickname { get; private set; } = ""; //<-- THÊM DÒNG NÀY
    [NotMapped]
    public string ProfileDescription { get; private set; } = ""; //<-- THÊM DÒNG NÀY

    
    public string RecommendationJson
    {
        get => Recommendation.ToJson() ?? "";
        set => Recommendation = Recommendation.FromJson(value);
    }

    public virtual ICollection<QuestionOption> SelectedOptions { get; private set; } = [];

    public static TestResult Create(Guid patientId, Guid testId, Score depressionScore,
        Score anxietyScore, Score stressScore, SeverityLevel severityLevel,
        Recommendation recommendation, string profileNickname, string profileDescription, List<QuestionOption>? selectedOptions)
    {
        var newTestResult = new TestResult(Guid.NewGuid(), patientId, testId, depressionScore, anxietyScore, stressScore,
            severityLevel, recommendation, profileNickname, profileDescription);

        newTestResult.AddSelectedOptions(selectedOptions ?? []);
        return newTestResult;
    }

    public void AddSelectedOptions(IEnumerable<QuestionOption> options)
    {
        foreach (var option in options)
        {
            SelectedOptions.Add(option);
        }
    }
}