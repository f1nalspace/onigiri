﻿using Finalspace.Onigiri.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.Persistence
{
    static class DatabaseFileFourCCs
    {
        private static readonly FourCC TitleType = FourCC.FromString("TITL");
        private static readonly FourCC DetailsType = FourCC.FromString("DTLS");
        private static readonly FourCC PictureType = FourCC.FromString("PICT");

        private static readonly FourCC XMLFormatType = FourCC.FromString("XML_");
        private static readonly FourCC BinaryFormatType = FourCC.FromString("BINY");
    }
}
