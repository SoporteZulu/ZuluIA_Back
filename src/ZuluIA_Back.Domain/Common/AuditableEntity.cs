namespace ZuluIA_Back.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset UpdatedAt { get; protected set; }
    public DateTimeOffset? DeletedAt { get; protected set; }
    public long? CreatedBy { get; protected set; }
    public long? UpdatedBy { get; protected set; }

    public bool IsDeleted => DeletedAt.HasValue;

    protected void SetCreated(long? userId)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        CreatedBy = userId;
    }

    protected void SetUpdated(long? userId)
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = userId;
    }

    protected void SetDeleted()
    {
        DeletedAt = DateTimeOffset.UtcNow;
    }
}
