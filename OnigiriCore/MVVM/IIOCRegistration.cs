namespace Finalspace.Onigiri.MVVM;

public interface IIOCRegistration
{
    void RegisterService(object instance);
    void RegisterService<T>(T instance);
}
