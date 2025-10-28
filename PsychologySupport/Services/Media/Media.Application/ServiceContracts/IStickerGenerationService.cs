namespace Media.Application.ServiceContracts;

public interface IStickerGenerationService
{
    Task<Stream> GenerateImageAsync(string prompt, CancellationToken cancellationToken);
}