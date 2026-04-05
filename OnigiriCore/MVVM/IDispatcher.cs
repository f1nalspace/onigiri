using System;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.MVVM;

public interface IDispatcher
{
    void Invoke(Action callback);
    Task InvokeAsync(Func<Task> callback);
    TResult Invoke<TResult>(Func<TResult> callback);
    Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> callback);
}