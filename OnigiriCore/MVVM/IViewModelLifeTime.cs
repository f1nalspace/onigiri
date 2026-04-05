namespace Finalspace.Onigiri.MVVM;

public interface IViewModelLifeTime
{
    bool IsLoaded { get; }
    void Loaded(IView view);
    void Unloaded(IView view);
}
