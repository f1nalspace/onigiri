# Onigiri Avalonia Migration TODO

## 2A. Global Changes Across All ViewModels
- [x] Replace `using DevExpress.Mvvm;` with `using Finalspace.Onigiri.MVVM;`
- [x] Replace `using System.Windows;` / `using System.Windows.Data;` removals
- [x] Replace `ILog` / `LogManager.GetLogger(...)` with `Serilog.ILogger` / `Log.ForContext<T>()`
- [x] Replace `System.Windows.Visibility` properties with `bool` properties
- [x] Replace `ICollectionView` with `ImmutableObservableCollection<T>`
- [x] Replace `ISaveFileDialogService` / `IOpenFileDialogService` with `IFileDialogService`
- [x] Replace `IFolderBrowserDialogService` with `IFolderDialogService`
- [x] Replace `IDispatcherService` with `Avalonia.Threading.Dispatcher.UIThread`

## 2B. ICollectionView Replacement Strategy
- [x] Pattern: `List<T>` + `ICollectionView` replaced with `List<T>` (backing) + `ImmutableObservableCollection<T>` (view)
- [x] `AnimeSorter`, `TitleSorter`, `IssuesSorter` reused via LINQ `List.Sort()`
- [x] `CollectionViewSource.GetDefaultView()` replaced with direct collection management
- [x] `BindingOperations.EnableCollectionSynchronization()` removed (ImmutableObservableCollection is thread-safe)

## 2C. File-by-File ViewModel Migration

### Trivial ViewModels (namespace change only)
- [x] `AnimeUserViewModel.cs` - BindableBase properties
- [x] `CategoryItemViewModel.cs` - ViewModelBase properties
- [x] `SortItemViewModel.cs` - ViewModelBase properties + IEquatable
- [x] `NameItemViewModel.cs` - ViewModelBase properties
- [x] `WatchStateItemViewModel.cs` - Extends NameItemViewModel
- [x] `MainTheme.cs` - Enum, no changes
- [x] `TestAnimeViewModel.cs` - Design-time data

### ConfigViewModel.cs - Low effort
- [x] Replace `IFolderBrowserDialogService` with `IFolderDialogService`
- [x] All command and property patterns migrated

### MainViewModel.cs - Medium effort (~930 lines)
- [x] Namespace imports updated (DevExpress, log4net, System.Windows removed)
- [x] Logging: `ILog` -> `Serilog.ILogger`
- [x] `Visibility LoadingWindowVisibility` -> `bool IsLoading`
- [x] `ICollectionView AnimesView` -> `ImmutableObservableCollection<Anime> Animes`
- [x] `ICollectionView UsersView` -> `ImmutableObservableCollection<User> Users`
- [x] `ExtendedObservableCollection` -> `ImmutableObservableCollection`
- [x] `AnimesView.Refresh()` -> `RefreshAnimes()` method
- [x] `UsersView.Refresh()` -> direct `Users.ReplaceAll()`
- [x] `ListCollectionView.CustomSort` -> LINQ sort via `AnimeSorter`
- [x] `AnimesViewFilter` updated (cast from `object` removed)
- [x] `ISaveFileDialogService` / `IOpenFileDialogService` -> `IFileDialogService` (async)
- [x] `IDispatcherService` -> removed (not needed with ImmutableObservableCollection)
- [x] Dialog methods (`ShowSettingsDialog`, `ShowTitlesDialog`, `ShowIssuesDialog`) made async
- [x] `BindingOperations.EnableCollectionSynchronization` removed
- [x] All commands, business logic, sort/filter logic preserved

### TitlesViewModel.cs - Medium effort
- [x] Replace `DevExpress.Mvvm` with `Finalspace.Onigiri.MVVM`
- [x] Remove `System.Windows`, `System.Windows.Data`
- [x] `Visibility LoadingWindowVisibility` -> `bool IsLoading`
- [x] `ICollectionView TitlesView` -> `ImmutableObservableCollection<Title> FilteredTitles`
- [x] `CollectionViewSource.GetDefaultView` -> direct collection management
- [x] `ListCollectionView.CustomSort = new TitleSorter()` -> LINQ sort
- [x] `TitlesView.Filter` -> filter applied in `RefreshTitles()` method
- [x] `IDispatcherService` -> removed
- [x] `BackgroundWorker` -> `Task.Run()`
- [x] Timer-based filter debouncing preserved

### IssuesViewModel.cs - Medium effort
- [x] Replace `DevExpress.Mvvm` with `Finalspace.Onigiri.MVVM`
- [x] Remove `System.Windows.Data`
- [x] `ICollectionView IssuesView` -> `ImmutableObservableCollection<Issue> Issues`
- [x] `CollectionViewSource.GetDefaultView` -> direct collection
- [x] `ListCollectionView.CustomSort = new IssuesSorter()` -> LINQ sort
- [x] `SetIssues()` -> populate ImmutableObservableCollection directly
- [x] `CmdSelectTitle` updated for async dialog service

### TestMainViewModel.cs
- [x] Port from WPF - extends MainViewModel with design-time data

## 2D. New Service Interfaces
- [x] `IFileDialogService.cs` - ShowOpenFileDialogAsync, ShowSaveFileDialogAsync
- [x] `IFolderDialogService.cs` - ShowFolderDialogAsync

## Build Verification (Phase 2)
- [x] `dotnet build OnigiriAvalonia/OnigiriAvalonia.csproj -c Debug -p:Platform=x64` compiles

---

## Phase 3: Service Layer Migration

### 3A. Service Interface Porting
- [x] `IOnigiriDialogService.cs` - async methods (ShowTitlesDialogAsync, ShowIssuesDialogAsync, ShowConfigurationDialogAsync)
- [x] `IProcessStarterService.cs` - 1 method: Start(executable, args)
- [x] `IThemeManagerService.cs` - Property: CurrentTheme, Method: ChangeTheme
- [x] `IDarkModeDetectionService.cs` - Event: DarkModeChanged, Property: IsDarkMode

### 3B. Service Implementation Changes
- [x] `DefaultOnigiriDialogService.cs` - sync ShowDialog() -> async ShowDialog<bool?>(_owner)
- [x] `DefaultProcessStarterService.cs` - added UseShellExecute = true for cross-platform
- [x] `AvaloniaThemeManagerService.cs` - replaces MaterialDesignThemes PaletteHelper + P/Invoke with RequestedThemeVariant + color dict swap
- [x] `AvaloniaDarkModeDetectionService.cs` - replaces WMI ManagementEventWatcher + Registry with ActualThemeVariantChanged

### 3C. New File Dialog Service Implementation
- [x] `IFileDialogService.cs` + `AvaloniaFileDialogService.cs` - StorageProvider.OpenFilePickerAsync / SaveFilePickerAsync with ParseFilter
- [x] `IFolderDialogService.cs` + `AvaloniaFolderDialogService.cs` - StorageProvider.OpenFolderPickerAsync

### 3D. Service Registration (App.axaml.cs)
- [x] All 6 services registered: IProcessStarterService, IThemeManagerService, IDarkModeDetectionService, IOnigiriDialogService, IFileDialogService, IFolderDialogService

### Build Verification (Phase 3)
- [x] `dotnet build OnigiriAvalonia/OnigiriAvalonia.csproj -c Debug -p:Platform=x64` compiles

---

## Phase 4: View Migration (XAML -> AXAML)

### 4A. Prerequisites
- [x] Add `Avalonia.Controls.DataGrid` NuGet package to csproj
- [x] Add `Avalonia.Controls.ItemsRepeater` 11.1.5 NuGet package (provides ItemsRepeater + UniformGridLayout + WrapLayout)
- [x] Add DataGrid Fluent theme to `App.axaml`
- [x] Create `ThemeToBoolConverter` (needed for MainWindow theme menu)
- [x] Create `BoolToImageSourceConverter` (needed for IssuesWindow solved icon)

### 4B. MainWindow.axaml - High effort
- [x] Remove Frame/Page navigation - inline CardListView UserControl
- [x] Remove DevExpress behaviors (DispatcherService, OpenFileDialogService, SaveFileDialogService)
- [x] EventTriggerBehavior for OnLoaded command (Avalonia.Xaml.Behaviors)
- [x] Menu system with avares:// icon URIs
- [x] Theme submenu with MainTheme command parameters
- [x] Sort/Filter submenus
- [x] Sorting GroupBox with ComboBox + CheckBox (IsVisible binding replaces BoolToVisibility)
- [x] Filtering GroupBox with ComboBox + TextBox with Watermark (replaces StringToVisibilityConverter)
- [x] Status bar as DockPanel (replaces WPF StatusBar + StatusBarItem)
- [x] Loading overlay with ProgressBar (replaces LoadingBarControl placeholder until Phase 5)
- [x] Code-behind: minimal (service registration + DataContext moved to App.axaml.cs)

### 4C. CardListView.axaml (new - replaces CardListPage.xaml)
- [x] Page -> UserControl conversion
- [x] ItemsRepeater + UniformGridLayout (replaces VirtualizingWrapPanel) - virtualized
- [x] Anime card DataTemplate with placeholder (Phase 5: replace with AnimeCardControl)
- [x] Binds to MainViewModel.Animes (ImmutableObservableCollection)

### 4D. ConfigWindow.axaml - Low effort
- [x] Search paths ListBox with Add/Remove buttons
- [x] Apply/Close buttons with code-behind Click handlers (Close(true/false))
- [x] Removed DevExpress FolderBrowserDialogService behavior

### 4E. TitlesWindow.axaml - Medium effort
- [x] ListView+GridView -> DataGrid with Aid/Name/Type/Lang columns
- [x] Filter TextBox with Enter key handler (code-behind KeyUp -> StartRefreshTimer)
- [x] Type ComboBox filter
- [x] Loading overlay with indeterminate ProgressBar
- [x] Apply/Cancel buttons with code-behind dialog result

### 4F. IssuesWindow.axaml - Medium effort
- [x] ListView+GridView -> DataGrid with State/Path/Kind/Message/Action columns
- [x] State column: two Image controls with IsVisible toggle (replaces WPF DataTrigger)
- [x] Action column: Fix Name button with $parent[Window] binding to CmdSelectTitle

### 4G. DetailsWindow.axaml - Trivial (new file)
- [x] Title TextBlock + Image placeholder + TabControl (Basic/Description tabs)

### Build Verification (Phase 4)
- [x] `dotnet build OnigiriAvalonia/OnigiriAvalonia.csproj -c Debug -p:Platform=x64` compiles (0 errors, 0 warnings)
