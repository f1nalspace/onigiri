using Finalspace.Onigiri.Enums;
using Finalspace.Onigiri.Events;
using Finalspace.Onigiri.Media;
using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.Storage;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Finalspace.Onigiri
{
    class Program
    {
        const string ShowTitlesCommand = "titles";
        const string ShowTitlesByAidCommand = "titles_by_aid";
        const string ShowAnimesCommand = "animes";

        static int GetArgumentIndex(string[] args, string name)
        {
            for (int i = 0; i < args.Length; ++i)
                if (args[i].Equals("-" + name))
                    return i;
            return -1;
        }

        static string ToUTF8(string str)
        {
            byte[] src = Encoding.Default.GetBytes(str);
            byte[] dst = Encoding.Convert(Encoding.Default, Encoding.UTF8, src);
            return Encoding.UTF8.GetString(dst);
        }

        static void ShowTitles(OnigiriService system)
        {
            foreach (Title title in system.Titles.Items)
                Console.WriteLine(title.ToString());
        }

        static void ShowTitlesByAid(OnigiriService system, ulong aid)
        {
            IEnumerable<Title> titles = system.Titles.GetTitlesByAID(aid);
            if (titles != null)
            {
                foreach (Title title in titles)
                {
                    string t = title.ToString();
                    Console.WriteLine(ToUTF8(t));
                }
            }
            else Console.WriteLine($"No titles by the given aid {aid} found!");
        }

        static void ShowAnimes(OnigiriService system)
        {
            var animes = system.Animes.Items;
            foreach (Anime anime in animes)
            {
                string t = anime.ToString();
                Console.WriteLine(ToUTF8(t));
            }
        }

        static void ProcessArguments(string[] args, OnigiriService system, IAnimeStorage storage)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            // Set search path
            int addSearchArgIndex = GetArgumentIndex(args, "setsearch");
            if (addSearchArgIndex > -1)
            {
                if (addSearchArgIndex + 1 < args.Length)
                {
                    string searchPath = args[addSearchArgIndex + 1];
                    system.Config.SearchPaths.Clear();
                    system.Config.SearchPaths.Add(new SearchPath() { Path = searchPath });
                }
                else
                    Console.WriteLine("Missing search path after argument!");
            }

            // Starup onigiri system
            system.Startup(new StatusChangedEventHandler((s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Header))
                    Console.WriteLine(e.Header);
            }));

            // Update
            int updateArgIndex = GetArgumentIndex(args, "update");
            if (updateArgIndex > -1)
            {
                system.ClearIssues();

                system.UpdateSources(UpdateFlags.DownloadDetails | UpdateFlags.DownloadPicture);
            }

            // Show
            int showArgIndex = GetArgumentIndex(args, "show");
            if (showArgIndex > -1)
            {
                if (showArgIndex + 1 < args.Length)
                {
                    string showCommand = args[showArgIndex + 1];
                    switch (showCommand)
                    {
                        case ShowTitlesCommand:
                            ShowTitles(system);
                            break;
                        case ShowTitlesByAidCommand:
                            if (showArgIndex + 2 < args.Length)
                            {
                                ulong aid = ulong.Parse(args[showArgIndex + 2]);
                                ShowTitlesByAid(system, aid);
                            }
                            else Console.WriteLine("Missing aid argument!");
                            break;
                        case ShowAnimesCommand:
                            ShowAnimes(system);
                            break;
                    }
                }
                else Console.WriteLine("Missing show command after argument!");
            }

            // Save
            int saveArgIndex = GetArgumentIndex(args, "save");
            if (addSearchArgIndex > -1)
                system.SaveConfig();
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine(string.Join(" ", args));

#if true

            string usersPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            string folderPath = Path.Combine(usersPath, "OneDrive", "Q3");

            DirectoryInfo folder = new DirectoryInfo(folderPath);
            if (!folder.Exists)
                throw new DirectoryNotFoundException($"Folder path '{folderPath}' does not exists");

            FileInfo[] files = folder
                .GetFiles("*.*", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f.Name)
                .ToArray();

            foreach (var file in files)
            {
                Console.WriteLine($"Parse media file: {file.Name}");
                MediaInfo info = MediaInfoParser.Parse(file);
                if (info != null)
                {
                    Console.WriteLine(FormattableString.Invariant($"\tContainer: '{info.Format}', Duration: {info.Duration.TotalSeconds} secs"));

                    foreach (VideoInfo video in info.Video)
                        Console.WriteLine(FormattableString.Invariant($"\tVideo: {video.Width}x{video.Height}, {video.FrameCount} frames, {video.FrameRate} fps [Codec:'{video.Codec}', Name: '{video.Name}']"));

                    foreach (AudioInfo audio in info.Audio)
                        Console.WriteLine(FormattableString.Invariant($"\tAudio: {audio.Channels} channels, {audio.SampleRate} Hz, {audio.BitsPerSample} bits/sample, {audio.BitRate / 1000} kHz [Codec: '{audio.Codec}', Name: '{audio.Name}']"));
                }
                else
                    Console.Error.WriteLine($"\tFailed getting media infos");
            }

            Console.WriteLine("press any key to exit");
            Console.ReadKey();

#endif

#if false


            XmlConfigurator.Configure();
            if (args.Length == 0)
            {
                string exeName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
                Console.WriteLine("Usage:\n");
                Console.WriteLine($"{exeName} -[argument-1] -[argument-2]...\n");
                Console.WriteLine($"{exeName} -[argument-1] [param for argument-1] -[argument-1] \"[param for argument-2]\"...\n");

                Console.WriteLine("Arguments (case-sensitive):\n");
                Console.WriteLine("-setsearch \"Path to search directory\"");
                Console.WriteLine("-show [show command]");
                Console.WriteLine($"\t{ShowTitlesCommand} (lists all found anime titles)");
                Console.WriteLine($"\t{ShowTitlesByAidCommand} [aid] (lists all titles for the given anime id)");
                Console.WriteLine($"\t{ShowAnimesCommand} (lists all found animes)");
                Console.WriteLine("-update (Updates anime infos if needed)");
                Console.ReadKey();
                return;
            }

            OnigiriService system = new OnigiriService();

            string persistentPath = OnigiriPaths.PersistentPath;

            FolderAnimeFilesStorage persistenceStorage = new FolderAnimeFilesStorage(persistentPath, system.Config.MaxThreadCount);

            ProcessArguments(args, system, persistenceStorage);

            Console.ReadKey();           
#endif

        }
    }
}
