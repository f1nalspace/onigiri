# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Onigiri is a Windows-only WPF desktop application for managing an anime database. It uses the AniDB HTTP XML API to retrieve anime metadata (details, ratings, titles, images) and supports browsing, searching, and tracking watch states. Written in C# targeting .NET 9, x64 only.

## Build & Test Commands

```bash
# Build entire solution
dotnet build Onigiri.sln -c Debug -p:Platform=x64

# Build a specific project
dotnet build OnigiriCore/OnigiriCore.csproj -c Debug -p:Platform=x64

# Run tests (MSTest)
dotnet test OnigiriTests/OnigiriTests.csproj -p:Platform=x64

# Run a single test
dotnet test OnigiriTests/OnigiriTests.csproj -p:Platform=x64 --filter "FullyQualifiedName~TestMethodName"
```

Build output goes to `Build/<ProjectName>/x64-Debug/` or `x64-Release/`.

## Solution Architecture

Five projects, all under namespace `Finalspace.Onigiri`:

- **OnigiriCore** — Platform-independent core library (`net9.0`). Contains all domain logic:
  - `OnigiriService` — Central service orchestrating anime discovery, title lookup, AniDB API calls, and storage
  - `AniDB/HttpApi` — AniDB HTTP API client (anime details, title dumps, images). Enforces a 3-second delay between requests (AniDB rate limit)
  - `Models/` — Domain models: `Anime` (extends `BindableBase` from DevExpress MVVM), `Title`, `Episode`, `Rating`, `Config`, `AnimeImage`, `Tag`, etc.
  - `Storage/` — `IAnimeStorage` with `DatabaseAnimeFilesStorage` and `FolderAnimeFilesStorage` implementations. Anime data serialized as XML
  - `Media/` — FFmpeg-based media info parsing via `FFmpeg.AutoGen` (video/audio/subtitle stream analysis)
  - `Security/` — `IUserService`/`IUserIdentity` abstractions for user impersonation

- **OnigiriPlatform** — Platform-specific implementations (`net9.0`). Currently Windows-only: `Win32UserService`, `Win32UserIdentity`, `Win32ImpersonationContext`. `OnigiriUserServiceFactory` creates the appropriate `IUserService` at runtime.

- **Onigiri** — WPF GUI app (`net9.0-windows`). MVVM pattern with ViewModels and Views. Uses MaterialDesignThemes and VirtualizingWrapPanel. Entry point: `App.xaml` / `Finalspace.Onigiri.App`.

- **OnigiriConsole** — Console app (`net9.0-windows`) for CLI operations (listing titles, animes). Entry point: `Program.cs`.

- **OnigiriTests** — MSTest unit tests. References OnigiriCore.

### Dependency graph

```
OnigiriCore (no project deps)
  ↑
OnigiriPlatform (depends on OnigiriCore)
  ↑
Onigiri (depends on OnigiriCore + OnigiriPlatform)
OnigiriConsole (depends on OnigiriCore + OnigiriPlatform)
OnigiriTests (depends on OnigiriCore)
```

## Key Conventions

- Per-anime data stored in folder alongside media files: `.anime.xml` (AniDB details), `.adata.xml` (additional user data), `aid.txt` (anime ID)
- App settings stored at `~/Documents/Onigiri/` (config, title dumps, persistent cache)
- Logging via log4net throughout all projects
- C# language version 10.0; `AllowUnsafeBlocks` enabled in OnigiriCore and OnigiriPlatform (FFmpeg interop)
- The `ffmpeg/` directory contains pre-built FFmpeg DLLs copied to output on build
