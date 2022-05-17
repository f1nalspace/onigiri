using System;
using System.IO;

namespace Finalspace.Onigiri
{
    public static class OnigiriPaths
    {
        public const string AnimeXMLDetailsFilename = ".anime.xml";
        public const string AnimeXMLAddonFilename = ".adata.xml";
        public const string AnimeAIDFilename = "aid.txt";

        private static readonly string _appSettingsPath;
        private static readonly string _persistentPath;
        private static readonly string _userImagesPath;
        private static readonly string _configFilePath;

        public static string AppSettingsPath => _appSettingsPath;
        public static string PersistentPath => _persistentPath;
        public static string UserImagesPath => _userImagesPath;
        public static string ConfigFilePath => _configFilePath;

        public static string AnimeTitlesDumpRawFilePath => Path.Combine(AppSettingsPath, "animetitles.xml.gz");
        public static string AnimeTitlesDumpXMLFilePath => Path.Combine(AppSettingsPath, "animetitles.xml");

        static OnigiriPaths()
        {
            _appSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Onigiri");
            _persistentPath = Path.Combine(_appSettingsPath, "PersistentCache");
            _userImagesPath = Path.Combine(_appSettingsPath, "UserImages");
            _configFilePath = Path.Combine(_appSettingsPath, "config.xml");
        }
    }
}
