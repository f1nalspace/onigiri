using System;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.MVVM;

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
