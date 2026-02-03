using Order.Core.BaseModels;

namespace Order.Application.Interfaces;

public interface IRequestIdentityAccessor
{
    RequestIdentity Get();
}