using System;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.MVVM
{
    class PassthroughDispatcher : IDispatcher
    {
        public void Invoke(Action action)
            => action();
        public Task InvokeAsync(Func<Task> callback)
            => Task.Run(callback);

        public TResult Invoke<TResult>(Func<TResult> callback)
            => callback();
        public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> callback)
            => Task.Run(callback);
    }

    class PassthroughDispatcherFactory : IDispatcherFactory
    {
        private static readonly PassthroughDispatcher _instance = new PassthroughDispatcher();
        public IDispatcher GetDefault() => _instance;
    }

    static class DispatcherFactory
    {
        private static readonly PassthroughDispatcherFactory _defaultFactory = new PassthroughDispatcherFactory();

        public static IDispatcherFactory Instance
        {
            get
            {
                if (_instance is null)
                    _instance = IOC.Default.QueryService<IDispatcherFactory>();
                return _instance ?? _defaultFactory;
            }
        }
        private static IDispatcherFactory _instance = null;
    }
}
