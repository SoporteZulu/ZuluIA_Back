using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Common.Interfaces;

public interface IDomainEventNotification : IDomainEvent, INotification { }