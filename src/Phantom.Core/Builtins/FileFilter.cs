using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Core.Builtins
{
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


        public FileFilter CopyToFtp(string sourceDirectory, FtpFolder ftpFolder) 
        {
            using (FtpConnection ftp = new FtpConnection("ftpserver", "username", "password")) {
                foreach (WrappedFileSystemInfo fileSystemInfo in GetFilesAndFolders(sourceDirectory)) {
                    if (fileSystemInfo is WrappedDirectoryInfo) {
                        var combinedPath = Path.Combine(ftpFolder.Folder, fileSystemInfo.PathWithoutBaseDirectory);
                        if (!ftp.DirectoryExists(combinedPath)) {
                            ftp.CreateDirectory(combinedPath);
                        }
                    }
                    else {
                        if (!ftp.DirectoryExists(ftpFolder.Folder))
                        {
                            ftp.CreateDirectory(ftpFolder.Folder);
                        }

                        var combinedPath = Path.Combine(ftpFolder.Folder, fileSystemInfo.PathWithoutBaseDirectory);
                        var newPath = Path.GetDirectoryName(combinedPath);
                        if (!ftp.DirectoryExists(newPath)) {
                            ftp.CreateDirectory(newPath);
                            ftp.SetCurrentDirectory("newPath");
                        }
                        ftp.PutFile(fileSystemInfo.FullName);
                    }

                }
            }

            return this;
        }

        public void Delete(FtpFolder ftpFolder)
        {
            using (FtpConnection ftpConnection = new FtpConnection(ftpFolder.ServerAddress, ftpFolder.Username, ftpFolder.Password)) 
            {
                ftpConnection.Open();
                foreach (WrappedFileSystemInfo fileSystemInfo in GetFtpFilesAndFolders(ftpFolder.Folder,ftpConnection)) {
                    fileSystemInfo.Delete();
                }
                ftpConnection.Close();
                ftpConnection.Dispose();
            }

        }

        public void DeleteFromFtp(string sourceDirectory)
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

        public IEnumerable<WrappedFileSystemInfo> GetFtpFilesAndFolders(string baseFolder, FtpConnection ftpConnection) 
        {
            FtpFileAdaptionLayer ftpFileAdaptionLayer = new FtpFileAdaptionLayer(ftpConnection);

            var includedFiles = from include in includes
                                from file in Glob.GlobResults(ftpFileAdaptionLayer, FixupPath(baseFolder, include), 0)
                                select file;

            var excludesFiles = from exclude in excludes
                                from file in Glob.GlobResults(ftpFileAdaptionLayer, FixupPath(baseFolder, exclude), 0)
                                select file;

            foreach (var path in includedFiles.Except(excludesFiles))
            {
                if (Directory.Exists(path))
                {
                    yield return new WrappedDirectoryInfo(baseFolder, path, false);
                }
                else
                {
                    yield return new WrappedFileInfo(baseFolder, path, false);
                }
            }
        }
    }
}
