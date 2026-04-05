namespace Finalspace.Onigiri.MVVM
{
    public static class DispatcherFactory
    {
        private static readonly PassthroughDispatcherFactory DefaultFactory = new PassthroughDispatcherFactory();

        public static IDispatcherFactory Instance
        {
            get
            {
                if (_instance is null)
                    _instance = ServiceContainer.Default.GetService<IDispatcherFactory>();
                return _instance ?? DefaultFactory;
            }
        }
        private static IDispatcherFactory _instance = null;
    }
}
