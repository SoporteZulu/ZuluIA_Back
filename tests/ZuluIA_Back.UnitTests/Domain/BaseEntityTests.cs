using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Domain;

public class BaseEntityTests
{
    // -----------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------

    private sealed class ConcreteEntity : BaseEntity
    {
        public ConcreteEntity(long id) { Id = id; }

        public void Raise(IDomainEvent domainEvent) => AddDomainEvent(domainEvent);
    }

    private sealed class TestDomainEvent : DomainEvent { }

    // -----------------------------------------------------------
    // Id
    // -----------------------------------------------------------

    [Fact]
    public void Id_SeEstableceCorrectamente()
    {
        var entity = new ConcreteEntity(42);
        entity.Id.Should().Be(42);
    }

    // -----------------------------------------------------------
    // DomainEvents — estado inicial
    // -----------------------------------------------------------

    [Fact]
    public void DomainEvents_InicialmenteVacia()
    {
        var entity = new ConcreteEntity(1);
        entity.DomainEvents.Should().BeEmpty();
    }

    // -----------------------------------------------------------
    // AddDomainEvent
    // -----------------------------------------------------------

    [Fact]
    public void AddDomainEvent_AgregaEventoAlColeccion()
    {
        var entity = new ConcreteEntity(1);
        var evt = new TestDomainEvent();

        entity.Raise(evt);

        entity.DomainEvents.Should().ContainSingle()
              .Which.Should().BeSameAs(evt);
    }

    [Fact]
    public void AddDomainEvent_MultipleEventos_TodosAgregados()
    {
        var entity = new ConcreteEntity(1);
        var evt1 = new TestDomainEvent();
        var evt2 = new TestDomainEvent();

        entity.Raise(evt1);
        entity.Raise(evt2);

        entity.DomainEvents.Should().HaveCount(2);
        entity.DomainEvents.Should().Contain(evt1);
        entity.DomainEvents.Should().Contain(evt2);
    }

    // -----------------------------------------------------------
    // ClearDomainEvents
    // -----------------------------------------------------------

    [Fact]
    public void ClearDomainEvents_LimpiaColeccion()
    {
        var entity = new ConcreteEntity(1);
        entity.Raise(new TestDomainEvent());
        entity.Raise(new TestDomainEvent());

        entity.ClearDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_ColeccionVacia_NoLanzaExcepcion()
    {
        var entity = new ConcreteEntity(1);

        var act = () => entity.ClearDomainEvents();

        act.Should().NotThrow();
    }

    // -----------------------------------------------------------
    // DomainEvents — es IReadOnlyCollection (no mutable externamente)
    // -----------------------------------------------------------

    [Fact]
    public void DomainEvents_DevuelveReadOnlyCollection()
    {
        var entity = new ConcreteEntity(1);

        entity.DomainEvents.Should().BeAssignableTo<IReadOnlyCollection<IDomainEvent>>();
    }

    // -----------------------------------------------------------
    // DomainEvent base class
    // -----------------------------------------------------------

    [Fact]
    public void DomainEvent_TieneEventIdUnico()
    {
        var evt1 = new TestDomainEvent();
        var evt2 = new TestDomainEvent();

        evt1.EventId.Should().NotBe(Guid.Empty);
        evt2.EventId.Should().NotBe(Guid.Empty);
        evt1.EventId.Should().NotBe(evt2.EventId);
    }

    [Fact]
    public void DomainEvent_TieneOccurredOnReciente()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var evt = new TestDomainEvent();
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        evt.OccurredOn.Should().BeAfter(before);
        evt.OccurredOn.Should().BeBefore(after);
    }
}
