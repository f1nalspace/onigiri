using Finalspace.Onigiri.AniDB;
using System;
using System.IO;
using log4net;
using System.Reflection;
using Finalspace.Onigiri.Utils;
using Finalspace.Onigiri.Models;
using System.Security.Principal;
using Finalspace.Onigiri.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Finalspace.Onigiri.Events;
using System.Linq;
using Finalspace.Onigiri.Security;
using System.Collections.Immutable;
using Finalspace.Onigiri.Types;
using Finalspace.Onigiri.Storage;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Finalspace.Onigiri.Media;

namespace Finalspace.Onigiri
{
    public class OnigiriService
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IUserIdentity _userIdentity;
        private readonly IIdentityImpersonator _identityImpersonator;

        public Config Config { get; }
        public Titles Titles { get; }
        public Animes Animes { get; }
        public Issues Issues { get; }

        public OnigiriService()
        {
            _userIdentity = new Win32UserIdentity();
            _identityImpersonator = new Win32IdentityImpersonator();

            if (!Directory.Exists(OnigiriPaths.AppSettingsPath))
                Directory.CreateDirectory(OnigiriPaths.AppSettingsPath);

            if (!Directory.Exists(OnigiriPaths.PersistentPath))
                Directory.CreateDirectory(OnigiriPaths.PersistentPath);

            Config = new Config();
            Titles = new Titles();
            Animes = new Animes();
            Issues = new Issues();
        }

        /// <summary>
        /// Finds the anime id (aid) from the given title.
        /// The title is typically the name of the folder.
        /// </summary>
        /// <remarks>Uses the 'SearchTypeLanguages' for probing titles</remarks>
        /// <param name="name">The title of the anime without special characters</param>
        /// <returns>Found aid or zero</returns>
        public Title FindTitle(string name)
        {
            Title result = null;
            foreach (SearchTypeLanguage stl in Config.SearchTypeLanguages)
            {
                if (!string.IsNullOrEmpty(stl.Type) &&
                    !string.IsNullOrEmpty(stl.Lang))
                {
                    Title found = Titles.FindTitle(name, stl.Type, stl.Lang);
                    if (found != null)
                    {
                        log.Debug($"Found title {name} for name '{name}', type='{stl.Type}', lang='{stl.Lang}'!");
                        result = found;
                        break;
                    }
                }
                else
                    log.Error($"Invalid search type language '{stl.Type}/{stl.Lang}'!");
            }
            return result;
        }

        private static string ResolveSearchPath(SearchPath searchPath)
        {
            string path = searchPath.Path;
            if (!string.IsNullOrEmpty(searchPath.DriveName))
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                foreach (DriveInfo drive in drives)
                {
                    if (drive.IsReady && drive.VolumeLabel.Equals(searchPath.DriveName))
                    {
                        DirectoryInfo dir = new DirectoryInfo(path);
                        path = path.Remove(0, dir.Root.FullName.Length);
                        path = Path.Combine(drive.RootDirectory.FullName, path);
                        break;
                    }
                }
            }
            return path;
        }

        private Anime GetOrUpdate(string sourcePath, UpdateFlags flags, StatusChangedEventHandler statusChanged)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentNullException(nameof(sourcePath));
            if (flags == UpdateFlags.None)
                throw new ArgumentException($"Flags must be set to something else other than zero", nameof(flags));

            log.Info($"Update anime from path '{sourcePath}' with flags {flags}");

            DirectoryInfo sourceDir = new DirectoryInfo(sourcePath);
            if (!sourceDir.Exists)
            {
                log.Warn($"Anime directory '{sourcePath}' could not be found!");
                return (null);
            }

            if (sourceDir.Attributes.HasFlag(FileAttributes.System))
            {
                log.Warn($"Anime directory '{sourcePath}' is a system directory!");
                return (null);
            }

            string folderName = sourceDir.Name;
            string cleanTitleName = AnimeUtils.GetCleanAnimeName(folderName);
            string animeXmlFilePath = Path.Combine(sourcePath, OnigiriPaths.AnimeXMLDetailsFilename);
            string animeAidFilePath = Path.Combine(sourcePath, OnigiriPaths.AnimeAIDFilename);

            ulong aid = 0;

            // Get AID from text file
            if (File.Exists(animeAidFilePath))
            {
                string text = File.ReadAllText(animeAidFilePath);
                if (ulong.TryParse(text, out ulong tmp))
                    aid = tmp;
            }

            // Get AID from xml file
            if (aid == 0 && File.Exists(animeXmlFilePath))
            {
                Anime tmpAnime = new Anime();
                tmpAnime.LoadFromAnimeXML(animeXmlFilePath, true);
                aid = tmpAnime.Aid;
            }

            // Still no aid found, use title as last fallback
            if (aid == 0)
            {
                // TODO:  This is useless to remove all special chars from the foldername, because names cannot have special characters anyway.
                log.Debug($"Find title by name '{cleanTitleName}' in folder '{folderName}'");
                Title title = FindTitle(cleanTitleName);
                if (title != null)
                {
                    log.Debug($"Found title by name '{cleanTitleName}' in folder '{folderName}', got result: {title}");
                    aid = title.Aid;
                }
                else
                {
                    // Still no aid, dont update anything
                    Issues.Add(IssueKind.TitleNotFound, $"No title for anime '{cleanTitleName}' found!", sourcePath, folderName);
                    log.Warn($"No title for anime '{sourcePath}' as '{cleanTitleName}' - no aid found!");
                    return (null);
                }
            }

            // Download anime details xml
            if (flags.HasFlag(UpdateFlags.DownloadDetails) || flags.HasFlag(UpdateFlags.ForceDetails))
            {
                bool updateDetails = flags.HasFlag(UpdateFlags.ForceDetails) || !File.Exists(animeXmlFilePath);
                if (updateDetails)
                {
                    log.Info($"Request details for aid {aid} as '{cleanTitleName}'");
                    TextContent content = HttpApi.RequestAnime(aid);
                    if (content != null && !string.IsNullOrEmpty(content.Text))
                    {
                        log.Info($"Save details xml file a'{animeXmlFilePath}'");
                        content.SaveToFile(animeXmlFilePath);
                    }
                    else
                        log.Warn($"Failed requesting  by aid {aid} from anidb'!");
                }
            }

            Anime anime = LoadAnimeFromSourceDir(sourceDir, statusChanged);
            Debug.Assert(anime != null);

            // Download picture
            string imageFilePath = FindImage(sourceDir);
            if (flags.HasFlag(UpdateFlags.DownloadPicture) || flags.HasFlag(UpdateFlags.ForcePicture))
            {
                bool updatePicture = flags.HasFlag(UpdateFlags.ForcePicture) || string.IsNullOrEmpty(imageFilePath);
                if (updatePicture)
                {
                    if (string.IsNullOrEmpty(anime.Picture))
                    {
                        log.Warn($"Missing picture file in anime '{anime}'!");
                        Issues.Add(IssueKind.PictureUndefined, $"No picture name in anime '{anime}' defined", sourceDir.FullName);
                    }
                    else
                    {
                        imageFilePath = Path.Combine(sourceDir.FullName, anime.Picture);
                        HttpApi.DownloadPicture(anime.Picture, imageFilePath);
                        if (!File.Exists(imageFilePath))
                        {
                            log.Warn($"Failed downloading picture '{anime.Picture}' to '{imageFilePath}' for '{anime}'!");
                            Issues.Add(IssueKind.PictureNotFound, $"The picture '{anime.Picture}' does not exists for anime '{anime}'", sourceDir.FullName);
                        }                       
                    }
                }
            }

            if ((anime.Image == null || flags.HasFlag(UpdateFlags.ForcePicture) || flags.HasFlag(UpdateFlags.DownloadPicture)) && 
                !string.IsNullOrWhiteSpace(imageFilePath) && 
                File.Exists(imageFilePath))
            {
                // TODO(tspaete): More robust image file read
                byte[] imageData = File.ReadAllBytes(imageFilePath);
                if (imageData != null && imageData.Length > 0)
                {
                    string filename = Path.GetFileName(imageFilePath);
                    anime.Image = new AnimeImage(filename, imageData.ToImmutableArray());
                }
                else
                {
                    log.Warn($"Failed reading picture file '{imageFilePath}' for anime '{anime}'!");
                    Issues.Add(IssueKind.PictureNotFound, $"The picture '{imageFilePath}' failed to load for anime '{anime}'", sourceDir.FullName);
                }
            }

            return (anime);
        }

        public void ClearIssues()
        {
            Issues.Clear();
        }

        public void UpdateSources(UpdateFlags flags, StatusChangedEventHandler statusChanged = null)
        {
            if (flags == UpdateFlags.None)
                throw new ArgumentException($"Flags must be set to something else other than zero", nameof(flags));

            log.Info($"Update animes with flags {flags}");

            // Download anime titles dump raw file from anidb if needed
            if (flags.HasFlag(UpdateFlags.DownloadTitles))
                ReadTitles(statusChanged, true);

            ConcurrentBag<Anime> list = new ConcurrentBag<Anime>();

            using (IImpersonationContext imp = _identityImpersonator.Impersonate(_userIdentity))
            {
                List<DirectoryInfo> animeDirs = new List<DirectoryInfo>();
                foreach (SearchPath searchPath in Config.SearchPaths)
                {
                    string path = ResolveSearchPath(searchPath);
                    if (string.IsNullOrEmpty(path))
                    {
                        log.Warn($"Search path '{path}' is empty!");
                        continue;
                    }
                    DirectoryInfo searchDir = new DirectoryInfo(path);
                    if (!searchDir.Exists)
                    {
                        Issues.Add(IssueKind.SearchPathMissing, "Search path not found!", path);
                        log.Warn($"Search path '{path}' not found!");
                        continue;
                    }

                    log.Info($"Get anime folders from search dir '{searchDir.FullName}'");
                    DirectoryInfo[] dirs = searchDir.GetDirectories("*", SearchOption.TopDirectoryOnly);
                    foreach (DirectoryInfo dir in dirs)
                    {
                        animeDirs.Add(dir);
                    }
                }

                int count = 0;
                int totalDirCount = animeDirs.Count;

                statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = $"Update {totalDirCount} animes" });

                int threadCount = Config.MaxThreadCount;
                //threadCount = 1;
                ParallelOptions poptions = new ParallelOptions() { MaxDegreeOfParallelism = threadCount };
                Parallel.ForEach(animeDirs, poptions, (animeDir) =>
                {
                    int c = Interlocked.Increment(ref count);
                    int percentage = (int)((c / (double)totalDirCount) * 100.0);
                    statusChanged?.Invoke(this, new StatusChangedArgs() { Percentage = percentage, Header = $"{c} of {totalDirCount} done" });
                    Anime anime = GetOrUpdate(animeDir.FullName, flags, statusChanged);
                    if (anime != null)
                        list.Add(anime);
                });
            }

            Anime[] sorted = list.OrderBy(a => a.FoundPath).ToArray();

            Animes.Set(sorted);
        }

        private static string FindImage(DirectoryInfo dir)
        {
            FileInfo[] files = dir.GetFiles("*", SearchOption.TopDirectoryOnly);
            DateTime? bestDate = null;
            FileInfo bestImage = null;
            foreach (FileInfo file in files)
            {
                if (!file.Extension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase) &&
                    !file.Extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase))
                    continue;
                if (bestDate == null || file.LastWriteTime > bestDate)
                {
                    bestDate = file.LastWriteTime;
                    bestImage = file;
                }
            }
            if (bestImage != null)
                return bestImage.FullName;
            return null;
        }

        public static readonly HashSet<string> MediaFileExtensions = new HashSet<string>
        {
            ".avi",
            ".mkv",
            ".ogm",
            ".ogv",
            ".mpg",
            ".mpeg",
            ".mp4"
        };

        private AnimeMediaFile[] GetExtendedMediaFiles(DirectoryInfo dir)
        {
            ConcurrentBag<AnimeMediaFile> list = new ConcurrentBag<AnimeMediaFile>();

            FileInfo[] files = dir
                .GetFiles("*", SearchOption.TopDirectoryOnly)
                .Where(f => !f.Attributes.HasFlag(FileAttributes.System))
                .Where(f => MediaFileExtensions.Contains(f.Extension.ToLower()))
                .ToArray();

            int threadCount = Math.Min(4, Config.MaxThreadCount);
            ParallelOptions poptions = new ParallelOptions() { MaxDegreeOfParallelism = threadCount };
            Parallel.ForEach(files, poptions, async (file) =>
            {
                MediaInfo info = await MediaInfoParser.Parse(file);
                AnimeMediaFile animeMediaFile = new AnimeMediaFile()
                {
                    FileName = file.Name,
                    FileSize = (ulong)file.Length,
                    Info = info,
                };
                list.Add(animeMediaFile);
            });

            AnimeMediaFile[] result = list.OrderBy(f => f.FileName).ToArray();

            return result;
        }

        private static Title CreateFallbackTitle(ulong aid, string folderName)
        {
            // TODO:  This is useless to remove all special chars from the foldername, because names cannot have special characters anyway.
            string cleanTitleName = AnimeUtils.GetCleanAnimeName(folderName);
            Title result = new Title()
            {
                Aid = aid,
                Lang = "en",
                Type = "main",
                Name = cleanTitleName
            };
            return result;
        }

        /// <summary>
        /// Loads an <see cref="Anime"/> from the specified source <see cref="DirectoryInfo"/>.
        /// </summary>
        /// <param name="sourceDir">The source <see cref="DirectoryInfo"/>.</param>
        /// <param name="statusChanged">The <see cref="StatusChangedEventHandler"/>.</param>
        /// <returns>The resulting <see cref="Anime"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the specified <paramref name="sourceDir"/> is <c>null</c>.</exception>
        private Anime LoadAnimeFromSourceDir(DirectoryInfo sourceDir, StatusChangedEventHandler statusChanged)
        {
            if (sourceDir == null)
                throw new ArgumentNullException(nameof(sourceDir));

            Stopwatch watch = new Stopwatch();

            // TODO:  This is useless to remove all special chars from the foldername, because names cannot have special characters anyway.
            string cleanTitleName = AnimeUtils.GetCleanAnimeName(sourceDir.Name);

            // Find AID
            ulong aid = 0;
            statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = $"Find aid from: {sourceDir.Name}" });
            log.Info($"Find title for anime '{cleanTitleName}' in folder '{sourceDir.FullName}'");
            watch.Restart();
            Title foundTitle = FindTitle(cleanTitleName);
            watch.Stop();
            log.Debug($"Find title for anime '{cleanTitleName}' in folder '{sourceDir.FullName}' took {watch.Elapsed.TotalSeconds} secs");
            if (foundTitle == null)
                log.Warn($"No title found for anime '{cleanTitleName}' in folder '{sourceDir.FullName}'!");
            else
                aid = foundTitle.Aid;

            // Push aid and path
            Anime result = new Anime()
            {
                Aid = aid,
                FoundPath = sourceDir.FullName
            };

            // Load anime details into the anime
            string animeXmlFilePath = Path.Combine(sourceDir.FullName, OnigiriPaths.AnimeXMLDetailsFilename);
            statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = $"Load details: {sourceDir.Name}" });
            if (!File.Exists(animeXmlFilePath))
            {
                log.Warn($"Not found anime details file '{animeXmlFilePath}', aid='{aid}', title='{cleanTitleName}'!");
            }
            else
            {
                watch.Restart();
                result.LoadFromAnimeXML(animeXmlFilePath);
                watch.Stop();
                log.Debug($"Loading anime details file '{animeXmlFilePath}' took {watch.Elapsed.TotalSeconds} secs");
                if (result.Aid > 0)
                    aid = result.Aid;
            }

            // Title fallbacks
            Title databaseTitle = Titles.GetTitle(aid);
            if (string.IsNullOrEmpty(result.MainTitle))
            {
                if (databaseTitle != null)
                    result.Titles.Add(databaseTitle);
                else
                    result.Titles.Add(CreateFallbackTitle(aid, sourceDir.Name));
            }

            // Find image
            statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = $"Find image: {sourceDir.Name}" });
            log.Info($"Find image for anime '{cleanTitleName}' in folder '{sourceDir.FullName}'");
            watch.Restart();
            string imageFilePath = FindImage(sourceDir);
            watch.Stop();
            log.Debug($"Find image in folder '{sourceDir.FullName}' took {watch.Elapsed.TotalSeconds} secs");
            if (!string.IsNullOrEmpty(imageFilePath) && File.Exists(imageFilePath))
            {
                // TODO(tspaete): More robust image file read
                byte[] imageData = File.ReadAllBytes(imageFilePath);
                if (imageData != null && imageData.Length > 0)
                {
                    string filename = Path.GetFileName(imageFilePath);
                    result.Image = new AnimeImage(filename, imageData.ToImmutableArray());
                }
                else
                {
                    log.Warn($"Failed reading picture file '{imageFilePath}' for anime '{result}'!");
                    Issues.Add(IssueKind.PictureNotFound, $"The picture '{imageFilePath}' failed to load for anime '{result}'", sourceDir.FullName);
                }
            }
            else
                log.Warn($"Not found anime image '{result.Picture}' aid='{aid}', title='{cleanTitleName}'!");

            // Find additional data
            string addonFilePath = Path.Combine(sourceDir.FullName, OnigiriPaths.AnimeXMLAddonFilename);
            if (File.Exists(addonFilePath))
            {
                log.Info($"Loading addon data file '{addonFilePath}'");
                watch.Restart();
                result.AddonData.LoadFromFile(addonFilePath);
                watch.Stop();
                log.Debug($"Loading addon data file '{addonFilePath}' took {watch.Elapsed.TotalSeconds} secs");
            }

            // Find media files
            statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = $"Find media files in: {sourceDir.Name}" });
            log.Info($"Find media files from path '{sourceDir.FullName}'");
            watch.Restart();
            IEnumerable<AnimeMediaFile> extendedMediaFiles = GetExtendedMediaFiles(sourceDir);
            result.ExtendedMediaFiles = extendedMediaFiles.ToList();
            result.MediaFiles = extendedMediaFiles.Select(m => m.FileName).ToList();
            watch.Stop();
            log.Debug($"Found {result.MediaFiles.Count} media files in path '{sourceDir.FullName}', took {watch.Elapsed.TotalSeconds} secs");

            return result;
        }

        /// <summary>
        /// Loads all <see cref="Animes"/> and <see cref="Issues"/> from the specified <see cref="IAnimeStorage"/>.
        /// </summary>
        /// <param name="storage">The <see cref="IAnimeStorage"/>.</param>
        /// <param name="statusChanged">The <see cref="StatusChangedEventHandler"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        public void Load(IAnimeStorage storage, StatusChangedEventHandler statusChanged)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            using (IImpersonationContext imp = _identityImpersonator.Impersonate(_userIdentity))
            {
                statusChanged?.Invoke(this, new StatusChangedArgs() { Header = $"Load from '{storage}'", Subject = "", Percentage = -1 });

                Stopwatch watch;

                watch = Stopwatch.StartNew();
                log.Info($"Load animes from '{storage}' animes");
                AnimeStorageData data = storage.Load(statusChanged);
                watch.Stop();
                log.Debug($"Load animes from storage '{storage}' took {watch.Elapsed.TotalSeconds} secs");

                watch.Restart();
                log.Debug($"Refresh {data.Animes.Length} animes");
                statusChanged?.Invoke(this, new StatusChangedArgs() { Header = $"Refresh to '{data.Animes.Length}' animes", Subject = "", Percentage = -1 });

                Anime[] sortedAnimes = data.Animes.OrderBy(a => a.FoundPath).ToArray();
                Animes.Set(sortedAnimes);

                watch.Stop();
                log.Debug($"Refresh animes from storage '{storage}' took {watch.Elapsed.TotalSeconds} secs");
            }
        }

        /// <summary>
        /// Saves the <see cref="Animes"/> to the specified <see cref="IAnimeStorage"/>.
        /// </summary>
        /// <param name="storage">The <see cref="IAnimeStorage"/>.</param>
        /// <param name="statusChanged">The <see cref="StatusChangedEventHandler"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when the specified <paramref name="storage"/> is <c>null</c>.</exception>
        public void Save(IAnimeStorage storage, StatusChangedEventHandler statusChanged)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            using (IImpersonationContext imp = _identityImpersonator.Impersonate(_userIdentity))
            {
                statusChanged?.Invoke(this, new StatusChangedArgs() { Header = $"Save to '{storage}'", Subject = "", Percentage = -1 });

                ImmutableArray<Anime> animes = Animes.Items.ToImmutableArray();

                ImmutableArray<Title> titles = Titles.Items.ToImmutableArray();

                AnimeStorageData data = new AnimeStorageData(animes, titles);

                Stopwatch watch = Stopwatch.StartNew();
                log.Info($"Save '{animes}' animes to storage '{storage}'");
                storage.Save(data, statusChanged);
                watch.Stop();
                log.Info($"Save '{animes}' animes to storage '{storage}' took {watch.Elapsed.TotalSeconds} secs");
            }
        }

        /// <summary>
        /// Saves the specified <paramref name="anime"/> to the <paramref name="storage"/>.
        /// </summary>
        /// <param name="anime">The <see cref="Anime"/>.</param>
        /// <param name="storage">The <see cref="IAnimeStorage"/>.</param>
        /// <param name="statusChanged">The <see cref="StatusChangedEventHandler"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when the specified <paramref name="storage"/> is <c>null</c>.</exception>
        public void Save(Anime anime, IAnimeStorage storage, StatusChangedEventHandler statusChanged)
        {
            if (anime == null)
                throw new ArgumentNullException(nameof(anime));
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            using (IImpersonationContext imp = _identityImpersonator.Impersonate(_userIdentity))
            {
                statusChanged?.Invoke(this, new StatusChangedArgs() { Header = $"Save to '{storage}'", Subject = "", Percentage = -1 });

                Stopwatch watch = Stopwatch.StartNew();
                log.Info($"Save anime '{anime}' to storage '{storage}'");
                storage.Save(anime, statusChanged);
                watch.Stop();
                log.Info($"Save anime '{anime}' to storage '{storage}' took {watch.Elapsed.TotalSeconds} secs");
            }
        }

        private void ReadTitles(StatusChangedEventHandler statusChanged, bool overwrite)
        {
            string xmlFilePath = OnigiriPaths.AnimeTitlesDumpXMLFilePath;
            string rawFilePath = OnigiriPaths.AnimeTitlesDumpRawFilePath;

            // Download anime titles dump raw file from anidb if needed
            bool updateTitlesRaw = !File.Exists(rawFilePath);
            if (updateTitlesRaw || overwrite)
            {
                log.Info($"Download anime titles dump to '{rawFilePath}'");
                statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = "Downloading titles database", Percentage = -1 });
                HttpApi.DownloadTitlesDump(rawFilePath);
            }
            if (!File.Exists(rawFilePath))
                log.Warn($"Not found anime titles dump file '{rawFilePath}'!");
            else
                log.Debug($"Use already existing anime titles dump file '{rawFilePath}'");

            // Decompress anime dump raw file if needed and save it to disk
            bool updateTitlesXML = updateTitlesRaw || overwrite || !File.Exists(xmlFilePath);
            if (File.Exists(rawFilePath) && updateTitlesXML)
            {
                log.Info($"Decompress anime titles dump '{rawFilePath}' to '{xmlFilePath}'");
                statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = "Decompress titles database", Percentage = -1 });
                FileUtils.DecompressFile(rawFilePath, xmlFilePath);
            }
            else if (File.Exists(xmlFilePath))
                log.Debug($"Use already existing anime titles xml file '{xmlFilePath}'");

            // Read anime titles
            if (File.Exists(xmlFilePath))
            {
                log.Info($"Parse titles dump xml file '{xmlFilePath}'");
                statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = "Parse titles database", Percentage = -1 });
                Titles.ReadFromFile(xmlFilePath);
            }
            else
                log.Warn($"Not found anime titles xml file '{xmlFilePath}'!");

            // Print out anime title statistics
            log.Info($"Found {Titles.Items.Count} anime titles total");
            log.Info($"Found {Titles.AIDCount} animes total");
        }

        public void Startup(StatusChangedEventHandler statusChanged = null)
        {
            log.Info("Started service");
            statusChanged?.Invoke(this, new StatusChangedArgs() { Header = "Startup", Subject = "", Percentage = -1 });

            string configFilePath = OnigiriPaths.ConfigFilePath;

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            log.Info($"Use identity: {identity.Name}");

            // Read config
            if (File.Exists(configFilePath))
            {
                log.Info($"Loading config file '{configFilePath}'");
                statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = "Loading config file", Percentage = -1 });
                Config.LoadFromFile(configFilePath);
            }
            else
                log.Warn($"Not found config file '{configFilePath}'!");

            // Download anime titles dump raw file from anidb if needed
            ReadTitles(statusChanged, false);
        }

        public void SaveConfig()
        {
            string configFilePath = OnigiriPaths.ConfigFilePath;
            log.Info($"Saving config file '{configFilePath}'");
            Config.SaveToFile(configFilePath);
        }
    }
}
