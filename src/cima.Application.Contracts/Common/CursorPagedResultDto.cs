using System;
using System.Collections.Generic;

namespace cima.Common;

/// <summary>
/// Resultado paginado basado en cursor para mejor rendimiento con grandes datasets
/// </summary>
public class CursorPagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public string? NextCursor { get; set; }
    public string? PreviousCursor { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public int Count => Items.Count;
    public int PageSize { get; set; }
    public long? TotalCount { get; set; }
}

public class CursorPagedRequestDto
{
    public string? Cursor { get; set; }
    public CursorDirection Direction { get; set; } = CursorDirection.Forward;
    public int PageSize { get; set; } = 20;
    public bool IncludeTotalCount { get; set; } = false;
}

public enum CursorDirection
{
    Forward,
    Backward
}

public static class CursorUtils
{
    public static string CreateCursor(Guid id, DateTime timestamp)
    {
        var data = $"{timestamp:O}|{id}";
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));
    }

    public static (DateTime Timestamp, Guid Id)? ParseCursor(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor))
            return null;

        try
        {
            var data = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = data.Split('|');
            
            if (parts.Length != 2)
                return null;

            var timestamp = DateTime.Parse(parts[0]);
            var id = Guid.Parse(parts[1]);
            
            return (timestamp, id);
        }
        catch
        {
            return null;
        }
    }

    public static string CreateIdCursor(Guid id)
    {
        return Convert.ToBase64String(id.ToByteArray());
    }

    public static Guid? ParseIdCursor(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor))
            return null;

        try
        {
            var bytes = Convert.FromBase64String(cursor);
            return new Guid(bytes);
        }
        catch
        {
            return null;
        }
    }
}
