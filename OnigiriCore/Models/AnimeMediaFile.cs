using DevExpress.Mvvm;
using Finalspace.Onigiri.Types;
using System;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    public class AnimeMediaFile : BindableBase
    {
        public string FileName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public ulong FileSize
        {
            get => GetValue<ulong>();
            set => SetValue(value);
        }

        public MediaInfo Info
        {
            get => GetValue<MediaInfo>();
            set => SetValue(value);
        }
    }
}
