using Finalspace.Onigiri.MVVM;
using Finalspace.Onigiri.Services;
using Finalspace.Onigiri.Security;
using Finalspace.Onigiri.Storage;
using System;

namespace Finalspace.Onigiri.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    public event Action CloseRequested;

    public OnigiriService CoreService { get; private set; }
    public IOnigiriDialogService DlgService => GetService<IOnigiriDialogService>();

    public MainViewModel()
    {
        // TODO: Full ViewModel migration
        IUserService userService = OnigiriUserServiceFactory.Instance.Create();
        CoreService = new OnigiriService(userService);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
