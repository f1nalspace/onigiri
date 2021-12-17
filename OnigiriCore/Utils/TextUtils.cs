using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.Utils
{
    static class TextUtils
    {
        public static string Base64EncodeData(byte[] data)
        {
            return System.Convert.ToBase64String(data);
        }

        public static string Base64EncodeString(string plainText, Encoding encoding)
        {
            byte[] plainTextBytes = encoding.GetBytes(plainText);
            return Base64EncodeData(plainTextBytes);
        }

        public static byte[] Base64DecodeBytes(string base64EncodedData)
        {
            byte[] base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return base64EncodedBytes;
        }

        public static string Base64DecodeString(string base64EncodedData, Encoding encoding)
        {
            byte[] base64EncodedBytes = Base64DecodeBytes(base64EncodedData);
            return encoding.GetString(base64EncodedBytes);
        }
    }
}
