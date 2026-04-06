using System.Threading.Tasks;

namespace Finalspace.Onigiri.Services;

public interface IFolderDialogService
{
    Task<string> ShowFolderDialogAsync(string title = null);
}
