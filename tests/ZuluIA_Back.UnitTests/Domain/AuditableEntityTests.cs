using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Domain;

public class AuditableEntityTests
{
    // -----------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------

    private sealed class ConcreteAuditable : AuditableEntity
    {
        private ConcreteAuditable() { }

        public static ConcreteAuditable Crear(long? userId = null)
        {
            var e = new ConcreteAuditable();
            e.CallSetCreated(userId);
            return e;
        }

        public void CallSetCreated(long? userId)    => SetCreated(userId);
        public void CallSetUpdated(long? userId)    => SetUpdated(userId);
        public void CallSetDeleted()                => SetDeleted();
        public void CallSetDeletedAt(DateTimeOffset? value) => SetDeletedAt(value);
    }

    // -----------------------------------------------------------
    // SetCreated
    // -----------------------------------------------------------

    [Fact]
    public void SetCreated_EstableceCreatedAt_Reciente()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var entity = ConcreteAuditable.Crear(userId: 10);
        var after  = DateTimeOffset.UtcNow.AddSeconds(1);

        entity.CreatedAt.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void SetCreated_EstableceUpdatedAt_Reciente()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var entity = ConcreteAuditable.Crear(userId: 10);
        var after  = DateTimeOffset.UtcNow.AddSeconds(1);

        entity.UpdatedAt.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void SetCreated_EstableceCreatedBy()
    {
        var entity = ConcreteAuditable.Crear(userId: 99);

        entity.CreatedBy.Should().Be(99);
    }

    [Fact]
    public void SetCreated_UserIdNulo_CreatedByEsNulo()
    {
        var entity = ConcreteAuditable.Crear(userId: null);

        entity.CreatedBy.Should().BeNull();
    }

    [Fact]
    public void SetCreated_IsDeleted_EsFalseInicial()
    {
        var entity = ConcreteAuditable.Crear();

        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAt.Should().BeNull();
    }

    // -----------------------------------------------------------
    // SetUpdated
    // -----------------------------------------------------------

    [Fact]
    public void SetUpdated_EstableceUpdatedAt_Reciente()
    {
        var entity = ConcreteAuditable.Crear(userId: 1);

        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        entity.CallSetUpdated(userId: 5);
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        entity.UpdatedAt.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void SetUpdated_EstableceUpdatedBy()
    {
        var entity = ConcreteAuditable.Crear(userId: 1);
        entity.CallSetUpdated(userId: 7);

        entity.UpdatedBy.Should().Be(7);
    }

    [Fact]
    public void SetUpdated_UserIdNulo_UpdatedByEsNulo()
    {
        var entity = ConcreteAuditable.Crear(userId: 1);
        entity.CallSetUpdated(userId: null);

        entity.UpdatedBy.Should().BeNull();
    }

    // -----------------------------------------------------------
    // SetDeleted
    // -----------------------------------------------------------

    [Fact]
    public void SetDeleted_EstableceDeletedAt_Reciente()
    {
        var entity = ConcreteAuditable.Crear();

        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        entity.CallSetDeleted();
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        entity.DeletedAt.Should().NotBeNull();
        entity.DeletedAt!.Value.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void SetDeleted_IsDeleted_EsTrue()
    {
        var entity = ConcreteAuditable.Crear();
        entity.CallSetDeleted();

        entity.IsDeleted.Should().BeTrue();
    }

    // -----------------------------------------------------------
    // SetDeletedAt (soft-undelete)
    // -----------------------------------------------------------

    [Fact]
    public void SetDeletedAt_Nulo_LimpiaDeletedAt_IsDeletedFalse()
    {
        var entity = ConcreteAuditable.Crear();
        entity.CallSetDeleted();          // mark as deleted
        entity.CallSetDeletedAt(null);    // undelete

        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void SetDeletedAt_ConValor_EstableceDeletedAt()
    {
        var entity   = ConcreteAuditable.Crear();
        var timestamp = DateTimeOffset.UtcNow.AddDays(-5);

        entity.CallSetDeletedAt(timestamp);

        entity.DeletedAt.Should().Be(timestamp);
        entity.IsDeleted.Should().BeTrue();
    }

    // -----------------------------------------------------------
    // IsDeleted — computed property
    // -----------------------------------------------------------

    [Fact]
    public void IsDeleted_CuandoDeletedAtNulo_EsFalse()
    {
        var entity = ConcreteAuditable.Crear();
        entity.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void IsDeleted_CuandoDeletedAtTieneValor_EsTrue()
    {
        var entity = ConcreteAuditable.Crear();
        entity.CallSetDeleted();
        entity.IsDeleted.Should().BeTrue();
    }
}
