using Finalspace.Onigiri.MVVM;
using System;

namespace Finalspace.Onigiri.ViewModels;

public class TitlesViewModel : ViewModelBase, IDisposable
{
    // TODO: Phase 2 - full ViewModel migration

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
