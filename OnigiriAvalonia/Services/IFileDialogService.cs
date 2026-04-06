using System.Threading.Tasks;

namespace Finalspace.Onigiri.Services;

public interface IFileDialogService
{
    Task<string> ShowOpenFileDialogAsync(string title, string filter);
    Task<string> ShowSaveFileDialogAsync(string title, string filter, string defaultExt);
}
