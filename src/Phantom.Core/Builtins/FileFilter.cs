using System.Collections.Generic;
using System.Linq;

namespace Phantom.Core.Builtins
{
    using System;
    using System.IO;
    using FtpLib;

    public class FileFilter
    {
        readonly List<string> includes = new List<string>();
        readonly List<string> excludes = new List<string>();

        public FileFilter Exclude(string pattern)
        {
            excludes.Add(pattern);
            return this;
        }

        public FileFilter Include(string pattern)
        {
            includes.Add(pattern);
            return this;
        }

        public FileFilter IncludeEverything() 
        {
            includes.Add("**/*");
            return this;
        }

        public FileFilter IncludeEveryThingInDirectory(string path)
        {
            if (!path.EndsWith("/") || !path.EndsWith("\\"))
                path += "/";
            includes.Add(path + "**/*");
            return this;
        }

        public FileFilter CopyToDirectory(string sourceDirectory, string destinationDirectory) 
        {
            foreach (WrappedFileSystemInfo fileSystemInfo in GetFilesAndFolders(sourceDirectory)) 
            {
                if(fileSystemInfo is WrappedDirectoryInfo) {
                    var combinedPath = Path.Combine(destinationDirectory, fileSystemInfo.PathWithoutBaseDirectory);
                    if (!Directory.Exists(combinedPath))
                    {
                        Directory.CreateDirectory(combinedPath);
                    }
                }
                else 
                {
                    if (!Directory.Exists(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }
                
                    var combinedPath = Path.Combine(destinationDirectory, fileSystemInfo.PathWithoutBaseDirectory);
                    var newPath = Path.GetDirectoryName(combinedPath);
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }
                    File.Copy(fileSystemInfo.FullName, combinedPath, true);
                }
                
            }

            return this;
            
        }


        public FileFilter CopyToFtp(string sourceDirectory, FtpDirectory ftpDir) 
        {
            using (FtpConnection ftpConnection = new FtpConnection(ftpDir.Host, ftpDir.Port, ftpDir.Username, ftpDir.Password))
            {
                ftpConnection.Open();
                ftpConnection.Login();
                foreach (WrappedFileSystemInfo fileSystemInfo in GetFilesAndFolders(sourceDirectory)) {
                    if (fileSystemInfo is WrappedDirectoryInfo) 
                    {
                        var combinedPath = Path.Combine(ftpDir.BaseDirectory, fileSystemInfo.PathWithoutBaseDirectory);
                        if (!ftpConnection.DirectoryExists(combinedPath))
                            ftpConnection.CreateDirectory(combinedPath);
                        
                    }
                    else 
                    {
                        if (!ftpConnection.DirectoryExists(ftpDir.BaseDirectory)) {
                            ftpConnection.CreateDirectory(ftpDir.BaseDirectory);
                        
                        }
                            
                        var combinedPath = Path.Combine(ftpDir.BaseDirectory, fileSystemInfo.PathWithoutBaseDirectory);
                        var newPath = Path.GetDirectoryName(combinedPath);
                        if (!ftpConnection.DirectoryExists(newPath))
                        {
                            ftpConnection.CreateDirectory(newPath);
                        }

                        ftpConnection.SetCurrentDirectory(newPath);
                        ftpConnection.PutFile(fileSystemInfo.FullName);
                    }
                }
            }

            return this;
        }

        public static string PathWithoutBaseDirectory(string path, string baseDir)
        {
            return path.Substring(baseDir.Length).Trim('/').Trim('\\'); 
        }

        public FileFilter CopyFromFtp(FtpDirectory ftpDir, string destinationDirectory)
        {
            using (FtpConnection ftpConnection = new FtpConnection(ftpDir.Host, ftpDir.Port, ftpDir.Username, ftpDir.Password))
            {
                ftpConnection.Open();
                ftpConnection.Login();
                foreach (string ftpPath in GetFtpFilesAndFolders(ftpDir.BaseDirectory, ftpConnection))
                {
                    if (ftpConnection.DirectoryExists(ftpPath))
                    {
                        var combinedPath = Path.Combine(destinationDirectory, PathWithoutBaseDirectory(ftpPath, ftpDir.BaseDirectory));
                        if (!Directory.Exists(combinedPath))
                        {
                            Directory.CreateDirectory(combinedPath);
                        }
                    }
                    else
                    {
                        if (!Directory.Exists(destinationDirectory))
                        {
                            Directory.CreateDirectory(destinationDirectory);
                        }

                        var combinedPath = Path.Combine(destinationDirectory, PathWithoutBaseDirectory(ftpPath, ftpDir.BaseDirectory));
                        var newPath = Path.GetDirectoryName(combinedPath);
                        if (!Directory.Exists(newPath))
                        {
                            Directory.CreateDirectory(newPath);
                        }
                        ftpConnection.GetFile(ftpPath, combinedPath, false);
                        
                    }
                }
                ftpConnection.Close();
            }

            return this;
        }

        public void DeleteFromFtp(FtpDirectory ftpDir)
        {

            using (FtpConnection ftpConnection = new FtpConnection(ftpDir.Host, ftpDir.Port, ftpDir.Username, ftpDir.Password)) 
            {
                ftpConnection.Open();
                ftpConnection.Login();

                
                foreach (var path in GetFtpFilesAndFolders(ftpDir.BaseDirectory,ftpConnection)) 
                {
                    Console.WriteLine(path);
                    if (ftpConnection.DirectoryExists(path))
                        RemoveDirectoryEvenIfNotEmpty(ftpConnection, path);
                    else {
                        ftpConnection.RemoveFile(path);
                    }
                }

                ftpConnection.Close();
            }

        }

        private static void RemoveDirectoryEvenIfNotEmpty(FtpConnection ftpConnection, string startDirectory)
        {
            List<string> directories = new List<string>();
            directories.Add(startDirectory);
            int index = 0;
            while (index < directories.Count)
            {
                string currentDirectory = directories[index];
                directories.AddRange(ftpConnection.GetDirectories(currentDirectory).Select(dir => currentDirectory + dir.Name + "/"));
                index++;
            }


            directories.Reverse();
            List<string> fileNames = new List<string>();
            foreach (var directory in directories)
            {
                foreach (var ftpFile in ftpConnection.GetFiles(directory))
                {
                    string fullPath = directory + ftpFile.Name;
                    if (ftpConnection.FileExists(fullPath))
                        ftpConnection.RemoveFile(fullPath);
                }

                foreach (var ftpDir in ftpConnection.GetDirectories(directory))
                {
                    string fullPath = directory + ftpDir.Name;
                    if (ftpConnection.DirectoryExists(fullPath))
                        ftpConnection.RemoveDirectory(fullPath);
                }
            }
        }

        public void Delete(string sourceDirectory)
        {
            foreach (WrappedFileSystemInfo fileSystemInfo in GetFilesAndFolders(sourceDirectory))
            {
                fileSystemInfo.Delete();
            }

        }

        static string FixupPath(string baseDir, string path)
        {
            //Glob likes forward slashes
            return Path.Combine(baseDir, path).Replace('\\', '/');
        }

        public IEnumerable<WrappedFileSystemInfo> GetFilesAndFolders(string baseDir)
        {
            var includedFiles = from include in includes
                                from file in Glob.GlobResults(FixupPath(baseDir, include))
                                select file;

            var excludesFiles = from exclude in excludes
                                from file in Glob.GlobResults(FixupPath(baseDir, exclude))
                                select file;

            foreach (var path in includedFiles.Except(excludesFiles))
            {
                if (Directory.Exists(path))
                {
                    yield return new WrappedDirectoryInfo(baseDir, path, false);
                }
                else
                {
                    yield return new WrappedFileInfo(baseDir, path, false);
                }
            }
        }

        public IEnumerable<string > GetFtpFilesAndFolders(string baseDir, FtpConnection ftpConnection) 
        {
            FtpFileAdaptionLayer ftpFileAdaptionLayer = new FtpFileAdaptionLayer(ftpConnection);
            
            var includedFiles = from include in includes
                                from file in Glob.GlobResults(ftpFileAdaptionLayer, FixupPath(baseDir, include), 0)
                                select file;

            var excludesFiles = from exclude in excludes
                                from file in Glob.GlobResults(ftpFileAdaptionLayer, FixupPath(baseDir, exclude), 0)
                                select file;
            
            return includedFiles.Except(excludesFiles);
        }
    }
}
