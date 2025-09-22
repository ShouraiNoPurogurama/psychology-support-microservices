namespace Feed.Application.Abstractions.CursorService;

public interface ICursorService
{
    string EncodeCursor(int offset, DateTime snapshotTs);
    (int Offset, DateTime SnapshotTs) DecodeCursor(string cursor);
    bool ValidateCursor(string cursor);
}
