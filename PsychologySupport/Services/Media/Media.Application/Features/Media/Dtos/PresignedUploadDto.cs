namespace Media.Application.Features.Media.Dtos
{
    public record PresignedUploadDto
    {
        public string UploadUrl { get; init; } = default!;
        public DateTime ExpiresAt { get; init; }
    }
}