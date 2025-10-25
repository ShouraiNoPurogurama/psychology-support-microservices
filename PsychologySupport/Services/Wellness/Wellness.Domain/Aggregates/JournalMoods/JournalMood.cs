using System;
using Wellness.Domain.Abstractions;

namespace Wellness.Domain.Aggregates.JournalMoods;

public sealed class JournalMood : AggregateRoot<Guid>
{
    public Guid SubjectRef { get; private set; }
    public Guid MoodId { get; private set; }
    public string? Note { get; private set; }

    // --- Navigation ---
    public Mood Mood { get; private set; } = null!;
    private JournalMood() { }

    private JournalMood(Guid subjectRef, Guid moodId, string? note)
    {
        if (subjectRef == Guid.Empty)
            throw new ArgumentException("SubjectRef không được rỗng.", nameof(subjectRef));
        if (moodId == Guid.Empty)
            throw new ArgumentException("MoodId không được rỗng.", nameof(moodId));

        Id = Guid.NewGuid();
        SubjectRef = subjectRef;
        MoodId = moodId;
        Note = note?.Trim();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static JournalMood Create(Guid subjectRef, Guid moodId, string? note = null)
    {
        var journalMood = new JournalMood(subjectRef, moodId, note);

        return journalMood;
    }

    public void Update(string? newNote)
    {

        Note = newNote?.Trim();
        LastModified = DateTimeOffset.UtcNow;
    }
}
