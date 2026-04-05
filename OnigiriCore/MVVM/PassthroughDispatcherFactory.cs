namespace Finalspace.Onigiri.MVVM;

class PassthroughDispatcherFactory : IDispatcherFactory
{
    private static readonly PassthroughDispatcher _instance = new PassthroughDispatcher();
    public IDispatcher GetDefault() => _instance;
}
