using Finalspace.Onigiri.MVVM;
using Finalspace.Onigiri.Services;
using Finalspace.Onigiri.Security;
using Finalspace.Onigiri.Storage;
using System;

namespace Finalspace.Onigiri.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    public event Action CloseRequested;

    public MainViewModel()
    {
        // TODO: Phase 2 - full ViewModel migration
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
