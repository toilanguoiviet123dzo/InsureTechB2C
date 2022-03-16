using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cores.Utilities
{
    public static class MyFile
    {
        #region Load file
        public static string Load_ToString(string fileName)
        {
            string readContents;
            //
            try
            {
                using (StreamReader streamReader = new StreamReader(fileName, GetEncoding(fileName)))
                {
                    readContents = streamReader.ReadToEnd();
                }
            }
            catch
            {
                return "";
            }
            //Return
            return readContents;
        }

        public static byte[] Load_ToByteArray(string fileName)
        {
            byte[] buff = null;
            try
            {
                //FileStream fs = new FileStream(fileName,
                //                           FileMode.Open,
                //                           FileAccess.Read);
                //BinaryReader br = new BinaryReader(fs);
                //long numBytes = new FileInfo(fileName).Length;
                //buff = br.ReadBytes((int)numBytes);
                buff = File.ReadAllBytes(fileName);
            }
            catch
            {
            }
            //
            return buff;
        }

        public static Encoding GetEncoding(string filename)
        {
            //Check exist
            if (!File.Exists(filename))
            {
                return Encoding.ASCII;
            }

            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.ASCII;
        }
        #endregion

        #region Save file
        public static void Write_ToBinary(string filename, byte[] buffer)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(buffer);
                    writer.Close();
                }
            }
        }

        public static void Write_ToString_ASCII(string filename, string fileStringContent, bool append = false)
        {
            File.WriteAllText(filename, fileStringContent, Encoding.ASCII);
        }

        public static void Write_ToString_Unicode(string filename, string fileStringContent, bool append = false)
        {
            File.WriteAllText(filename, fileStringContent, Encoding.Unicode);
        }

        public static void Write_ToString_UTF8(string filename, string fileStringContent, bool append = false)
        {
            File.WriteAllText(filename, fileStringContent, Encoding.UTF8);
        }

        public static void Write_Log(string fullFilename, string fileStringContent)
        {
            File.WriteAllText(fullFilename, fileStringContent, Encoding.UTF8);
        }

        #endregion

        #region Remove file
        public static void Delete(string filename)
        {
            File.Delete(filename);
        }
        #endregion

        #region View file
        public static void View_File(string filePath)
        {
            System.Diagnostics.Process.Start(filePath);
        }

        #endregion

        #region FileFunctions
        public static string Get_ExecutingFolder()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string Get_ShortFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        public static string Get_FileExtention(string filePath)
        {
            return Path.GetExtension(filePath).Replace(".","");
        }
        #endregion
    }
}
