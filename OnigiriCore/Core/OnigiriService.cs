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
using Finalspace.Onigiri.Persistence;
using Finalspace.Onigiri.Events;
using System.Linq;
using Finalspace.Onigiri.Extensions;
using Finalspace.Onigiri.Security;

namespace Finalspace.Onigiri.Core
{
    public class OnigiriService
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const string AnimeXMLDetailsFilename = ".anime.xml";
        public const string AnimeXMLAddonFilename = ".adata.xml";
        public const string AnimeAIDFilename = "aid.txt";

        private WindowsIdentity _activeIdentity;
        private readonly string _appSettingsPath;
        private readonly string _configFilePath;
        private readonly string _persistentPath;
        private readonly string _animeTitlesDumpRawFilePath;
        private readonly string _animeTitlesDumpXMLFilePath;

        public Config Config { get; }
        public Titles Titles { get; }
        private readonly object _animesLock = new object();
        public Animes Animes { get; }
        public Issues Issues { get; }
        public IAnimeCache Cache { get; }

        public OnigiriService()
        {
            _activeIdentity = WindowsIdentity.GetCurrent();
            _appSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Onigiri");
            if (!Directory.Exists(_appSettingsPath)) Directory.CreateDirectory(_appSettingsPath);
            _persistentPath = Path.Combine(_appSettingsPath, "PersistentCache");
            if (!Directory.Exists(_persistentPath)) Directory.CreateDirectory(_persistentPath);
            _configFilePath = Path.Combine(_appSettingsPath, "config.xml");
            _animeTitlesDumpRawFilePath = Path.Combine(_appSettingsPath, "animetitles.xml.gz");
            _animeTitlesDumpXMLFilePath = Path.Combine(_appSettingsPath, "animetitles.xml");
            Config = new Config();
            Cache = new FolderAnimeFilesCache(Config.MaxThreadCount, _persistentPath);
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

        private string ResolveSearchPath(SearchPath searchPath)
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

        public void Update(string path, UpdateFlags flags, StatusChangedEventHandler statusChanged)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (flags == UpdateFlags.None)
                throw new ArgumentException($"Flags must be set to something else other than zero", nameof(flags));

            log.Info($"Update anime from path '{path}' with flags {flags}");

            DirectoryInfo dir = new DirectoryInfo(path);
            if (!dir.Exists)
            {
                log.Warn($"Anime directory '{path}' could not be found. Skipping path.");
                return;
            }

            if (dir.Attributes.HasFlag(FileAttributes.System))
            {
                log.Warn($"Skip system directory '{path}'");
                return;
            }

            string folderName = dir.Name;
            string cleanTitleName = AnimeUtils.GetCleanAnimeName(folderName);
            string animeXmlFilePath = Path.Combine(path, AnimeXMLDetailsFilename);
            string animeAidFilePath = Path.Combine(path, AnimeAIDFilename);

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
                    Issues.Add(IssueKind.TitleNotFound, $"No title for anime '{cleanTitleName}' found!", path, folderName);
                    log.Warn($"No title for anime '{path}' as '{cleanTitleName}' - no aid found!");
                    return;
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

            Anime anime = Aquire(dir, statusChanged);
            Debug.Assert(anime != null);

            // Download picture
            string pictureFile = FindImage(dir);
            if (flags.HasFlag(UpdateFlags.DownloadPicture) || flags.HasFlag(UpdateFlags.ForcePicture))
            {
                bool updatePicture = flags.HasFlag(UpdateFlags.ForcePicture) || string.IsNullOrEmpty(pictureFile);
                if (updatePicture)
                {
                    if (string.IsNullOrEmpty(anime.Picture))
                    {
                        log.Warn($"Missing picture file in anime '{anime}'!");
                        Issues.Add(IssueKind.PictureUndefined, $"No picture name in anime '{anime}' defined", dir.FullName);
                    }
                    else
                    {
                        pictureFile = Path.Combine(dir.FullName, anime.Picture);
                        HttpApi.DownloadPicture(anime.Picture, pictureFile);
                        if (!File.Exists(pictureFile))
                        {
                            log.Warn($"Failed downloading picture '{anime.Picture}' to '{pictureFile}' for '{anime}'!");
                            Issues.Add(IssueKind.PictureNotFound, $"The picture '{anime.Picture}' does not exists for anime '{anime}'", dir.FullName);
                        }
                    }
                }
                else
                    log.Debug($"Picture '{pictureFile}' for '{anime}' already found.");
            }

            if (flags.HasFlag(UpdateFlags.WriteCache))
            {
                statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = $"Write to persistent cache: {anime.MainTitle}" });
                Cache.Serialize(anime, pictureFile);
            }
        }

        public void ClearIssues()
        {
            Issues.Clear();
        }

        public void UpdateAnimes(UpdateFlags flags, StatusChangedEventHandler statusChanged = null)
        {
            // Download anime titles dump raw file from anidb if needed
            if (flags.HasFlag(UpdateFlags.DownloadTitles))
                ReadTitles(statusChanged, true);

            using (IImpersonationContext imp = _activeIdentity.Impersonate())
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
                    log.Info($"Update animes in '{searchDir.FullName}'");
                    DirectoryInfo[] dirs = searchDir.GetDirectories("*", SearchOption.TopDirectoryOnly);
                    foreach (DirectoryInfo dir in dirs)
                    {
                        animeDirs.Add(dir);
                    }
                }

                int count = 0;
                int totalDirCount = animeDirs.Count;
                ParallelOptions poptions = new ParallelOptions() { MaxDegreeOfParallelism = Config.MaxThreadCount };
                Parallel.ForEach(animeDirs, poptions, (dir) =>
                {
                    int c = Interlocked.Increment(ref count);
                    int percentage = (int)((c / (double)totalDirCount) * 100.0);
                    statusChanged?.Invoke(this, new StatusChangedArgs() { Percentage = percentage, Header = $"{c} of {totalDirCount} done" });
                    Update(dir.FullName, flags, statusChanged);
                });
            }
        }

        public string FindImage(DirectoryInfo dir)
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

        readonly static HashSet<string> mediaFileExtensions = new HashSet<string>
        {
            ".avi",
            ".mkv",
            ".ogm",
            ".ogv",
            ".mpg",
            ".mpeg",
            ".mp4"
        };
        public IEnumerable<string> FindMediaFileNames(DirectoryInfo dir)
        {
            FileInfo[] files = dir.GetFiles("*", SearchOption.TopDirectoryOnly);
            foreach (FileInfo file in files)
            {
                string ext = file.Extension.ToLower();
                if (!mediaFileExtensions.Contains(ext))
                    continue;
                yield return file.Name;
            }
        }

        private Title CreateFallbackTitle(ulong aid, string folderName)
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

        public Anime Aquire(DirectoryInfo dir, StatusChangedEventHandler statusChanged)
        {
            Stopwatch watch = new Stopwatch();

            // TODO:  This is useless to remove all special chars from the foldername, because names cannot have special characters anyway.
            string cleanTitleName = AnimeUtils.GetCleanAnimeName(dir.Name);

            // Find AID
            ulong aid = 0;
            statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = $"Find aid from: {dir.Name}" });
            log.Info($"Find title for anime '{cleanTitleName}' in folder '{dir.FullName}'");
            watch.Restart();
            Title foundTitle = FindTitle(cleanTitleName);
            watch.Stop();
            log.Debug($"Find title for anime '{cleanTitleName}' in folder '{dir.FullName}' took {watch.Elapsed.TotalSeconds} secs");
            if (foundTitle == null)
                log.Warn($"No title found for anime '{cleanTitleName}' in folder '{dir.FullName}'!");
            else
                aid = foundTitle.Aid;

            // Push aid and path
            Anime result = new Anime()
            {
                Aid = aid,
                FoundPath = dir.FullName
            };

            // Load anime details into the anime
            string animeXmlFilePath = Path.Combine(dir.FullName, AnimeXMLDetailsFilename);
            statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = $"Load details: {dir.Name}" });
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
                    result.Titles.Add(CreateFallbackTitle(aid, dir.Name));
            }

            // Find image
            statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = $"Find image: {dir.Name}" });
            log.Info($"Find image for anime '{cleanTitleName}' in folder '{dir.FullName}'");
            watch.Restart();
            string imageFile = FindImage(dir);
            watch.Stop();
            log.Debug($"Find image in folder '{dir.FullName}' took {watch.Elapsed.TotalSeconds} secs");
            if (!string.IsNullOrEmpty(imageFile))
                result.ImageFilePath = imageFile;
            else
                log.Warn($"Not found anime image '{result.Picture}' aid='{aid}', title='{cleanTitleName}'!");

            // Find additional data
            string addonFilePath = Path.Combine(dir.FullName, AnimeXMLAddonFilename);
            if (File.Exists(addonFilePath))
            {
                log.Info($"Loading addon data file '{addonFilePath}'");
                watch.Restart();
                result.AddonData.LoadFromFile(addonFilePath);
                watch.Stop();
                log.Debug($"Loading addon data file '{addonFilePath}' took {watch.Elapsed.TotalSeconds} secs");
            }

            // Find media files
            log.Info($"Find media files from path '{dir.FullName}'");
            IEnumerable<string> mediaFiles = FindMediaFileNames(dir);
            result.MediaFiles = new List<string>(mediaFiles);
            log.Debug($"Found {result.MediaFiles.Count} media files in path '{dir.FullName}'");

            return result;
        }

        public void RefreshAnimes(StatusChangedEventHandler statusChanged)
        {
            using (IImpersonationContext imp = _activeIdentity.Impersonate())
            {
                statusChanged?.Invoke(this, new StatusChangedArgs() { Header = $"Load persistent cache", Subject = "", Percentage = -1 });
                Cache.Load(Config, statusChanged);

                statusChanged?.Invoke(this, new StatusChangedArgs() { Header = $"Update list", Subject = "", Percentage = -1 });
                lock (_animesLock)
                {
                    Animes.Clear();
                    IOrderedEnumerable<Anime> sorted = Cache.Animes.OrderBy(a => a.FoundPath);
                    Animes.Items.AddRange(sorted);
                    Animes.RefreshGroups();
                }
            }
        }

        private void ReadTitles(StatusChangedEventHandler statusChanged, bool overwrite)
        {
            // Download anime titles dump raw file from anidb if needed
            bool updateTitlesRaw = !File.Exists(_animeTitlesDumpRawFilePath);
            if (updateTitlesRaw || overwrite)
            {
                log.Info($"Download anime titles dump to '{_animeTitlesDumpRawFilePath}'");
                statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = "Downloading titles database", Percentage = -1 });
                HttpApi.DownloadTitlesDump(_animeTitlesDumpRawFilePath);
            }
            if (!File.Exists(_animeTitlesDumpRawFilePath))
                log.Warn($"Not found anime titles dump file '{_animeTitlesDumpRawFilePath}'!");
            else
                log.Debug($"Use already existing anime titles dump file '{_animeTitlesDumpRawFilePath}'");

            // Decompress anime dump raw file if needed and save it to disk
            bool updateTitlesXML = updateTitlesRaw || overwrite || !File.Exists(_animeTitlesDumpXMLFilePath);
            if (File.Exists(_animeTitlesDumpRawFilePath) && updateTitlesXML)
            {
                log.Info($"Decompress anime titles dump '{_animeTitlesDumpRawFilePath}' to '{_animeTitlesDumpXMLFilePath}'");
                statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = "Decompress titles database", Percentage = -1 });
                FileUtils.DecompressFile(_animeTitlesDumpRawFilePath, _animeTitlesDumpXMLFilePath);
            }
            else if (File.Exists(_animeTitlesDumpXMLFilePath))
                log.Debug($"Use already existing anime titles xml file '{_animeTitlesDumpXMLFilePath}'");

            // Read anime titles
            if (File.Exists(_animeTitlesDumpXMLFilePath))
            {
                log.Info($"Parse titles dump xml file '{_animeTitlesDumpXMLFilePath}'");
                statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = "Parse titles database", Percentage = -1 });
                Titles.ReadFromFile(_animeTitlesDumpXMLFilePath);
            }
            else
                log.Warn($"Not found anime titles xml file '{_animeTitlesDumpXMLFilePath}'!");

            // Print out anime title statistics
            log.Info($"Found {Titles.Items.Count} anime titles total");
            log.Info($"Found {Titles.AIDCount} animes total");
        }

        public void Startup(StatusChangedEventHandler statusChanged = null)
        {
            log.Info("Started service");
            statusChanged?.Invoke(this, new StatusChangedArgs() { Header = "Startup", Subject = "", Percentage = -1 });

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            log.Info($"Use identity: {identity.Name}");

            // Read config
            if (File.Exists(_configFilePath))
            {
                log.Info($"Loading config file '{_configFilePath}'");
                statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = "Loading config file", Percentage = -1 });
                Config.LoadFromFile(_configFilePath);
            }
            else
                log.Warn($"Not found config file '{_configFilePath}'!");

            // Download anime titles dump raw file from anidb if needed
            ReadTitles(statusChanged, false);
        }

        public void SaveConfig()
        {
            log.Info($"Saving config file '{_configFilePath}'");
            Config.SaveToFile(_configFilePath);
        }
    }
}
