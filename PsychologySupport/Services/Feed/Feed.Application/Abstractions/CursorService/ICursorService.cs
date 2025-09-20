using System;

namespace Feed.Application.Abstractions.CursorService;

public interface ICursorService
{
    string EncodeCursor(int pageIndex, DateTime snapshotTime);
    (int PageIndex, DateTime SnapshotTime) DecodeCursor(string cursor);
    bool ValidateCursor(string cursor);
}
