using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace ACT.Hojoring.ATDExtractor
{
    /// <summary>
    /// Get original files which combinded by Fody/Costura.
    /// 
    /// Reference:
    /// https://github.com/Fody/Costura/blob/master/Costura.Template/Common.cs
    /// </summary>
    public static class CosturaDecompress
    {

        public static int SaveResouceFromAssembly(string assembly, string resoucename, string filename)
        {
            var result = 0;
            var stream = LoadStreamFromAssembly(assembly, resoucename);
            if (stream != null)
            {
                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    var array = new byte[81920];
                    int count;
                    while ((count = stream.Read(array, 0, array.Length)) != 0)
                    {
                        fs.Write(array, 0, count);
                        result += count;
                    }
                }
            }
            return result;
        }

        static Stream LoadStreamFromAssembly(string assembly, string fullname)
        {
            var asm = Assembly.LoadFile(assembly);

            if (fullname.EndsWith(".compressed"))
            {

                using (var stream = asm.GetManifestResourceStream(fullname))
                using (var compressStream = new DeflateStream(stream, CompressionMode.Decompress))
                {
                    var memStream = new MemoryStream();
                    CopyTo(compressStream, memStream);
                    memStream.Position = 0;
                    return memStream;
                }
            }

            return asm.GetManifestResourceStream(fullname);
        }

        static void CopyTo(Stream source, Stream destination)
        {
            var array = new byte[81920];
            int count;
            while ((count = source.Read(array, 0, array.Length)) != 0)
            {
                destination.Write(array, 0, count);
            }
        }

        public static void DecompressFile(string compressedFileName, string outFileName) 
        {
            using (FileStream compressedFileStream = new FileStream(compressedFileName, FileMode.Open))
            using (DeflateStream deflateStream = new DeflateStream(compressedFileStream, CompressionMode.Decompress))
            using (FileStream outFileStream = new FileStream(outFileName, FileMode.Create))
            {
                Byte[] buffer = new Byte[81920];
                while (true)
                {
                    Int32 readBytes = deflateStream.Read(buffer, 0, buffer.Length);
                    if (readBytes == 0)
                    {
                        break;
                    }

                    outFileStream.Write(buffer, 0, readBytes);
                }
            }
        }
    }
}
