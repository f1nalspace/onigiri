using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.MVVM;
using Finalspace.Onigiri.Services;
using System.IO;

namespace Finalspace.Onigiri.ViewModels;

public class ConfigViewModel : ViewModelBase
{
    private IFolderDialogService FolderDialogService => GetService<IFolderDialogService>();

    public Config Config { get; }

    public DelegateCommand CmdClose { get; }
    public DelegateCommand CmdApply { get; }

    public DelegateCommand CmdAddSearchPath { get; }
    public DelegateCommand<SearchPath> CmdRemoveSearchPath { get; }

    private readonly Config _targetConfig;

    public ConfigViewModel(Config config)
    {
        _targetConfig = config;

        Config = new Config();
        Config.Assign(config);

        CmdClose = new DelegateCommand(Close);
        CmdApply = new DelegateCommand(Apply);
        CmdAddSearchPath = new DelegateCommand(AddSearchPath);
        CmdRemoveSearchPath = new DelegateCommand<SearchPath>(RemoveSearchPath, CanRemoveSearchPath);
    }

    private void Close()
    {
    }

    private void Apply()
    {
        _targetConfig.Assign(Config);
    }

    private async void AddSearchPath()
    {
        string newSearchPath = await FolderDialogService.ShowFolderDialogAsync("Select Search Path");
        if (string.IsNullOrEmpty(newSearchPath))
            return;

        string driveName = null;
        DriveInfo[] drives = DriveInfo.GetDrives();
        foreach (DriveInfo drive in drives)
        {
            if (string.Equals(drive.RootDirectory.FullName, newSearchPath))
                driveName = drive.VolumeLabel;
        }
        Config.SearchPaths.Add(new SearchPath() { Path = newSearchPath, DriveName = driveName });
    }

    private bool CanRemoveSearchPath(SearchPath searchPath) => searchPath is not null;
    private void RemoveSearchPath(SearchPath searchPath)
    {
        Config.SearchPaths.Remove(searchPath);
    }
}
