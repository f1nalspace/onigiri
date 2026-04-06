# Onigiri Avalonia Migration Plan

## Context

The Onigiri WPF project (`Onigiri/`) needs to be migrated to a new AvaloniaUI project (`OnigiriAvalonia/`). The core infrastructure is already in place:

- **OnigiriCore** (`net10.0`): Fully migrated with custom MVVM layer (`Finalspace.Onigiri.MVVM`) built on `CommunityToolkit.Mvvm`, Serilog logging, and all 20 domain models converted
- **OnigiriPlatform** (`net10.0`): Cross-platform with Win32 and POSIX implementations
- **OnigiriTests** (`net10.0`): Migrated and passing
- **OnigiriConsole** (`net10.0`): Migrated with Serilog

The WPF `Onigiri/` project remains on `net9.0-windows` with DevExpress MVVM references, log4net, MaterialDesignThemes, and VirtualizingWrapPanel. This plan creates a **new** `OnigiriAvalonia/` project alongside it.

**Key goals:**
- Use `Finalspace.Onigiri.MVVM` classes (ViewModelBase, BindableBase, DelegateCommand, ServiceContainer) from OnigiriCore
- Replace log4net with Serilog
- Replace MaterialDesignThemes with Avalonia Fluent theme + custom styles
- Replace WPF-specific APIs (ICollectionView, DependencyProperty, Frame/Page, etc.) with Avalonia equivalents
- Use ImmutableObservableCollection as Backing Store for all collections
- Cross-platform support (Windows, Linux)

---

## Phase 1: Project Setup & Bootstrap

### 1A. Create Project File (`OnigiriAvalonia/OnigiriAvalonia.csproj`)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Platforms>x64</Platforms>
    <OutputType>WinExe</OutputType>
    <RootNamespace>Finalspace.Onigiri</RootNamespace>
    <Nullable>disable</Nullable>
    <ApplicationIcon>..\Onigiri\Resources\onigiri.ico</ApplicationIcon>
    <BuiltInComInteropSupport>true</BuiltInComInteropSupport>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
  </PropertyGroup>

  <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Debug|x64'">
    <OutputPath>..\Build\OnigiriAvalonia\x64-Debug\</OutputPath>
  </PropertyGroup>
  <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Release|x64'">
    <OutputPath>..\Build\OnigiriAvalonia\x64-Release\</OutputPath>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\OnigiriCore\OnigiriCore.csproj" />
    <ProjectReference Include="..\OnigiriPlatform\OnigiriPlatform.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.3.*" />
    <PackageReference Include="Avalonia.Desktop" Version="11.3.*" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.3.*" />
    <PackageReference Include="Avalonia.Fonts.Inter" Version="11.3.*" />
    <PackageReference Include="Avalonia.Xaml.Behaviors" Version="11.3.*" />
    <PackageReference Include="Avalonia.Diagnostics" Version="11.3.*" Condition="'$(Configuration)' == 'Debug'" />
    <PackageReference Include="Serilog" Version="4.3.*" />
    <PackageReference Include="Serilog.Sinks.Console" Version="6.1.*" />
    <PackageReference Include="Serilog.Sinks.Debug" Version="3.0.*" />
    <PackageReference Include="Serilog.Sinks.File" Version="6.0.*" />
  </ItemGroup>

  <ItemGroup>
    <AvaloniaResource Include="Resources\**" />
  </ItemGroup>
</Project>
```

**NuGet package mapping from WPF:**

| WPF Package | Avalonia Replacement |
|---|---|
| `MaterialDesignThemes` 5.2.1 | `Avalonia.Themes.Fluent` (built-in) |
| `VirtualizingWrapPanel` 1.5.0 | Avalonia `ItemsRepeater` + `WrapLayout` |
| `System.Management` 6.0.0 | Remove (dark mode via Avalonia `ActualThemeVariant`) |
| `log4net` (local DLL) | `Serilog` + sinks |
| DevExpress MVVM (in WPF project) | `Finalspace.Onigiri.MVVM` from OnigiriCore |

### 1B. Application Entry Point (`OnigiriAvalonia/Program.cs`)

```csharp
using Avalonia;
using System;

namespace Finalspace.Onigiri;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
```

### 1C. App Manifest (`OnigiriAvalonia/app.manifest`)

Standard Avalonia app manifest with DPI awareness and long path support.

### 1D. Application Definition (`OnigiriAvalonia/App.axaml`)

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="Finalspace.Onigiri.App"
             RequestedThemeVariant="Default">
  <Application.Styles>
    <FluentTheme />
    <StyleInclude Source="avares://OnigiriAvalonia/Styles/Controls.axaml" />
    <StyleInclude Source="avares://OnigiriAvalonia/Styles/Onigiri.axaml" />
  </Application.Styles>
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <ResourceInclude Source="avares://OnigiriAvalonia/Styles/LightColors.axaml" />
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>
```

### 1E. Application Code-Behind (`OnigiriAvalonia/App.axaml.cs`)

```csharp
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Finalspace.Onigiri.MVVM;
using Finalspace.Onigiri.Services;
using Finalspace.Onigiri.ViewModels;
using Finalspace.Onigiri.Views;
using Serilog;

namespace Finalspace.Onigiri;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Serilog setup (replaces log4net XmlConfigurator.Configure())
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(OnigiriPaths.PersistentPath, "log_app.txt"),
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Service registration (replaces DevExpress ServiceContainer.Default)
        ServiceContainer.Default.RegisterService(new DefaultProcessStarterService());
        ServiceContainer.Default.RegisterService(new AvaloniaThemeManagerService(this));
        ServiceContainer.Default.RegisterService(new AvaloniaDarkModeDetectionService(this));

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            ServiceContainer.Default.RegisterService<IOnigiriDialogService>(
                new DefaultOnigiriDialogService(mainWindow));
            var mainViewModel = new MainViewModel();
            mainViewModel.CloseRequested += () => mainWindow.Close();
            mainWindow.DataContext = mainViewModel;
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
```

### 1F. Add to Solution (`Onigiri.sln`)

Add `OnigiriAvalonia/OnigiriAvalonia.csproj` with `Debug|x64` and `Release|x64` configurations.

### 1G. Resources

Copy all image files from `Onigiri/Resources/` to `OnigiriAvalonia/Resources/`. The `<AvaloniaResource Include="Resources\**" />` glob picks them all up. URI format changes from `"/Resources/icon.png"` to `"avares://OnigiriAvalonia/Resources/icon.png"`.

**Full resource list** (~59 files): `add_32x32.png`, `anni.png`, `anni_delete.png`, `anni_false.png`, `anni_delete_false.png`, `close_16x16.png`, `close_32x32.png`, `customization_16x16.png`, `customization_32x32.png`, `darkmode_trigger_32x32.png`, `delete.png`, `delete_false.png`, `download_16x16.png`, `download_32x32.png`, `download_to_local_store_32x32.png`, `export_16x16.png`, `export_32x32.png`, `final.png`, `final_delete.png`, `final_delete_false.png`, `final_false.png`, `filter_32x32.png`, `find_32x32.png`, `folder_32x32.png`, `info_16x16.png`, `info_32x32.png`, `mark.png`, `mark_false.png`, `moon_32x32.png`, `new_16x16.png`, `new_32x32.png`, `next_32x32.png`, `next_tag_32x32.png`, `nopicture.png`, `onigiri.ico`, `open2_16x16.png`, `open2_32x32.png`, `p221098.jpg`, `prev_32x32.png`, `prev_tag_32x32.png`, `rebuild_database_32x32.png`, `refresh_16x16.png`, `refresh_32x32.png`, `show_32x32.png`, `star0.png`, `star1.png`, `star2.png`, `suggestion_32x32.png`, `sun_32x32.png`, `testicon_16x16.png`, `testicon_32x32.png`, `titles_32x32.png`, `v_576p.png`, `v_720p.png`, `v_1080p.png`, `v_1440p.png`, `v_2160p.png`, `watch.png`, `watch_false.png`

### 1H. Validation

```bash
dotnet build OnigiriAvalonia/OnigiriAvalonia.csproj -c Debug -p:Platform=x64
```

Must compile with Avalonia packages resolved. No WPF, no DevExpress, no log4net references.

---

## Phase 2: ViewModel Migration

ViewModels are ported from `Onigiri/ViewModels/` to `OnigiriAvalonia/ViewModels/`. The custom MVVM layer in OnigiriCore mimics the DevExpress API, so most code needs only namespace changes. The major exceptions are `ICollectionView` usage and `System.Windows.Visibility`.

### 2A. Global Changes Across All ViewModels

| WPF Pattern | Avalonia Replacement |
|---|---|
| `using DevExpress.Mvvm;` | `using Finalspace.Onigiri.MVVM;` |
| `using System.Windows;` | Remove (no `Visibility` enum needed) |
| `using System.Windows.Data;` | Remove (no `ICollectionView`/`CollectionViewSource`) |
| `private static readonly ILog log = LogManager.GetLogger(...)` | `private static readonly Serilog.ILogger log = Serilog.Log.ForContext<ClassName>()` |
| `System.Windows.Visibility` property | `bool` property with `IsVisible` binding in AXAML |
| `ICollectionView` | Filtered `ImmutableObservableCollection<T>` |
| `ListCollectionView` + `CustomSort` + `Filter` | LINQ sort + predicate filter applied to backing list |
| `CollectionViewSource.GetDefaultView()` | Direct collection management |
| `BindingOperations.EnableCollectionSynchronization()` | `Dispatcher.UIThread.InvokeAsync()` for collection mutations |
| `ISaveFileDialogService` / `IOpenFileDialogService` (DevExpress) | Custom `IFileDialogService` wrapping Avalonia `StorageProvider` |
| `IFolderBrowserDialogService` (DevExpress) | Custom `IFolderDialogService` wrapping Avalonia `StorageProvider` |
| `IDispatcherService` (DevExpress) | Avalonia `Dispatcher.UIThread` directly, or custom `IDispatcher` from OnigiriCore |

### 2B. ICollectionView Replacement Strategy

This is the most complex change. Avalonia has no `ICollectionView`, `ListCollectionView`, or `CollectionViewSource`.

**Pattern:** Replace `List<T>` + `ICollectionView` with `List<T>` (all items) + `ObservableCollection<T>` (filtered/sorted view):

```csharp
// BEFORE (WPF):
private readonly List<Anime> _animes;
public ICollectionView AnimesView { get; }
// Constructor:
AnimesView = new ListCollectionView(_animes) { CustomSort = new AnimeSorter(), Filter = AnimesViewFilter };
// Refresh:
AnimesView.Refresh();

// AFTER (Avalonia):
private readonly List<Anime> _allAnimes = new(4096);
public ObservableCollection<Anime> Animes { get; } = new();

private void RefreshAnimes()
{
    var filtered = _allAnimes.Where(AnimesViewFilter);
    var sorted = SortAnimes(filtered);
    Animes.Clear();
    foreach (var anime in sorted)
        Animes.Add(anime);
    RaisePropertyChanged(nameof(VisibleAnimeCount));
    RaisePropertyChanged(nameof(TotalAnimeCount));
}
```

The existing `AnimeSorter` (`OnigiriCore/Helper/AnimeSorter.cs`) implements `IComparer` and can be reused directly with LINQ's `OrderBy` or `List.Sort()`. Same for `TitleSorter` and `IssuesSorter`.

**Sort helper method:**
```csharp
private IEnumerable<Anime> SortAnimes(IEnumerable<Anime> source)
{
    var sorter = new AnimeSorter
    {
        FirstSortKey = FirstSortKey?.Value ?? AnimeSortKey.None,
        FirstSortIsDesc = IsFirstSortOrderDesc,
        SecondSortKey = SecondSortKey?.Value ?? AnimeSortKey.None,
        SecondSortIsDesc = IsSecondSortOrderDesc,
    };
    var list = source.ToList();
    list.Sort((a, b) => sorter.Compare(a, b));
    return list;
}
```

### 2C. File-by-File ViewModel Migration

#### `MainViewModel.cs` (~930 lines) - **Medium effort**

Source: `Onigiri/ViewModels/MainViewModel.cs`
Target: `OnigiriAvalonia/ViewModels/MainViewModel.cs`

**Changes required:**

1. **Namespace imports:**
   - Remove: `using DevExpress.Mvvm;`, `using System.Windows;`, `using System.Windows.Data;`, `using log4net;`
   - Add: `using Serilog;` (already used as `Log.ForContext<MainViewModel>()`)
   - Keep: `using Finalspace.Onigiri.MVVM;` (already present)

2. **Logging:**
   - `private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);`
   - becomes: `private static readonly ILogger log = Log.ForContext<MainViewModel>();`

3. **Visibility properties:**
   - `public Visibility LoadingWindowVisibility => (!IsNotLoading) ? Visibility.Visible : Visibility.Collapsed;`
   - becomes: `public bool IsLoading => !IsNotLoading;` (bind `IsVisible` in AXAML)

4. **ICollectionView removal:**
   - `AnimesView` (`ICollectionView` backed by `List<Anime>` + `ListCollectionView` + `AnimeSorter` + `AnimesViewFilter`)
     - Replace with `ObservableCollection<Anime> Animes` + `RefreshAnimes()` method
     - Move `AnimesViewFilter` predicate to work on `Anime` directly (already does, the `object item` cast is trivial)
     - Reuse `AnimeSorter` from `OnigiriCore/Helper/AnimeSorter.cs` via LINQ
   - `UsersView` (`ICollectionView` backed by `List<User>`)
     - Replace with `ObservableCollection<User> Users`
   - Remove `BindingOperations.EnableCollectionSynchronization(_animes, _animes)` calls
   - All `AnimesView.Refresh()` calls become `RefreshAnimes()`
   - All `UsersView.Refresh()` calls become direct collection update

5. **Dialog services:**
   - `ISaveFileDialogService` (line 356) - Replace with custom `IFileDialogService` using Avalonia `StorageProvider`
   - `IOpenFileDialogService` (line 404) - Same
   - `IDispatcherService` (line 34) - Replace with `Avalonia.Threading.Dispatcher.UIThread`

6. **Everything else stays the same:**
   - All `GetValue<T>()`/`SetValue()` property patterns (unchanged)
   - All `DelegateCommand`/`DelegateCommand<T>` declarations (unchanged)
   - All `GetService<T>()` calls (unchanged, uses custom `ServiceContainer`)
   - All `RaisePropertyChanged()` calls (unchanged)
   - All business logic: `RefreshAsync()`, `UpdateAsync()`, `ExportAsync()`, `ImportAsync()` (unchanged)
   - Filter logic: `AnimesViewFilter()` (unchanged, just rename parameter from `object item` to `Anime anime`)
   - Sort logic: `UpdateSort()` (rewritten to use `AnimeSorter` directly instead of via `ListCollectionView.CustomSort`)
   - All action commands: `ToggleMarked`, `ToggleWatched`, `ToggleDeletion` (unchanged)
   - Theme management: `ChangedTheme`, `SetTheme`, `OnDarkModeChanged` (unchanged)
   - `Dispose` pattern (unchanged)

#### `TitlesViewModel.cs` - **Medium effort**

Source: `Onigiri/ViewModels/TitlesViewModel.cs`
Target: `OnigiriAvalonia/ViewModels/TitlesViewModel.cs`

**Changes required:**

1. Replace `using DevExpress.Mvvm;` with `using Finalspace.Onigiri.MVVM;`
2. Remove `using System.Windows;`, `using System.Windows.Data;`
3. `Visibility LoadingWindowVisibility` -> `bool IsLoading`
4. `ICollectionView TitlesView` -> `ObservableCollection<Title> FilteredTitles`
5. `CollectionViewSource.GetDefaultView(_titles)` -> direct collection management
6. `ListCollectionView.CustomSort = new TitleSorter()` -> LINQ sort using `TitleSorter` (`OnigiriCore/Helper/TitleSorter.cs`)
7. `TitlesView.Filter = TitleFilter` -> apply filter in `RefreshTitles()` method
8. `TitlesView.Refresh()` -> `RefreshTitles()`
9. `IDispatcherService` -> `Avalonia.Threading.Dispatcher.UIThread`
10. `BackgroundWorker` -> `Task.Run()` (modernize)
11. Timer-based filter debouncing can remain the same

#### `IssuesViewModel.cs` - **Medium effort**

Source: `Onigiri/ViewModels/IssuesViewModel.cs`
Target: `OnigiriAvalonia/ViewModels/IssuesViewModel.cs`

**Changes required:**

1. Replace `using DevExpress.Mvvm;` with `using Finalspace.Onigiri.MVVM;`
2. Remove `using System.Windows.Data;`
3. `ICollectionView IssuesView` -> `ObservableCollection<Issue> Issues`
4. `CollectionViewSource.GetDefaultView(_issues)` -> direct collection
5. `ListCollectionView.CustomSort = new IssuesSorter()` -> LINQ sort using `IssuesSorter` (`OnigiriCore/Helper/IssuesSorter.cs`)
6. `SetIssues()` -> populate `ObservableCollection` directly

#### `ConfigViewModel.cs` - **Low effort**

Source: `Onigiri/ViewModels/ConfigViewModel.cs`
Target: `OnigiriAvalonia/ViewModels/ConfigViewModel.cs`

**Changes required:**

1. Replace `using DevExpress.Mvvm;` with `using Finalspace.Onigiri.MVVM;`
2. `IFolderBrowserDialogService` -> custom `IFolderDialogService` wrapping Avalonia `StorageProvider.OpenFolderPickerAsync()`
3. All command and property patterns stay the same

#### Trivial ViewModels (namespace change only)

These files need only `using DevExpress.Mvvm;` -> `using Finalspace.Onigiri.MVVM;`:

| File | Lines | Notes |
|---|---|---|
| `AnimeUserViewModel.cs` | ~15 | Simple tuple class |
| `CategoryItemViewModel.cs` | ~15 | BindableBase properties |
| `SortItemViewModel.cs` | ~15 | BindableBase properties |
| `NameItemViewModel.cs` | ~10 | BindableBase properties |
| `WatchStateItemViewModel.cs` | ~15 | Extends NameItemViewModel |
| `MainTheme.cs` | ~5 | Enum, no changes at all |
| `TestMainViewModel.cs` | ~30 | Design-time data |
| `TestAnimeViewModel.cs` | ~50 | Design-time data |

### 2D. New Service Interfaces

Create interfaces for Avalonia-specific services that replace DevExpress dialog services:

**`OnigiriAvalonia/Services/IFileDialogService.cs`:**
```csharp
public interface IFileDialogService
{
    Task<string> ShowOpenFileDialogAsync(string title, string filter);
    Task<string> ShowSaveFileDialogAsync(string title, string filter, string defaultExt);
}
```

**`OnigiriAvalonia/Services/IFolderDialogService.cs`:**
```csharp
public interface IFolderDialogService
{
    Task<string> ShowFolderDialogAsync(string title = null);
}
```

These wrap `Window.StorageProvider.OpenFilePickerAsync()`, `SaveFilePickerAsync()`, and `OpenFolderPickerAsync()`.

---

## Phase 3: Service Layer Migration

Port from `Onigiri/Services/` to `OnigiriAvalonia/Services/`. Service interfaces remain the same; implementations adapt to Avalonia APIs.

### 3A. Service Interface Porting

These interfaces are copied as-is (already framework-agnostic):

| Interface | File | Notes |
|---|---|---|
| `IOnigiriDialogService` | `Services/IOnigiriDialogService.cs` | 3 methods: ShowTitlesDialog, ShowIssuesDialog, ShowConfigurationDialog |
| `IProcessStarterService` | `Services/IProcessStarterService.cs` | 1 method: Start(executable, args) |
| `IThemeManagerService` | `Services/IThemeManagerService.cs` | Property: CurrentTheme, Method: ChangeTheme |
| `IDarkModeDetectionService` | `Services/IDarkModeDetectionService.cs` | Event: DarkModeChanged, Property: IsDarkMode |

### 3B. Service Implementation Changes

#### `DefaultOnigiriDialogService` - **Medium rewrite**

Source: `Onigiri/Services/DefaultOnigiriDialogService.cs`

WPF pattern:
```csharp
TitlesWindow window = new TitlesWindow();
window.Owner = _owner;
window.DataContext = titlesViewModel;
bool? result = window.ShowDialog();  // synchronous
```

Avalonia pattern:
```csharp
var window = new TitlesWindow { DataContext = titlesViewModel };
var result = await window.ShowDialog<bool?>(_owner);  // async
```

**Key difference:** Avalonia dialogs are **async** (`ShowDialog<TResult>` returns `Task<TResult>`). The interface methods must become async:
```csharp
public interface IOnigiriDialogService
{
    Task<bool> ShowTitlesDialogAsync(TitlesViewModel titlesViewModel);
    Task ShowIssuesDialogAsync(IssuesViewModel issuesViewModel);
    Task<bool> ShowConfigurationDialogAsync(ConfigViewModel configViewModel);
}
```

This requires updating callers in `MainViewModel` (`ShowSettingsDialog`, `ShowTitlesDialog`, `ShowIssuesDialog`) to be async. The command handlers (`CmdSettings`, `CmdTitles`, `CmdIssues`) should use `AsyncCommand` instead of `DelegateCommand` for these.

#### `DefaultProcessStarterService` - **Minor change**

Add `UseShellExecute = true` for cross-platform URL opening:
```csharp
public void Start(string executable, params string[] args)
{
    Process.Start(new ProcessStartInfo(executable, string.Join(" ", args))
    {
        UseShellExecute = true
    });
}
```

#### `AvaloniaThemeManagerService` (replaces `DefaultThemeManagerService`) - **Full rewrite**

The WPF version uses `MaterialDesignThemes.Wpf.PaletteHelper`, `ResourceDictionary` swapping, and `WindowHelper.SetTheme()` P/Invoke. All of this is replaced by Avalonia's built-in theme system:

```csharp
class AvaloniaThemeManagerService : IThemeManagerService
{
    private readonly Application _app;

    public AvaloniaThemeManagerService(Application app) => _app = app;

    public MainTheme CurrentTheme { get; private set; }

    public void ChangeTheme(MainTheme theme)
    {
        CurrentTheme = theme;
        _app.RequestedThemeVariant = theme switch
        {
            MainTheme.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Light,
        };
        // Swap custom color resources
        SwapColorDictionary(theme);
    }

    private void SwapColorDictionary(MainTheme theme)
    {
        // Find and replace the color resource dictionary in Application.Resources
        string source = theme == MainTheme.Dark
            ? "avares://OnigiriAvalonia/Styles/DarkColors.axaml"
            : "avares://OnigiriAvalonia/Styles/LightColors.axaml";
        // Replace the MergedDictionary at index 0
        var dict = _app.Resources.MergedDictionaries;
        if (dict.Count > 0)
            dict[0] = new ResourceInclude(new Uri(source)) { Source = new Uri(source) };
    }
}
```

**Eliminated WPF-specific code:**
- `PaletteHelper` (MaterialDesignThemes)
- `Application.Current.Resources.MergedDictionaries[2]` indexing
- `WindowHelper.SetTheme()` (dwmapi.dll P/Invoke)
- Frame reload hack (`mainFrame.GoBack()` / `mainFrame.Navigate()`)

#### `AvaloniaDarkModeDetectionService` (replaces `Win32DarkModeDetectionService`) - **Full rewrite**

The WPF version uses Windows Registry + WMI `ManagementEventWatcher`. Avalonia provides cross-platform theme detection:

```csharp
class AvaloniaDarkModeDetectionService : IDarkModeDetectionService
{
    private readonly Application _app;

    public AvaloniaDarkModeDetectionService(Application app)
    {
        _app = app;
        _app.ActualThemeVariantChanged += OnThemeVariantChanged;
    }

    public event EventHandler<bool> DarkModeChanged;

    public bool IsDarkMode => _app.ActualThemeVariant == ThemeVariant.Dark;

    private void OnThemeVariantChanged(object sender, EventArgs e)
    {
        DarkModeChanged?.Invoke(this, IsDarkMode);
    }
}
```

**Eliminated WPF-specific code:**
- `Microsoft.Win32.Registry` access
- `System.Management.ManagementEventWatcher` (WMI)
- `System.Security.Principal.WindowsIdentity`
- `System.Management` NuGet package dependency

### 3C. New File Dialog Service Implementation

**`OnigiriAvalonia/Services/AvaloniaFileDialogService.cs`:**
```csharp
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

    private static List<FilePickerFileType> ParseFilter(string filter) { /* parse "Name|*.ext" format */ }
}
```

**`OnigiriAvalonia/Services/AvaloniaFolderDialogService.cs`:**
```csharp
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
```

---

## Phase 4: View Migration (XAML -> AXAML)

### 4A. Global XAML Translation Rules

| WPF | Avalonia |
|---|---|
| `xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"` | `xmlns="https://github.com/avaloniaui"` |
| `clr-namespace:Foo` | `using:Foo` |
| `Visibility="Collapsed"` / `Visibility="Visible"` | `IsVisible="False"` / `IsVisible="True"` |
| `Visibility="{Binding ..., Converter={BoolToVis}}"` | `IsVisible="{Binding ...}"` (direct bool binding) |
| `Page` + `Frame` navigation | `UserControl` + `ContentControl` with content switching |
| `StatusBar` + `StatusBarItem` | Custom `DockPanel` or `Grid` layout |
| `DependencyProperty` | `StyledProperty<T>` via `AvaloniaProperty.Register<>()` |
| `Style TargetType="Button"` | `<Style Selector="Button">` |
| `Style BasedOn="{StaticResource key}"` | Avalonia styles cascade; use `<Style Selector="Button.myClass">` |
| `pack://application:,,,/path` | `avares://OnigiriAvalonia/path` |
| `Source="/Resources/icon.png"` | `Source="avares://OnigiriAvalonia/Resources/icon.png"` |
| `dxmvvm:EventToCommand` | `<Interaction.Behaviors><EventTriggerBehavior>` from `Avalonia.Xaml.Behaviors` |
| `dxmvvm:Interaction.Behaviors` | `<Interaction.Behaviors>` from `Avalonia.Xaml.Behaviors` |
| `dxmvvm:DispatcherService` | Remove (use `Dispatcher.UIThread` directly) |
| `dxmvvm:OpenFileDialogService` / `SaveFileDialogService` | Remove (use `IFileDialogService` via ServiceContainer) |
| `dxmvvm:BooleanToVisibilityConverter` | Direct `IsVisible` bool binding |
| `dxmvvm:StringToVisibilityConverter` | Custom converter or `StringNotEmpty` -> `IsVisible` binding |
| `RelativeSource AncestorType=Window` | `$parent[Window]` in binding path, or named element `#elementName` |
| `RelativeSource AncestorType=Page` | `$parent[UserControl]` (since Page becomes UserControl) |
| `{d:DesignInstance ...}` | `d:DataContext` with `Design.DataContext` |
| `IsAsync=True` (Image binding) | Avalonia image loading is async by default; use `Task<Bitmap>` property |
| `UpdateSourceTrigger=PropertyChanged` | Default in Avalonia for most controls; remove explicitly |
| `IsSynchronizedWithCurrentItem="True"` | Remove (no concept in Avalonia) |
| `BitmapImage` | `Avalonia.Media.Imaging.Bitmap` |
| `MultiBinding` | `MultiBinding` (Avalonia supports this) |
| `MarkupExtension`-based converters | Declare as `<StaticResource>` (Avalonia doesn't support inline MarkupExtension converters the same way) |

### 4B. View-by-View Migration

#### `MainWindow.xaml` -> `MainWindow.axaml` - **High effort**

Source: `Onigiri/Views/MainWindow.xaml` + `MainWindow.xaml.cs`

**Structure changes:**

1. **Remove Frame/Page navigation** - Replace `<Frame Source="CardListPage.xaml" />` with `<ContentControl Content="{Binding CardListContent}">` or inline the `CardListView` UserControl:
   ```xml
   <Grid>
       <controls:LoadingBarControl ... />
       <views:CardListView DataContext="{Binding}" />
   </Grid>
   ```

2. **Replace StatusBar** - Avalonia has no `StatusBar` control. Use a styled `DockPanel`:
   ```xml
   <Border DockPanel.Dock="Bottom" Classes="statusBar">
       <DockPanel>
           <TextBlock Text="Number of animes" />
           <Separator />
           <TextBlock Text="{Binding VisibleAnimeCount}" />
           <TextBlock Text="of" />
           <TextBlock Text="{Binding TotalAnimeCount}" />
           <Panel HorizontalAlignment="Stretch" /> <!-- spacer -->
           <TextBlock Text="{Binding LoadingSubject}" />
           <Separator />
           <TextBlock Text="{Binding LoadingHeader}" />
       </DockPanel>
   </Border>
   ```

3. **Replace DevExpress behaviors:**
   ```xml
   <!-- BEFORE (WPF): -->
   <dxmvvm:Interaction.Behaviors>
       <dxmvvm:DispatcherService />
       <dxmvvm:OpenFileDialogService />
       <dxmvvm:SaveFileDialogService />
       <dxmvvm:EventToCommand EventName="Loaded" Command="{Binding OnLoadedCommand}" />
   </dxmvvm:Interaction.Behaviors>

   <!-- AFTER (Avalonia): -->
   <Interaction.Behaviors>
       <EventTriggerBehavior EventName="Loaded" SourceObject="{Binding $self}">
           <InvokeCommandAction Command="{Binding OnLoadedCommand}" />
       </EventTriggerBehavior>
   </Interaction.Behaviors>
   ```

4. **Menu system** - Avalonia `Menu` and `MenuItem` are similar but:
   - `IsCheckable` works the same
   - `IsChecked` binding works the same
   - Icon images use `avares://` URIs
   - MenuItem `ItemsSource` with `DataTemplate` works similarly
   - Replace `{x:Static viewmodels:MainTheme.Automatic}` with Avalonia's `x:Static` (same syntax)

5. **Sorting/Filtering GroupBoxes** - Direct port with:
   - `Visibility="{Binding ShowSorting, Converter={BoolToVis}}"` -> `IsVisible="{Binding ShowSorting}"`
   - `dxmvvm:BooleanToVisibilityConverter` removed (native bool binding)
   - `dxmvvm:StringToVisibilityConverter` -> custom converter or watermark

6. **Code-behind simplification:**
   - Remove `ServiceContainer.Default.RegisterService(new DefaultOnigiriDialogService(this))` (moved to `App.axaml.cs`)
   - Remove `DataContext = new MainViewModel()` (moved to `App.axaml.cs`)
   - Remove `Window_Loaded` + `WindowHelper.SetTheme()` (Avalonia handles this natively)
   - Remove all Frame DataContext synchronization (`UpdateFrameDataContext`, `mainFrame_LoadCompleted`, etc.)
   - Remove `mainFrame_ContentRendered` resource dictionary merging hack

#### `CardListPage.xaml` -> `CardListView.axaml` - **High effort**

Source: `Onigiri/Views/CardListPage.xaml`

Convert from `Page` to `UserControl`. The main challenge is replacing `VirtualizingWrapPanel`.

**Option A: ItemsRepeater + WrapLayout** (recommended for virtualization):
```xml
<ScrollViewer>
    <ItemsRepeater ItemsSource="{Binding Animes}">
        <ItemsRepeater.Layout>
            <WrapLayout Orientation="Horizontal"
                        HorizontalSpacing="10"
                        VerticalSpacing="10" />
        </ItemsRepeater.Layout>
        <ItemsRepeater.ItemTemplate>
            <DataTemplate>
                <controls:AnimeCardControl Width="800" Height="400" />
            </DataTemplate>
        </ItemsRepeater.ItemTemplate>
    </ItemsRepeater>
</ScrollViewer>
```

Note: Avalonia's `ItemsRepeater` with `WrapLayout` provides virtualization. The `WrapLayout` is available in Avalonia 11.x. If `WrapLayout` doesn't meet performance needs, consider `UniformGridLayout` as an alternative.

**Option B: ListBox with WrapPanel** (simpler, less performant):
```xml
<ListBox ItemsSource="{Binding Animes}" SelectionMode="Single">
    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <WrapPanel Orientation="Horizontal" />
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>
    <ListBox.ItemTemplate>
        <DataTemplate>
            <controls:AnimeCardControl Width="800" Height="400" />
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

Start with Option A. The WPF version used `VirtualizingWrapPanel` with page-based caching for performance with large lists.

#### `ConfigWindow.xaml` -> `ConfigWindow.axaml` - **Low effort**

Source: `Onigiri/Views/ConfigWindow.xaml`

Straightforward conversion:
- `Window` stays `Window`
- `SizeToContent="Height"` -> `SizeToContent="Height"`
- `WindowStartupLocation="CenterOwner"` -> `WindowStartupLocation="CenterOwner"`
- `ButtonHelper.DialogResult` attached property -> Replace with command-based dialog closing:
  ```csharp
  // In ConfigViewModel or code-behind:
  private void Close() => (view as Window)?.Close(false);
  private void Apply() { _targetConfig.Assign(Config); (view as Window)?.Close(true); }
  ```
  Or use Avalonia's approach: bind button click to close the window via command with `Window.Close(result)`.
- `IFolderBrowserDialogService` -> `IFolderDialogService` (async)

#### `DetailsWindow.xaml` -> `DetailsWindow.axaml` - **Trivial**

Minimal content (TextBlock + Image + TabControl stub). Direct port with namespace changes.

#### `TitlesWindow.xaml` -> `TitlesWindow.axaml` - **Medium effort**

Source: `Onigiri/Views/TitlesWindow.xaml`

- `ListView` with `GridView` columns -> Avalonia `DataGrid` or `ListBox` with grid layout:
  ```xml
  <DataGrid ItemsSource="{Binding FilteredTitles}"
            AutoGenerateColumns="False"
            IsReadOnly="True"
            SelectedItem="{Binding SelectedTitle}">
      <DataGrid.Columns>
          <DataGridTextColumn Header="Aid" Binding="{Binding Aid}" />
          <DataGridTextColumn Header="Name" Binding="{Binding Name}" />
          <DataGridTextColumn Header="Type" Binding="{Binding Type}" />
          <DataGridTextColumn Header="Lang" Binding="{Binding Lang}" />
      </DataGrid.Columns>
  </DataGrid>
  ```
  (Requires `Avalonia.Controls.DataGrid` package, or use styled `ListBox`)

- `TextBox_KeyUp` handler (Enter key) -> `KeyBindings` or `Interaction.Behaviors`
- `LoadingBarControl` overlay stays the same
- `ButtonHelper.DialogResult` -> command-based close

#### `IssuesWindow.xaml` -> `IssuesWindow.axaml` - **Medium effort**

Source: `Onigiri/Views/IssuesWindow.xaml`

Same approach as TitlesWindow:
- `ListView` + `GridView` -> `DataGrid`
- State column with `Image` DataTrigger -> `DataGrid.CellTemplate` with converter
- Action column with `Button` -> `DataGridTemplateColumn` with Button

---

## Phase 5: Custom Controls Migration

Port from `Onigiri/Controls/` to `OnigiriAvalonia/Controls/`.

### 5A. Control Property System Change

All `DependencyProperty` declarations become `StyledProperty<T>`:

```csharp
// BEFORE (WPF):
public static readonly DependencyProperty LoadingHeaderProperty =
    DependencyProperty.Register("LoadingHeader", typeof(string), typeof(LoadingBarControl));
public string LoadingHeader
{
    get => (string)GetValue(LoadingHeaderProperty);
    set => SetValue(LoadingHeaderProperty, value);
}

// AFTER (Avalonia):
public static readonly StyledProperty<string> LoadingHeaderProperty =
    AvaloniaProperty.Register<LoadingBarControl, string>(nameof(LoadingHeader));
public string LoadingHeader
{
    get => GetValue(LoadingHeaderProperty);
    set => SetValue(LoadingHeaderProperty, value);
}
```

### 5B. Control-by-Control Migration

#### `LoadingBarControl` - **Low effort**

Source: `Onigiri/Controls/LoadingBarControl.xaml` + `.cs`

4 properties to convert: `LoadingSubject` (string), `LoadingHeader` (string), `LoadingPercentage` (double), `IsLoadingMarque` (bool).

AXAML changes:
- `Background="{DynamicResource MaterialDesign.Brush.Background}"` -> `Background="{DynamicResource SystemControlBackgroundAltHighBrush}"` or custom brush
- `TextTrimming="CharacterEllipsis"` -> `TextTrimming="CharacterEllipsis"` (same)
- `ProgressBar.IsIndeterminate` -> same property exists in Avalonia

#### `AnimeCardControl` - **Medium effort**

Source: `Onigiri/Controls/AnimeCardControl.xaml` + `.cs`

AXAML changes:
- `IsAsync=True` on Image binding -> Avalonia loads images async by default; use converter that returns `Task<Bitmap>` or `Bitmap` directly
- `FallbackValue={StaticResource noPictureImage}` -> `FallbackValue` works in Avalonia
- `RelativeSource AncestorType=Page` -> `$parent[UserControl]` or `$parent[views:CardListView]`
- `DelegateCommand` properties in code-behind (scroll commands) -> same, using custom MVVM DelegateCommand
- `ScrollViewer` scroll offset manipulation -> Avalonia `ScrollViewer.Offset` property

Code-behind changes:
- `ScrollViewer.ScrollToHorizontalOffset()` -> `scrollViewer.Offset = new Vector(newOffset, scrollViewer.Offset.Y)`

#### `UserStatesControl` - **Low effort**

Source: `Onigiri/Controls/UserStatesControl.xaml` + `.cs`

Minimal code-behind. AXAML changes:
- `DataTrigger` on `AddonData.Marked` -> Avalonia `<Style Selector="...">` with data binding or converter
- `AddonStateImageSourceMultiConverter` (MarkupExtension) -> declare as resource
- Image source binding patterns stay similar

#### `UserActionsPanel` - **Medium effort**

Source: `Onigiri/Controls/UserActionsPanel.xaml` + `.cs`

4 `DependencyProperty` -> 4 `StyledProperty<T>`:
- `ToggleRemoveAnimeCommand` (ICommand)
- `ToggleWatchedAnimeCommand` (ICommand)
- `ToggleMarkedAnimeCommand` (ICommand)
- `UsersView` (ICollectionView -> ObservableCollection)

AXAML changes:
- `ControlTemplate` with `ControlTemplate.Triggers` -> Avalonia `ControlTheme` with `Style` selectors:
  ```xml
  <!-- WPF Trigger: -->
  <ControlTemplate.Triggers>
      <Trigger SourceName="actionsPanel" Property="IsMouseOver" Value="True">
          <Setter TargetName="innerPanel" Property="Visibility" Value="Visible" />
      </Trigger>
  </ControlTemplate.Triggers>

  <!-- Avalonia: -->
  <Style Selector="Button:pointerover /template/ StackPanel#innerPanel">
      <Setter Property="IsVisible" Value="True" />
  </Style>
  ```
- `MultiBinding` with `AnimeUserMultiConverter` -> same pattern (Avalonia supports `MultiBinding`)

---

## Phase 6: Converter Migration

Port from `Onigiri/Converters/` to `OnigiriAvalonia/Converters/`.

### 6A. Converter Interface Changes

| WPF | Avalonia |
|---|---|
| `System.Windows.Data.IValueConverter` | `Avalonia.Data.Converters.IValueConverter` |
| `System.Windows.Data.IMultiValueConverter` | `Avalonia.Data.Converters.IMultiValueConverter` |
| `DependencyProperty.UnsetValue` | `AvaloniaProperty.UnsetValue` |
| `System.Windows.Markup.MarkupExtension` | Remove (declare converters as `<StaticResource>`) |

### 6B. Converter-by-Converter Migration

| Converter | Action | Effort |
|---|---|---|
| **`BoolVisibilityConverter`** | **Delete** - Avalonia binds `IsVisible` to `bool` natively | None |
| **`AnimeDescriptionConverter`** | Namespace change only (string logic, no UI types) | Trivial |
| **`ThemeToBoolConverter`** | Namespace change only (enum comparison) | Trivial |
| **`IsGreaterThanConverter`** | Namespace change only (numeric comparison) | Trivial |
| **`IntGreaterThanConverter`** | Remove `MarkupExtension` base, change `IValueConverter` namespace. Declare as resource in AXAML | Low |
| **`NullImageConverter`** | Return `AvaloniaProperty.UnsetValue` instead of `DependencyProperty.UnsetValue` | Trivial |
| **`AnimeImageConverter`** | Replace `BitmapImage` with `Avalonia.Media.Imaging.Bitmap`. Remove `BeginInit()/EndInit()/Freeze()`. Use `new Bitmap(stream)` | Medium |
| **`AnimeUserMultiConverter`** | Namespace change only (creates `AnimeUserViewModel`, no UI types) | Trivial |
| **`ReferenceEqualsToBooleanMultiConverter`** | Remove `MarkupExtension` base, use `Avalonia.Data.Converters.IMultiValueConverter`. Declare as resource | Low |
| **`BooleanLogicalOrMultiConverter`** | Remove `MarkupExtension` base. Or replace with `Avalonia.Data.Converters.BoolConverters.Or` (built-in) | Trivial |
| **`AddonStateImageSourceMultiConverter`** | Remove `MarkupExtension`. Replace `BitmapImage` -> `Avalonia.Media.Imaging.Bitmap`. Replace `pack://` URIs -> `avares://` URIs | Medium |
| **`UserImageResourceConverter`** | **Major rewrite**: Replace WPF `RenderTargetBitmap` + `Grid`/`TextBlock` rendering with Avalonia `RenderTargetBitmap` + `DrawingContext`. Replace `Application.GetResourceStream()` with `AssetLoader.Open()`. Replace `PngBitmapEncoder` with Avalonia's `Bitmap.Save()` | High |
| **`UsersSampleData`** | Remove `MarkupExtension`. Declare as design-time resource | Trivial |

### 6C. AnimeImageConverter Detail

```csharp
// BEFORE (WPF):
BitmapImage image = new BitmapImage();
mem.Seek(0, SeekOrigin.Begin);
image.BeginInit();
image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
image.CacheOption = BitmapCacheOption.OnLoad;
image.StreamSource = mem;
image.EndInit();
image.Freeze();

// AFTER (Avalonia):
mem.Seek(0, SeekOrigin.Begin);
var image = new Avalonia.Media.Imaging.Bitmap(mem);
```

### 6D. UserImageResourceConverter Detail

This is the most complex converter. It renders a text-based avatar when no user image exists.

**WPF approach:** Creates WPF `Grid` + `TextBlock` controls, measures/arranges them, renders to `RenderTargetBitmap`, saves as PNG.

**Avalonia approach:** Use `DrawingContext` to draw directly:
```csharp
var rtb = new RenderTargetBitmap(new PixelSize(64, 64), new Vector(96, 96));
using (var ctx = rtb.CreateDrawingContext())
{
    ctx.FillRectangle(Brushes.LightGray, new Rect(0, 0, 64, 64));
    var text = new FormattedText(userName[..Math.Min(5, userName.Length)],
        CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
        Typeface.Default, 32, Brushes.Black);
    ctx.DrawText(text, new Point((64 - text.Width) / 2, (64 - text.Height) / 2));
}
```

Resource loading: `Application.GetResourceStream(uri)` -> `AssetLoader.Open(new Uri("avares://OnigiriAvalonia/Resources/..."))`.

---

## Phase 7: Styles Migration (XAML -> AXAML)

### 7A. Style Syntax Changes

Avalonia uses CSS-like selectors instead of WPF's `TargetType` + `BasedOn`:

```xml
<!-- WPF: -->
<Style x:Key="windowStyle" TargetType="Window" BasedOn="{StaticResource MaterialDesignWindow}">
    <Setter Property="Foreground" Value="{DynamicResource MaterialDesign.Brush.Foreground}" />
    <Setter Property="FontFamily" Value="{DynamicResource MaterialDesignFont}" />
    <Setter Property="FontSize" Value="20" />
</Style>

<!-- Avalonia: -->
<Style Selector="Window.onigiri">
    <Setter Property="Foreground" Value="{DynamicResource SystemControlForegroundBaseHighBrush}" />
    <Setter Property="FontFamily" Value="{StaticResource ContentControlThemeFontFamily}" />
    <Setter Property="FontSize" Value="20" />
</Style>
```

### 7B. File-by-File Style Migration

#### `Controls.axaml` (from `Controls.xaml`)

**Resource declarations (converter instances, images, brushes, sizes):**
```xml
<!-- Converter instances: same pattern -->
<converters:AnimeImageConverter x:Key="animeImageConverter" />
<converters:AnimeDescriptionConverter x:Key="animeDescConv" />

<!-- BitmapImage -> Bitmap with avares:// URIs -->
<!-- WPF: -->
<BitmapImage x:Key="noPictureImage" UriSource="/Resources/nopicture.png" />
<!-- Avalonia: Can't use BitmapImage. Use a converter or load in code, or reference directly in Image.Source -->

<!-- Brushes: same syntax -->
<SolidColorBrush x:Key="animeTitleBackground" Color="#BF000000" />

<!-- Sizes as doubles/Thickness: -->
<Thickness x:Key="cardMargin">10</Thickness>
<sys:Double x:Key="cardWidth">800</sys:Double>
<sys:Double x:Key="cardHeight">400</sys:Double>
```

**Style definitions:** Convert all `TargetType` styles to selector-based:
```xml
<!-- Column/Row definitions can't have styles in Avalonia. Use fixed values or classes. -->
<!-- WPF Style on ColumnDefinition is not supported in Avalonia. Define widths inline. -->

<!-- Image styles: -->
<Style Selector="Image.pictureImage">
    <Setter Property="Width" Value="260" />
    <Setter Property="Height" Value="400" />
    <Setter Property="Stretch" Value="UniformToFill" />
</Style>

<!-- Text styles: -->
<Style Selector="TextBlock.animeLabel">
    <Setter Property="FontFamily" Value="{StaticResource ContentControlThemeFontFamily}" />
</Style>
```

**DataTrigger conversion:**
```xml
<!-- WPF DataTrigger: -->
<Style.Triggers>
    <DataTrigger Binding="{Binding Rating}" Value="True">
        <Setter Property="Source" Value="{StaticResource star1}" />
    </DataTrigger>
</Style.Triggers>

<!-- Avalonia: Use a converter or multiple styles with data selectors -->
<!-- Avalonia doesn't have DataTriggers. Use IValueConverter or classes + pseudoclasses -->
```

**ControlTemplate conversion:**
```xml
<!-- WPF ControlTemplate with Triggers: -->
<ControlTemplate x:Key="videoQualityPanelTemplate" TargetType="Button">
    <Grid>...</Grid>
    <ControlTemplate.Triggers>
        <Trigger Property="IsMouseOver" Value="True">...</Trigger>
    </ControlTemplate.Triggers>
</ControlTemplate>

<!-- Avalonia ControlTheme: -->
<ControlTheme x:Key="videoQualityPanelTheme" TargetType="Button">
    <Setter Property="Template">
        <ControlTemplate>
            <Grid>...</Grid>
        </ControlTemplate>
    </Setter>
    <Style Selector="^:pointerover">
        <Setter Property="..." Value="..." />
    </Style>
</ControlTheme>
```

#### `Onigiri.axaml` (from `Onigiri.xaml`)

Application-specific styles. Same conversion patterns as above. Focus on:
- Card layout styles (grid dimensions, margins)
- Rating icon styles (star images based on rating value)
- User state panel styles
- Action panel hover effects
- Tag/category item styles

#### `LightColors.axaml` and `DarkColors.axaml`

These are straightforward resource dictionaries with `SolidColorBrush` definitions:

```xml
<!-- Same in both WPF and Avalonia: -->
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <SolidColorBrush x:Key="animeListBackground" Color="#D6E0EF" />
    <SolidColorBrush x:Key="animeItemBackground" Color="#FAFCFC" />
    <SolidColorBrush x:Key="headerBackground" Color="#FAFCFC" />
    <SolidColorBrush x:Key="tagsBackground" Color="#E5E9F5" />
    <SolidColorBrush x:Key="descriptionForeground" Color="#5C728A" />
    <SolidColorBrush x:Key="mainlineForeground" Color="#5C728A" />
    <SolidColorBrush x:Key="sublineForeground" Color="#5C728A" />
    <SolidColorBrush x:Key="noAnimesForeground" Color="#5C728A" />
    <SolidColorBrush x:Key="tagItemBorderBrush" Color="Black" />
</ResourceDictionary>
```

### 7C. MaterialDesign -> Fluent Theme Resource Mapping

| MaterialDesign Resource | Fluent Theme Equivalent |
|---|---|
| `MaterialDesign.Brush.Foreground` | `SystemControlForegroundBaseHighBrush` or `TextFillColorPrimaryBrush` |
| `MaterialDesign.Brush.Background` | `SystemControlBackgroundAltHighBrush` |
| `MaterialDesignFont` | `ContentControlThemeFontFamily` |
| `MaterialDesignWindow` | Default `Window` style (Fluent) |
| `MaterialDesignListView` | Default `ListBox`/`DataGrid` style (Fluent) |
| `MaterialDesignBody1TextBlock` | Use `TextBlock` with `Theme` property |

---

## Phase 8: Helpers Migration

### 8A. WindowHelper - **Delete**

`Onigiri/Helpers/WindowHelper.cs` uses P/Invoke `DwmSetWindowAttribute` to set dark mode on the Windows titlebar. Avalonia handles this natively via `RequestedThemeVariant`. **No equivalent needed.**

### 8B. ButtonHelper - **Rewrite or Remove**

`Onigiri/Helpers/ButtonHelper.cs` provides an attached `DialogResult` property for buttons.

**Avalonia approach:** Use command-based dialog closing instead:

```csharp
// In ViewModel or code-behind:
public void CloseDialog(Window window, bool result)
{
    window.Close(result);
}
```

Or create an Avalonia `AttachedProperty<bool?>`:
```csharp
public class ButtonHelper
{
    public static readonly AttachedProperty<bool?> DialogResultProperty =
        AvaloniaProperty.RegisterAttached<ButtonHelper, Button, bool?>("DialogResult");

    static ButtonHelper()
    {
        DialogResultProperty.Changed.AddClassHandler<Button>((button, e) =>
        {
            button.Click += (s, _) =>
            {
                var window = button.FindAncestorOfType<Window>();
                window?.Close(GetDialogResult(button));
            };
        });
    }

    public static bool? GetDialogResult(Button button) => button.GetValue(DialogResultProperty);
    public static void SetDialogResult(Button button, bool? value) => button.SetValue(DialogResultProperty, value);
}
```

### 8C. ExtendedObservableCollection - **Port as-is**

`Onigiri/Utils/ExtendedObservableCollection.cs` uses `ObservableCollection<T>` which is a .NET BCL type, not WPF-specific. Works unchanged in Avalonia.

---

## Phase 9: DataTrigger Replacement Strategy

Avalonia does not have WPF's `DataTrigger`. This affects several controls. There are three replacement strategies:

### Strategy 1: Value Converters (recommended for most cases)

Replace DataTrigger-based image/visibility switching with converters:

```xml
<!-- WPF DataTrigger: -->
<Image>
    <Image.Style>
        <Style TargetType="Image">
            <Setter Property="Source" Value="{StaticResource star0}" />
            <Style.Triggers>
                <DataTrigger Binding="{Binding PermanentRating, Converter={StaticResource isGreaterThan}, ConverterParameter=2}" Value="True">
                    <Setter Property="Source" Value="{StaticResource star1}" />
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Image.Style>
</Image>

<!-- Avalonia: Use a RatingToImageConverter -->
<Image Source="{Binding PermanentRating, Converter={StaticResource ratingIconConverter}, ConverterParameter=0}" />
```

### Strategy 2: Classes + Pseudoclasses (for control states)

For mouse-over, focus, and other interactive states:

```xml
<Style Selector="Button:pointerover /template/ StackPanel#innerPanel">
    <Setter Property="IsVisible" Value="True" />
</Style>
```

### Strategy 3: Data Template Selectors

For switching entire templates based on data type or value, use `IDataTemplate` or `FuncDataTemplate<T>`.

---

## Phase 10: Verification & Testing

### 10A. Build Verification

```bash
# Build the new project
dotnet build OnigiriAvalonia/OnigiriAvalonia.csproj -c Debug -p:Platform=x64

# Build entire solution (both WPF and Avalonia)
dotnet build Onigiri.sln -c Debug -p:Platform=x64

# Run existing tests (should still pass)
dotnet test OnigiriTests/OnigiriTests.csproj -p:Platform=x64
```

### 10B. Runtime Verification

```bash
# Launch the Avalonia app
dotnet run --project OnigiriAvalonia/OnigiriAvalonia.csproj -c Debug
```

**Visual verification checklist:**
- [ ] Application window opens with Fluent theme
- [ ] Menu bar renders with all items (File, Data, View, Window)
- [ ] Anime card list loads and displays cards with images
- [ ] Card layout: image (260px) + content (540px) with rating stars, episode info, description, categories
- [ ] Scrolling performance is acceptable (virtualized list)
- [ ] Theme switching works (Light/Dark/Automatic)
- [ ] Sort panel: primary/secondary sort by title/rating/date, ascending/descending
- [ ] Filter panel: by type, title text, watch state, marked, category
- [ ] Settings dialog opens, add/remove search paths works
- [ ] Titles dialog opens with filterable DataGrid
- [ ] Issues dialog opens with issue list and "Select Title" action
- [ ] Import/Export dialogs (file pickers) work
- [ ] Loading overlay shows during async operations
- [ ] Status bar shows counts and loading status
- [ ] User action panel appears on hover (mark, watch, delete)
- [ ] User state icons display correctly

### 10C. Cross-Platform Testing

```bash
# Linux
dotnet run --project OnigiriAvalonia/OnigiriAvalonia.csproj
```

---

## Implementation Order Summary

| Step | What | Depends On | Effort |
|---|---|---|---|
| 1 | Project setup, App bootstrap, resources | Nothing | Low |
| 2 | Trivial ViewModels (8 files) | Step 1 | Trivial |
| 3 | Service interfaces + implementations | Step 1 | Medium |
| 4 | Converters (13 files) | Step 1 | Medium |
| 5 | Styles (4 AXAML files) | Step 4 | Medium |
| 6 | `MainViewModel` | Steps 2, 3 | Medium |
| 7 | `TitlesViewModel`, `IssuesViewModel`, `ConfigViewModel` | Steps 2, 3 | Medium |
| 8 | `LoadingBarControl` | Step 5 | Low |
| 9 | `UserStatesControl`, `UserActionsPanel` | Steps 4, 5 | Medium |
| 10 | `AnimeCardControl` | Steps 4, 5, 8, 9 | Medium |
| 11 | `CardListView` (from CardListPage) | Step 10 | Medium |
| 12 | `ConfigWindow`, `DetailsWindow` | Step 7 | Low |
| 13 | `TitlesWindow`, `IssuesWindow` | Step 7 | Medium |
| 14 | `MainWindow` | Steps 6, 8, 11, 12, 13 | High |
| 15 | Helpers (ButtonHelper) | Step 14 | Low |
| 16 | Integration testing | All | - |

---

## Risk Assessment

| Risk | Impact | Mitigation |
|---|---|---|
| `ICollectionView` replacement complexity | High | The sort/filter logic already exists in `AnimeSorter`/`TitleSorter`/`IssuesSorter`. Only the collection refresh mechanism changes. |
| `VirtualizingWrapPanel` performance parity | Medium | Start with `ItemsRepeater` + `WrapLayout`. If insufficient, consider `Avalonia.Labs` virtualization or custom `VirtualizingLayout`. |
| DataTrigger absence in Avalonia | Medium | Converters handle most cases. The rating stars, user state icons, and action panel hover are the most affected areas. |
| `UserImageResourceConverter` complexity | Medium | WPF `RenderTargetBitmap` API is significantly different from Avalonia's. Test avatar generation separately. |
| Dialog async migration | Low | `ShowDialog` becomes async. Commands calling dialogs need `async void` or `AsyncCommand`. Already partially async in WPF version. |
| MaterialDesign visual fidelity | Low | Fluent theme looks different. Custom color dictionaries (LightColors/DarkColors) preserve the Onigiri brand colors. |
| Serilog configuration | Low | Pattern already proven in OnigiriConsole. Add file sink for parity with log4net's file appender. |

---

## Final Project Structure

```
OnigiriAvalonia/
  OnigiriAvalonia.csproj
  Program.cs
  app.manifest
  App.axaml / App.axaml.cs
  ViewModels/
    MainViewModel.cs          (medium rewrite: ICollectionView, Visibility, dialogs)
    ConfigViewModel.cs        (low: IFolderDialogService)
    TitlesViewModel.cs        (medium: ICollectionView, BackgroundWorker)
    IssuesViewModel.cs        (medium: ICollectionView)
    AnimeUserViewModel.cs     (trivial: using change)
    CategoryItemViewModel.cs  (trivial)
    SortItemViewModel.cs      (trivial)
    NameItemViewModel.cs      (trivial)
    WatchStateItemViewModel.cs (trivial)
    MainTheme.cs              (none)
    TestMainViewModel.cs      (trivial)
    TestAnimeViewModel.cs     (trivial)
  Views/
    MainWindow.axaml / .cs
    CardListView.axaml / .cs  (was CardListPage)
    ConfigWindow.axaml / .cs
    DetailsWindow.axaml / .cs
    TitlesWindow.axaml / .cs
    IssuesWindow.axaml / .cs
  Controls/
    AnimeCardControl.axaml / .cs
    LoadingBarControl.axaml / .cs
    UserStatesControl.axaml / .cs
    UserActionsPanel.axaml / .cs
  Converters/
    AnimeImageConverter.cs
    AnimeDescriptionConverter.cs
    ThemeToBoolConverter.cs
    IsGreaterThanConverter.cs
    IntGreaterThanConverter.cs
    NullImageConverter.cs
    ReferenceEqualsToBooleanMultiConverter.cs
    AddonStateImageSourceMultiConverter.cs
    UserImageResourceConverter.cs
    AnimeUserMultiConverter.cs
    UsersSampleData.cs
    (BoolVisibilityConverter.cs - DELETED, not needed)
    (BooleanLogicalOrMultiConverter.cs - DELETED, use built-in BoolConverters.Or)
  Services/
    IOnigiriDialogService.cs
    DefaultOnigiriDialogService.cs
    IProcessStarterService.cs
    DefaultProcessStarterService.cs
    IThemeManagerService.cs
    AvaloniaThemeManagerService.cs
    IDarkModeDetectionService.cs
    AvaloniaDarkModeDetectionService.cs
    IFileDialogService.cs
    AvaloniaFileDialogService.cs
    IFolderDialogService.cs
    AvaloniaFolderDialogService.cs
  Helpers/
    ButtonHelper.cs           (rewritten for Avalonia AttachedProperty)
  Utils/
    ExtendedObservableCollection.cs  (ported as-is)
  Styles/
    Controls.axaml
    Onigiri.axaml
    LightColors.axaml
    DarkColors.axaml
  Resources/
    (all ~59 PNG/ICO/JPG files copied from Onigiri/Resources/)
```
