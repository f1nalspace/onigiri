using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Finalspace.Onigiri.MVVM;

public class ServiceContainer : IServiceContainer
{
    private static ServiceContainer _default = null;
    public static ServiceContainer Default => _default ??= new ServiceContainer();

    private readonly ConcurrentDictionary<string, object> _serviceKeyMap = new ConcurrentDictionary<string, object>();
    private readonly ConcurrentDictionary<Type, object> _serviceTypeMap = new ConcurrentDictionary<Type, object>();

    private static IReadOnlyCollection<Type> GetTypes(Type type)
    {
        Type[] interfaceTypes = type.GetInterfaces();
        List<Type> keys = interfaceTypes.ToList();
        keys.Insert(0, type);
        return keys;
    }

    private object GetServiceFromRootType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return _serviceTypeMap.GetValueOrDefault(type);
    }

    public T GetService<T>()
    {
        Type t = typeof(T);
        object instance = GetServiceFromRootType(t);
        if (instance is null)
            return default;
        return (T)instance;
    }

    public object GetService(Type type)
        => GetServiceFromRootType(type);

    public object GetService(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _serviceKeyMap.GetValueOrDefault(key);
    }

    public void RegisterService(object service)
    {
        ArgumentNullException.ThrowIfNull(service);
        Type type = service.GetType();
        if (!_serviceTypeMap.TryAdd(type, service))
            throw new ArgumentException($"Service by type '{type}' is already registered.", nameof(service));
        string key = type.FullName;
        if (!_serviceKeyMap.TryAdd(key, service))
            throw new ArgumentException($"Service by key '{key}' is already registered.", nameof(service));
    }

    public void RegisterService(string key, object service)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(service);
        Type type = service.GetType();
        if (!_serviceTypeMap.TryAdd(type, service))
            throw new ArgumentException($"Service by type '{type}' is already registered.", nameof(service));
        if (!_serviceKeyMap.TryAdd(key, service))
            throw new ArgumentException($"Service by key '{key}' is already registered.", nameof(service));
    }

    public void RegisterService<T>(T service)
    {
        ArgumentNullException.ThrowIfNull(service);
        Type type = typeof(T);
        if (!_serviceTypeMap.TryAdd(type, service))
            throw new ArgumentException($"Service by type '{type}' is already registered.", nameof(service));
        string key = type.FullName;
        if (!_serviceKeyMap.TryAdd(key, service))
            throw new ArgumentException($"Service by key '{key}' is already registered.", nameof(service));
    }

    public void UnregisterService(object service)
    {
        ArgumentNullException.ThrowIfNull(service);
        Type type = service.GetType();
        UnregisterService(type);
    }

    public void UnregisterService(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        IReadOnlyCollection<Type> types = GetTypes(type);
        foreach (Type t in types)
        {
            string key = t.FullName;
            _serviceTypeMap.TryRemove(t, out object _);
            _serviceKeyMap.TryRemove(key, out object _);
        }
    }
}
