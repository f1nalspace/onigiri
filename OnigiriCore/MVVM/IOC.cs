namespace Finalspace.Onigiri.MVVM;

public static class IOC
{
    public static IOCContainer Default => _default ??= new IOCContainer();
    private static IOCContainer _default = null;    
}
