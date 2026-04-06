using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.Services;

class AvaloniaFolderDialogService : IFolderDialogService
{
    private readonly Window _owner;

    public AvaloniaFolderDialogService(Window owner) => _owner = owner;

    public async Task<string> ShowFolderDialogAsync(string title)
    {
        var folders = await _owner.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });
        return folders.Count > 0 ? folders[0].Path.LocalPath : null;
    }
}
