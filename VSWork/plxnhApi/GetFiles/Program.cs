using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace GetFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            string srcPath = ConfigurationManager.AppSettings["srcPath"];
            string destPath = ConfigurationManager.AppSettings["destPath"];

            List<FileInformation> list = DirectoryAllFiles.GetAllFiles(new System.IO.DirectoryInfo(srcPath));
            //if (list.Where(t => t.FileName.ToLower().Contains("android")).Any()) Console.WriteLine("true");
            int i = 0;
            foreach (var item in list)
            {
                //Console.WriteLine(string.Format("文件名：{0}---文件目录{1}", item.FileName, item.FilePath));
                i++;
                string path = Path.GetDirectoryName(item.FilePath) + "\\";
                string fileName = Path.GetFileName(item.FilePath);
                string fullPath = destPath + i.ToString() + fileName;
                if (fileName.Contains(".xltd") == false)
                {
                    File.Copy(item.FilePath,fullPath);
                }
                
            } 
        }
    }

    public class DirectoryAllFiles
    {
        static List<FileInformation> FileList = new List<FileInformation>();
        public static List<FileInformation> GetAllFiles(DirectoryInfo dir)
        {
            FileInfo[] allFile = dir.GetFiles();
            foreach (FileInfo fi in allFile)
            {
                FileList.Add(new FileInformation { FileName = fi.Name, FilePath = fi.FullName });
            }
            DirectoryInfo[] allDir = dir.GetDirectories();
            foreach (DirectoryInfo d in allDir)
            {
                GetAllFiles(d);
            }
            return FileList;
        }
    }

    public class FileInformation
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
    } 
}
