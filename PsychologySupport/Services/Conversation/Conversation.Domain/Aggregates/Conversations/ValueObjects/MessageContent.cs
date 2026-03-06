
using Conversation.Domain.Exceptions;

namespace Conversation.Domain.Aggregates.Conversations.ValueObjects;

public sealed record MessageContent
{
    public string Text { get; init; } = default!;
    public string? AttachmentUrl { get; init; }
    public string? AttachmentType { get; init; }
    public int CharacterCount { get; init; }

    private MessageContent() { }

    private MessageContent(string text, string? attachmentUrl, string? attachmentType, int characterCount)
    {
        Text = text;
        AttachmentUrl = attachmentUrl;
        AttachmentType = attachmentType;
        CharacterCount = characterCount;
    }

    public static MessageContent Create(string text, string? attachmentUrl = null, string? attachmentType = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidMessageContentException("Nội dung tin nhắn không được để trống.");

        if (text.Length > 5000)
            throw new InvalidMessageContentException("Nội dung tin nhắn không được vượt quá 5000 ký tự.");

        if (!string.IsNullOrEmpty(attachmentUrl) && string.IsNullOrEmpty(attachmentType))
            throw new InvalidMessageContentException("Loại đính kèm là bắt buộc khi có URL đính kèm.");

        var trimmedText = text.Trim();

        return new MessageContent(
            text: trimmedText,
            attachmentUrl: attachmentUrl?.Trim(),
            attachmentType: attachmentType?.Trim(),
            characterCount: trimmedText.Length
        );
    }

    public bool HasAttachment => !string.IsNullOrWhiteSpace(AttachmentUrl);
    public bool IsLongMessage => CharacterCount > 500;
}