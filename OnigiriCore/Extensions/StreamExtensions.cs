using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Finalspace.Onigiri.Extensions
{
    static class StreamExtensions
    {
        public static void WriteStruct<T>(this Stream stream, T data)
        {
            int structLength = Marshal.SizeOf(typeof(T));
            IntPtr ptr = Marshal.AllocHGlobal(structLength);
            Marshal.StructureToPtr(data, ptr, false);
            byte[] bytes = new byte[structLength];
            Marshal.Copy(ptr, bytes, 0, bytes.Length);
            Marshal.FreeHGlobal(ptr);
            stream.Write(bytes, 0, structLength);
        }
        public static T ReadStruct<T>(this Stream stream)
        {
            int structLength = Marshal.SizeOf(typeof(T));
            byte[] structData = new byte[structLength];
            stream.Read(structData, 0, structLength);
            GCHandle ptr = GCHandle.Alloc(structData, GCHandleType.Pinned);
            T result = (T)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(T));
            ptr.Free();
            return result;
        }

        public static void WriteText(this Stream stream, string text, Encoding encoding)
        {
            byte[] data = encoding.GetBytes(text);
            uint dataLength = (uint)data.Length;
            byte[] dataLengthBytes = BitConverter.GetBytes(dataLength);
            Debug.Assert(dataLengthBytes.Length == 4);
            stream.Write(dataLengthBytes, 0, dataLengthBytes.Length);
            stream.Write(data, 0, (int)dataLength);
        }

        public static string ReadText(this Stream stream, Encoding encoding)
        {
            byte[] dataLengthBytes = new byte[4];
            stream.Read(dataLengthBytes, 0, 4);
            uint dataLength = BitConverter.ToUInt32(dataLengthBytes, 0);
            byte[] data = new byte[dataLength];
            stream.Read(data, 0, (int)dataLength);
            string result = encoding.GetString(data);
            return (result);
        }
    }
}
