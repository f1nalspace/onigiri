using System;

namespace Finalspace.Onigiri.MVVM;

public interface IIOCQuery
{
    T QueryService<T>();
    object QueryService(Type type);
}
