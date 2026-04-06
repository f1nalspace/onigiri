using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.Services;

class AvaloniaFileDialogService : IFileDialogService
{
    private readonly Window _owner;

    public AvaloniaFileDialogService(Window owner) => _owner = owner;

    public async Task<string> ShowOpenFileDialogAsync(string title, string filter)
    {
        var files = await _owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            FileTypeFilter = ParseFilter(filter),
            AllowMultiple = false
        });
        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }

    public async Task<string> ShowSaveFileDialogAsync(string title, string filter, string defaultExt)
    {
        var file = await _owner.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            FileTypeChoices = ParseFilter(filter),
            DefaultExtension = defaultExt
        });
        return file?.Path.LocalPath;
    }

    private static List<FilePickerFileType> ParseFilter(string filter)
    {
        var result = new List<FilePickerFileType>();
        if (string.IsNullOrWhiteSpace(filter))
            return result;

        var parts = filter.Split('|');
        for (int i = 0; i + 1 < parts.Length; i += 2)
        {
            string name = parts[i].Trim();
            string[] patterns = parts[i + 1].Trim().Split(';')
                .Select(p => p.Trim())
                .ToArray();

            result.Add(new FilePickerFileType(name)
            {
                Patterns = patterns
            });
        }

        return result;
    }
}
