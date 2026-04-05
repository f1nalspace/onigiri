using System;
using System.Collections.Concurrent;

namespace Finalspace.Onigiri.MVVM;

public class IOCContainer : IIOCQuery, IIOCRegistration
{
    private readonly ConcurrentDictionary<Type, object> _services = new ConcurrentDictionary<Type, object>();

    public T QueryService<T>()
    {
        Type t = typeof(T);
        if (_services.TryGetValue(t, out object instance))
            return (T)instance;
        return default;
    }

    public object QueryService(Type type)
    {
        if (_services.TryGetValue(type, out object instance))
            return instance;
        return default;
    }

    private void Register(Type key, object instance)
    {
        _services.AddOrUpdate(key, instance, (key, oldValue) => {
            if (oldValue is IDisposable disposable)
                disposable.Dispose();
            return instance;
        });
    }

    public void RegisterService<T>(T instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        Type key = typeof(T);

        Type[] interfaces = key.GetInterfaces();
        foreach (Type iface in interfaces)
            Register(iface, instance);

        Register(key, instance);
    }

    public void RegisterService(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        Type key = instance.GetType();
        Register(key, instance);
    }
}
