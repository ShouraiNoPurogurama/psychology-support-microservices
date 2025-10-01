namespace Feed.Application.Abstractions.CursorService;

public interface ICursorService
{
    string EncodeCursor(int offset, DateTimeOffset snapshotTs);
    (int Offset, DateTimeOffset SnapshotTs) DecodeCursor(string cursor);
    bool ValidateCursor(string cursor);
}
