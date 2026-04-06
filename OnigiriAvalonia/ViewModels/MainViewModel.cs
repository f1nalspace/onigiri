using Finalspace.Onigiri.MVVM;
using System;

namespace Finalspace.Onigiri.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    // TODO: Phase 2 - full ViewModel migration
    
    public event Action CloseRequested;
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
