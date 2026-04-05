# Onigiri Migration Plan: .NET 10.0 + AvaloniaUI (Cross-Platform)

## Context

Onigiri is a Windows-only WPF anime database app built on .NET 9.0 with DevExpress MVVM and MaterialDesignThemes. The goal is to make it cross-platform by:

1. Migrating core/platform/console/test projects from .NET 9.0 to .NET 10.0 (removing Windows-only TFMs)
2. Replacing `DevExpressMvvm` (WPF-only) with a **custom MVVM layer that mimics the DevExpress API**, built on top of `CommunityToolkit.Mvvm` — so model/ViewModel code requires only a `using` change
3. **Creating a new `OnigiriAvalonia` project** alongside the existing `Onigiri` WPF project, using AvaloniaUI
4. Adding Linux/macOS platform support in OnigiriPlatform

The existing WPF `Onigiri` project is **preserved as-is** — the Avalonia UI is a new project.

---

## Phase 1: OnigiriCore — Custom MVVM Layer, Target .NET 10.0

This is the foundation all projects depend on, so it must be migrated first.

### 1A. Project File (`OnigiriCore/OnigiriCore.csproj`)

- `TargetFramework`: `net9.0` → `net10.0`
- `LangVersion`: `10.0` → `latest`
- Remove: `<PackageReference Include="DevExpressMvvm" Version="21.1.5" />`
- Add: `<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />`
- Move `System.Security.Principal.Windows` to OnigiriPlatform (it's only needed by Win32 implementations)
- Keep `FFmpeg.AutoGen` v5.0.0 for now (multi-platform native libs are a Phase 5 follow-up)
- Keep the ffmpeg DLL `<Content>` items (they still work for Windows builds)

### 1B. Custom MVVM Layer — Create `OnigiriCore/MVVM/`

Build a DevExpress-API-compatible MVVM layer on top of `CommunityToolkit.Mvvm`. This means **all 19 model classes and all ViewModels need only a `using` change** — no property/method rewrites.

#### `OnigiriCore/MVVM/BindableBase.cs`

Extends `CommunityToolkit.Mvvm.ComponentModel.ObservableObject`. Mimics the DevExpress dictionary-backed property storage API.

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using System.Runtime.CompilerServices;

namespace Finalspace.Onigiri.MVVM;

public abstract class BindableBase : ObservableObject
{
    private readonly Dictionary<string, object?> _propertyBag = new();

    protected T? GetValue<T>([CallerMemberName] string? propertyName = null)
    {
        if (_propertyBag.TryGetValue(propertyName!, out var value))
            return (T?)value;
        return default;
    }

    protected void SetValue<T>(T? value, [CallerMemberName] string? propertyName = null)
    {
        if (_propertyBag.TryGetValue(propertyName!, out var existing) && EqualityComparer<T>.Default.Equals((T?)existing, value))
            return;
        _propertyBag[propertyName!] = value;
        OnPropertyChanged(propertyName);
    }

    protected void SetValue<T>(T? value, Action callback, [CallerMemberName] string? propertyName = null)
    {
        if (_propertyBag.TryGetValue(propertyName!, out var existing) && EqualityComparer<T>.Default.Equals((T?)existing, value))
            return;
        _propertyBag[propertyName!] = value;
        OnPropertyChanged(propertyName);
        callback();
    }

    protected void RaisePropertyChanged(string propertyName)
        => OnPropertyChanged(propertyName);

    protected void RaisePropertyChanged<T>(System.Linq.Expressions.Expression<Func<T>> propertyExpression)
    {
        var memberExpr = (System.Linq.Expressions.MemberExpression)propertyExpression.Body;
        OnPropertyChanged(memberExpr.Member.Name);
    }

    protected void RaisePropertiesChanged(params string[] propertyNames)
    {
        foreach (var name in propertyNames)
            OnPropertyChanged(name);
    }
}
```

**Key points:**
- Dictionary-backed storage preserves the exact DevExpress behavior (no backing fields needed in models)
- `SetValue(value, callback)` overload supports the side-effect pattern used in `Anime.MediaFiles`, `Relation.TypeStr`, `MainViewModel.FilterTitle`, etc.
- `RaisePropertyChanged(() => X)` lambda overload supports the expression-tree pattern used throughout
- Inheriting `ObservableObject` gives us `INotifyPropertyChanged` for free and interop with CommunityToolkit ecosystem

#### `OnigiriCore/MVVM/IServiceContainer.cs`

```csharp
namespace Finalspace.Onigiri.MVVM;

public interface IServiceContainer
{
    void RegisterService(object service);
    T? GetService<T>() where T : class;
}
```

#### `OnigiriCore/MVVM/ServiceContainer.cs`

Mimics `DevExpress.Mvvm.ServiceContainer`:

```csharp
namespace Finalspace.Onigiri.MVVM;

public class ServiceContainer : IServiceContainer
{
    public static ServiceContainer Default { get; } = new();

    private readonly Dictionary<Type, object> _services = new();

    public void RegisterService(object service)
    {
        // Register by all interfaces the service implements
        foreach (var iface in service.GetType().GetInterfaces())
            _services[iface] = service;
    }

    public T? GetService<T>() where T : class
    {
        _services.TryGetValue(typeof(T), out var service);
        return service as T;
    }
}
```

#### `OnigiriCore/MVVM/ViewModelBase.cs`

Extends `BindableBase`, adds service resolution — mimics `DevExpress.Mvvm.ViewModelBase`:

```csharp
namespace Finalspace.Onigiri.MVVM;

public abstract class ViewModelBase : BindableBase
{
    protected T? GetService<T>() where T : class
        => ServiceContainer.Default.GetService<T>();
}
```

#### `OnigiriCore/MVVM/DelegateCommand.cs`

Implements `ICommand` with `RaiseCanExecuteChanged()` — mimics `DevExpress.Mvvm.DelegateCommand`:

```csharp
using System.Windows.Input;

namespace Finalspace.Onigiri.MVVM;

public class DelegateCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public DelegateCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class DelegateCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool>? _canExecute;

    public DelegateCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke((T)parameter!) ?? true;
    public void Execute(object? parameter) => _execute((T)parameter!);
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
```

### 1C. Model/Service Migration — `using` Change Only

With the custom MVVM layer in place, the 19 model files and all ViewModel references need only:

```csharp
// BEFORE:
using DevExpress.Mvvm;

// AFTER:
using Finalspace.Onigiri.MVVM;
```

**No property rewrites, no backing field additions, no method renames.** The `GetValue<T>()`, `SetValue()`, `RaisePropertyChanged()`, `DelegateCommand`, `ServiceContainer`, `ViewModelBase` APIs all remain identical.

**Affected files (all `using DevExpress.Mvvm;` → `using Finalspace.Onigiri.MVVM;`):**

In `OnigiriCore/`:
- `Models/Anime.cs`, `Models/Config.cs`, `Models/Title.cs`, `Models/Titles.cs`
- `Models/Episode.cs`, `Models/Rating.cs`, `Models/Category.cs`, `Models/Tag.cs`
- `Models/User.cs`, `Models/AdditionalData.cs`, `Models/Relation.cs`
- `Models/AnimeMediaFile.cs`, `Models/UserState.cs`, `Models/SearchPath.cs`
- `Models/SearchTypeLanguage.cs`, `Models/Issue.cs`, `Models/Issues.cs`
- `Models/AnimeGroup.cs`, `Models/AnimeGroupItem.cs`
- `Media/MediaInfo.cs`

In `Onigiri/` (WPF project, also benefits from decoupling):
- `ViewModels/MainViewModel.cs`, `ViewModels/ConfigViewModel.cs`
- `ViewModels/TitlesViewModel.cs`, `ViewModels/IssuesViewModel.cs`
- `ViewModels/AnimeUserViewModel.cs`, `ViewModels/CategoryItemViewModel.cs`
- `ViewModels/SortItemViewModel.cs`, `ViewModels/NameItemViewModel.cs`
- `ViewModels/WatchStateItemViewModel.cs`
- `App.xaml.cs`, `Views/MainWindow.xaml.cs`

### 1D. OnigiriService Search Path Abstraction (`OnigiriCore/OnigiriService.cs`)

`ResolveSearchPath()` uses `DriveInfo.GetDrives()` with Windows drive letter matching. Extract to an interface:

- Create `OnigiriCore/Storage/ISearchPathResolver.cs` with `string Resolve(SearchPath searchPath)`
- Inject `ISearchPathResolver` into `OnigiriService` constructor
- Move current implementation to `OnigiriPlatform/Win32SearchPathResolver.cs`
- Create `OnigiriPlatform/Posix/PosixSearchPathResolver.cs` that returns `searchPath.Path` directly

### 1E. OnigiriPaths (`OnigiriCore/OnigiriPaths.cs`)

`Environment.SpecialFolder.MyDocuments` returns `~/Documents` on Linux and macOS — acceptable for now. Optionally switch to `Environment.SpecialFolder.ApplicationData` for more conventional cross-platform behavior (`~/.config` on Linux, `~/Library/Application Support` on macOS).

### 1F. Validation

```bash
dotnet build OnigiriCore/OnigiriCore.csproj -c Debug -p:Platform=x64
```

Must compile with zero DevExpress references. Run existing `AnimeSerializationTests` to verify XML serialization still works (the dictionary-backed storage preserves the same public property API).

---

## Phase 2: OnigiriPlatform — Cross-Platform User Service

### 2A. Project File (`OnigiriPlatform/OnigiriPlatform.csproj`)

- `TargetFramework`: `net9.0` → `net10.0`
- `LangVersion`: `10.0` → `latest`
- Add: `<PackageReference Include="System.Security.Principal.Windows" Version="5.0.0" Condition="$([MSBuild]::IsOSPlatform('Windows'))" />`

### 2B. New Posix Implementation Files

Create `OnigiriPlatform/Posix/` directory with:

- **`PosixUserService.cs`** — implements `IUserService`, returns `PosixUserIdentity`
- **`PosixUserIdentity.cs`** — implements `IUserIdentity`, uses `Environment.UserName`
- **`PosixImpersonationContext.cs`** — implements `IImpersonationContext`, no-op `Dispose()`
- **`PosixSearchPathResolver.cs`** — implements `ISearchPathResolver`, returns path directly

### 2C. Update Factory (`OnigiriPlatform/OnigiriUserServiceFactory.cs`)

Add Linux/macOS branch:
```csharp
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    return new PosixUserService();
```

### 2D. Validation

```bash
dotnet build OnigiriPlatform/OnigiriPlatform.csproj -c Debug -p:Platform=x64
```

---

## Phase 3: OnigiriTests + OnigiriConsole — .NET 10.0

### 3A. OnigiriTests (`OnigiriTests/OnigiriTests.csproj`)

- `TargetFramework`: `net9.0-windows` → `net10.0`
- Update MSTest packages to latest stable
- `MediaParserTests.cs`: Add `[TestCategory("Windows")]` or runtime platform check for hard-coded Windows paths

### 3B. OnigiriConsole (`OnigiriConsole/OnigiriConsole.csproj`)

- `TargetFramework`: `net9.0-windows` → `net10.0`
- Replace local `log4net.dll` HintPath reference with NuGet `<PackageReference>`

### 3C. Validation

```bash
dotnet test OnigiriTests/OnigiriTests.csproj -p:Platform=x64
dotnet build OnigiriConsole/OnigiriConsole.csproj -c Debug -p:Platform=x64
```

---

## Phase 4: OnigiriAvalonia — New Avalonia UI Project

**This is a new project** added to the solution, not a replacement of the WPF `Onigiri` project.

### 4A. Create Project (`OnigiriAvalonia/OnigiriAvalonia.csproj`)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>WinExe</OutputType>
    <RootNamespace>Finalspace.Onigiri.Avalonia</RootNamespace>
    <Nullable>enable</Nullable>
    <ApplicationIcon>..\Assets\onigiri.ico</ApplicationIcon>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\OnigiriCore\OnigiriCore.csproj" />
    <ProjectReference Include="..\OnigiriPlatform\OnigiriPlatform.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.*" />
    <PackageReference Include="Avalonia.Desktop" Version="11.*" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.*" />
    <PackageReference Include="Avalonia.Diagnostics" Version="11.*" />
    <PackageReference Include="Avalonia.Xaml.Behaviors" Version="11.*" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
    <PackageReference Include="log4net" Version="2.0.13" />
  </ItemGroup>
</Project>
```

**Theme choice**: Use `Avalonia.Themes.Fluent` (built-in, well-maintained). Alternatively `Material.Avalonia` for closer Material Design look — but Fluent is the Avalonia default and most stable.

### 4B. Application Bootstrap

**`Program.cs`** (Avalonia entry point):
```csharp
public static class Program
{
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace();
}
```

**`App.axaml`** — Application definition with FluentTheme + custom styles:
```xml
<Application xmlns="https://github.com/avaloniaui" ...>
  <Application.Styles>
    <FluentTheme />
    <StyleInclude Source="avares://OnigiriAvalonia/Styles/Controls.axaml" />
    <StyleInclude Source="avares://OnigiriAvalonia/Styles/Onigiri.axaml" />
  </Application.Styles>
</Application>
```

**`App.axaml.cs`** — Service registration via our custom `ServiceContainer` + MainWindow creation in `OnFrameworkInitializationCompleted()`:
```csharp
public override void OnFrameworkInitializationCompleted()
{
    ServiceContainer.Default.RegisterService(new DefaultProcessStarterService());
    ServiceContainer.Default.RegisterService(new AvaloniaThemeManagerService());
    ServiceContainer.Default.RegisterService(new AvaloniaDarkModeDetectionService());
    // ... etc

    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        var mainWindow = new MainWindow();
        ServiceContainer.Default.RegisterService(new DefaultOnigiriDialogService(mainWindow));
        mainWindow.DataContext = new MainViewModel();
        desktop.MainWindow = mainWindow;
    }
    base.OnFrameworkInitializationCompleted();
}
```

### 4C. ViewModel Migration

ViewModels are ported from `Onigiri/ViewModels/` to `OnigiriAvalonia/ViewModels/`. Because our custom MVVM layer mimics the DevExpress API, the changes are minimal:

**What stays the same** (zero changes needed):
- All `GetValue<T>()`/`SetValue()` property patterns
- All `DelegateCommand`/`DelegateCommand<T>` declarations and construction
- All `GetService<T>()` calls
- All `RaisePropertyChanged()`/`RaisePropertiesChanged()` calls
- `ServiceContainer.Default.RegisterService()` / `GetService<T>()` in App startup

**What must change:**

| Change | Reason |
|--------|--------|
| `using DevExpress.Mvvm;` → `using Finalspace.Onigiri.MVVM;` | Namespace swap |
| `System.Windows.Visibility` → `bool` | Avalonia uses `IsVisible` bound to `bool` |
| `ICollectionView` / `ListCollectionView` / `CollectionViewSource` → manual filtered `ObservableCollection` | Avalonia has no CollectionView |
| `BindingOperations.EnableCollectionSynchronization()` → `Dispatcher.UIThread.InvokeAsync()` | WPF-specific cross-thread API |
| `ISaveFileDialogService` / `IOpenFileDialogService` → custom service wrapping Avalonia `StorageProvider` | WPF DevExpress dialog services |
| `IDispatcherService` → Avalonia `Dispatcher.UIThread` | WPF DevExpress dispatcher service |

**Critical: `ICollectionView` replacement** — Avalonia has no `ICollectionView`/`CollectionViewSource`/`ListCollectionView`. The filtering and sorting in `MainViewModel`, `TitlesViewModel`, and `IssuesViewModel` must move to the ViewModel layer:

- Replace `List<Anime> _animes` + `ICollectionView AnimesView` with:
  ```csharp
  private List<Anime> _allAnimes;
  private ObservableCollection<Anime> _filteredAnimes;
  public ObservableCollection<Anime> FilteredAnimes => _filteredAnimes;
  ```
- `UpdateFilter()` / `UpdateSort()` recompute `_filteredAnimes` from `_allAnimes` using LINQ + the existing `AnimeSorter`/`AnimesViewFilter` logic
- Remove `BindingOperations.EnableCollectionSynchronization()` — ensure collection mutations happen on UI thread via `Dispatcher.UIThread.InvokeAsync()`

**Files to port (from `Onigiri/ViewModels/` → `OnigiriAvalonia/ViewModels/`):**

| File | Effort | Notes |
|------|--------|-------|
| `MainViewModel.cs` (~930 lines) | Medium | Replace `ICollectionView`, `Visibility`, dialog services. Commands/properties stay as-is. |
| `ConfigViewModel.cs` | Low | Replace `IFolderBrowserDialogService` with Avalonia `StorageProvider`-based service |
| `TitlesViewModel.cs` | Medium | Replace `ICollectionView` + `TitleSorter`, replace `IDispatcherService` |
| `IssuesViewModel.cs` | Medium | Replace `ICollectionView` + `IssuesSorter` |
| `AnimeUserViewModel.cs` | Trivial | `using` change only |
| `CategoryItemViewModel.cs` | Trivial | `using` change only |
| `SortItemViewModel.cs` | Trivial | `using` change only |
| `NameItemViewModel.cs` | Trivial | `using` change only |
| `WatchStateItemViewModel.cs` | Trivial | `using` change only |
| `MainTheme.cs` | None | Enum, no changes |
| `TestMainViewModel.cs` | Trivial | `using` change only |
| `TestAnimeViewModel.cs` | Trivial | `using` change only |

### 4D. View Migration (XAML → AXAML)

Port views from `Onigiri/Views/` to `OnigiriAvalonia/Views/` with WPF → Avalonia changes.

**Key XAML differences:**

| WPF | Avalonia |
|-----|----------|
| `xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"` | `xmlns="https://github.com/avaloniaui"` |
| `clr-namespace:Foo` | `using:Foo` |
| `Window` | `Window` |
| `Page` + `Frame` | `UserControl` + `ContentControl` (no Frame/Page in Avalonia) |
| `StatusBar` | `DockPanel` or custom panel |
| `DependencyProperty` | `StyledProperty<T>` via `AvaloniaProperty.Register<>()` |
| `Visibility="Collapsed"` | `IsVisible="False"` |
| `Style TargetType="Button"` | `<Style Selector="Button">` (CSS-like selectors) |
| `BasedOn={StaticResource ...}` | Not needed — Avalonia styles cascade |
| `pack://application:,,,/` URIs | `avares://AssemblyName/` URIs |
| `dxmvvm:EventToCommand` | `Avalonia.Xaml.Behaviors` `EventTriggerBehavior` + `InvokeCommandAction` |
| `dxmvvm:Interaction.Behaviors` | `Interaction.Behaviors` from Avalonia.Xaml.Behaviors |
| MaterialDesign controls/brushes | Fluent theme resources |
| `VirtualizingWrapPanel` | `ItemsRepeater` with `UniformGridLayout` or `WrapLayout` |

**Views to create:**

| Source (WPF) | Target (Avalonia) | Notes |
|---|---|---|
| `Views/MainWindow.xaml` | `Views/MainWindow.axaml` | Remove Frame, use ContentControl. Remove dxmvvm behaviors. Replace StatusBar. |
| `Views/CardListPage.xaml` | `Views/CardListView.axaml` | Convert to UserControl. Replace VirtualizingWrapPanel with ItemsRepeater + UniformGridLayout. |
| `Views/ConfigWindow.xaml` | `Views/ConfigWindow.axaml` | Replace dialog service bindings with Avalonia StorageProvider. |
| `Views/DetailsWindow.xaml` | `Views/DetailsWindow.axaml` | Straightforward conversion. |
| `Views/IssuesWindow.xaml` | `Views/IssuesWindow.axaml` | Replace ListView with DataGrid or styled ListBox. |
| `Views/TitlesWindow.xaml` | `Views/TitlesWindow.axaml` | Same. |

### 4E. Custom Controls Migration

Port from `Onigiri/Controls/` to `OnigiriAvalonia/Controls/`:

| Control | Key Changes |
|---------|-------------|
| `AnimeCardControl` | `DependencyProperty` → `StyledProperty`. Commands stay as `DelegateCommand` (our custom impl). |
| `LoadingBarControl` | 4 `DependencyProperty` → 4 `StyledProperty<T>`. Pattern: `AvaloniaProperty.Register<LoadingBarControl, string>(nameof(LoadingHeader))` |
| `UserStatesControl` | Minimal changes — namespace + StyledProperty |
| `UserActionsControl` | 4 `DependencyProperty` → 4 `StyledProperty`. Commands stay as `DelegateCommand`. |

### 4F. Converter Migration

Port from `Onigiri/Converters/` to `OnigiriAvalonia/Converters/`:

| Converter | Action |
|-----------|--------|
| `AnimeImageConverter` | Replace `BitmapImage` with `Avalonia.Media.Imaging.Bitmap`. Remove `BeginInit()/EndInit()/Freeze()`. Use `new Bitmap(stream)`. |
| `BoolVisibilityConverter` | **Delete** — Avalonia binds `IsVisible` to `bool` natively. |
| `NullImageConverter` | Return `AvaloniaProperty.UnsetValue` instead of `DependencyProperty.UnsetValue`. |
| `AnimeDescriptionConverter` | Namespace change only (string-only logic). |
| `ThemeToBoolConverter` | Namespace change only. |
| `IsGreaterThanConverter` | Namespace change only. |
| `IntGreaterThanConverter` | Remove `MarkupExtension` base. Declare as AXAML resource. |
| `ReferenceEqualsToBooleanMultiConverter` | Remove `MarkupExtension`. Use `Avalonia.Data.Converters.IMultiValueConverter`. |
| `BooleanLogicalOrMultiConverter` | Remove `MarkupExtension`. Declare as resource. |
| `AddonStateImageSourceMultiConverter` | Remove `MarkupExtension`. Replace `BitmapImage` with Avalonia `Bitmap`. |
| `UserImageResourceConverter` | **Major rewrite**: Replace `Application.GetResourceStream()` with `AssetLoader.Open(new Uri("avares://..."))`. Replace `RenderTargetBitmap`/`PngBitmapEncoder` with Avalonia's `RenderTargetBitmap` or `DrawingContext`. |
| `AnimeUserMultiConverter` | Namespace change only (no UI types). |

### 4G. Service Layer

Port from `Onigiri/Services/` to `OnigiriAvalonia/Services/`. Service interfaces remain identical; implementations adapt to Avalonia APIs.

| Service | Changes |
|---------|---------|
| `IOnigiriDialogService` / `DefaultOnigiriDialogService` | Replace `System.Windows.Window` with `Avalonia.Controls.Window`. Use `await dialog.ShowDialog<TResult>(owner)`. |
| `IProcessStarterService` / `DefaultProcessStarterService` | Add `UseShellExecute = true` for cross-platform URL opening. |
| `IThemeManagerService` / `DefaultThemeManagerService` | Replace MaterialDesign `PaletteHelper` with Avalonia `Application.Current.RequestedThemeVariant = ThemeVariant.Dark/Light`. Replace `ResourceDictionary` swap with Avalonia style swap. Remove Frame reload hack. |
| `IDarkModeDetectionService` / `Win32DarkModeDetectionService` | Replace with Avalonia-native: use `Application.Current.ActualThemeVariant` and `ActualThemeVariantChanged` event. Fully cross-platform, no P/Invoke needed. |

### 4H. Helpers

| Helper | Action |
|--------|--------|
| `WindowHelper.cs` | **Delete** — `DwmSetWindowAttribute` P/Invoke unnecessary with Avalonia's built-in dark mode. |
| `ButtonHelper.cs` | Rewrite using Avalonia `AttachedProperty<bool?>` or replace with command-based dialog closing. |

### 4I. Styles (Port to AXAML)

Port from `Onigiri/Styles/` to `OnigiriAvalonia/Styles/`:

| File | Notes |
|------|-------|
| `Controls.xaml` → `Controls.axaml` | Replace MaterialDesign resource references with Fluent theme resources. Convert `Style TargetType` to Avalonia selector syntax. |
| `Onigiri.xaml` → `Onigiri.axaml` | Convert `ControlTemplate`, `DataTemplate`, `Style` to Avalonia syntax. Replace `BitmapImage` URIs with `avares://` format. |
| `LightColors.xaml` → `LightColors.axaml` | Convert `SolidColorBrush` resources. |
| `DarkColors.xaml` → `DarkColors.axaml` | Same. |

### 4J. Resources & Assets

- Image `.png`/`.ico` files: Copy to `OnigiriAvalonia/Resources/`
- Change from `<Resource Include="...">` to `<AvaloniaResource Include="...">` in csproj
- URI format: `"/Resources/icon.png"` → `"avares://OnigiriAvalonia/Resources/icon.png"`
- Window icon: set programmatically: `Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://OnigiriAvalonia/Resources/onigiri.ico")))`

### 4K. Utils

- `ExtendedObservableCollection.cs` — Port as-is. `ObservableCollection<T>` is a .NET type, not WPF-specific. The `AddRange` implementation works unchanged.

### 4L. Add to Solution

Add `OnigiriAvalonia` project to `Onigiri.sln` with both `Debug|x64` and `Release|x64` (and optionally `AnyCPU`) configurations.

### 4M. Validation

```bash
dotnet build OnigiriAvalonia/OnigiriAvalonia.csproj -c Debug
dotnet run --project OnigiriAvalonia/OnigiriAvalonia.csproj
```

Visual validation: card list renders, theme switching works, dialogs open, filtering/sorting works.

---

## Phase 5: Follow-Up Items (Post-Migration)

These are not blocking but improve cross-platform support:

- **FFmpeg multi-platform native libs**: Create `runtimes/win-x64/native/`, `runtimes/linux-x64/native/`, `runtimes/osx-x64/native/` with platform-appropriate shared libraries. Update `FFMpegMediaInfoParser.cs` to use `ffmpeg.RootPath`.
- **OnigiriPaths**: Consider switching from `MyDocuments` to `ApplicationData` for Linux/macOS conventions.
- **Solution platform configs**: Add `AnyCPU` platform alongside `x64` if FFmpeg doesn't require it.
- **CI/CD**: Add Linux and macOS build/test targets.

---

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| XML serialization breaks after BindableBase swap | High | Custom `BindableBase` preserves the dictionary-backed storage + same public property API. `AnimeSerializationTests` validate round-trips. |
| Custom MVVM layer has subtle API differences | Medium | The API surface is small and well-defined (6 methods on BindableBase, 2 on DelegateCommand). Unit test the custom layer. |
| ICollectionView replacement in MainViewModel | High | Most complex refactor. Implement `FilteredAnimes` ObservableCollection with manual filter/sort. The sorting/filter logic already exists. |
| VirtualizingWrapPanel equivalent | Medium | Start with non-virtualized `WrapPanel` for validation, then optimize with `ItemsRepeater` + `UniformGridLayout`. |
| UserImageResourceConverter rewrite | Medium | Uses WPF `RenderTargetBitmap` + `PngBitmapEncoder`. Avalonia has equivalent API but rendering code differs. |
| MaterialDesign visual fidelity | Low | FluentTheme looks different than MaterialDesign. If closer parity needed, use `Material.Avalonia` package instead. |

---

## NuGet Package Summary

| Current Package | Replacement | Scope |
|---|---|---|
| `DevExpressMvvm` 21.1.5 | Custom MVVM layer (built on `CommunityToolkit.Mvvm`) | OnigiriCore |
| `MaterialDesignThemes` 5.2.1 | `Avalonia.Themes.Fluent` (built-in) | OnigiriAvalonia only |
| `VirtualizingWrapPanel` 1.5.0 | Avalonia `ItemsRepeater` + layout | OnigiriAvalonia only |
| `System.Management` 6.0.0 | Remove (dark mode detection via Avalonia) | OnigiriAvalonia only |
| N/A | `Avalonia` 11.x | OnigiriAvalonia |
| N/A | `Avalonia.Desktop` 11.x | OnigiriAvalonia |
| N/A | `Avalonia.Diagnostics` 11.x | OnigiriAvalonia (dev) |
| N/A | `Avalonia.Xaml.Behaviors` 11.x | OnigiriAvalonia |
| N/A | `CommunityToolkit.Mvvm` 8.4.x | OnigiriCore (base for custom MVVM) |
