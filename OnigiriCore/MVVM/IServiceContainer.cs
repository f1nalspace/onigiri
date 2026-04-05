using System;

namespace Finalspace.Onigiri.MVVM;

public interface IServiceContainer
{
    T GetService<T>();
    object GetService(Type type);
    object GetService(string key);
    void RegisterService(object service);
    void RegisterService(string key, object service);
    void RegisterService<T>(T service);
    void UnregisterService(object service);
    void UnregisterService(Type type);
}
