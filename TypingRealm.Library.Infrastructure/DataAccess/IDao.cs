using System;

namespace TypingRealm.Library.Infrastructure.DataAccess;

// TODO: Move this to some shared framework project and reuse it in other Infrastructure projects.
public interface IDao<in T>
{
    string Id { get; }
}

public abstract class TrackableDao
{
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
