using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Core.Builtins
{
    using System.IO;

    [Serializable, ExcludeFromCoverage]
    internal class PlatformAdaptationLayer
    {
        // Methods
        public virtual bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public virtual string[] GetDirectories(string path, string searchPattern)
        {
            return Directory.GetDirectories(path, searchPattern);
        }

        public virtual string GetEnvironmentVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }       

        public virtual string GetFileName(string file)
        {
            return Path.GetFileName(file);
        }

        public virtual string[] GetFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern);
        }

        public virtual string GetFullPath(string path)
        {
            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(path);
            }
            catch (Exception)
            {
                throw new Exception("Invalid path");
            }
            return fullPath;
        }

        public virtual bool IsAbsolutePath(string path)
        {
            return (((Environment.OSVersion.Platform != PlatformID.Unix) && Path.GetPathRoot(path).EndsWith(@":\")) ||
					((Environment.OSVersion.Platform == PlatformID.Unix) && Path.IsPathRooted(path)));
        }

       
        public virtual Stream OpenInputFileStream(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public virtual Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(path, mode, access, share);
        }

        public virtual Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share,
                                                  int bufferSize)
        {
            return new FileStream(path, mode, access, share, bufferSize);
        }

        public virtual Stream OpenOutputFileStream(string path)
        {
            return new FileStream(path, FileMode.Create, FileAccess.Write);
        }

        public virtual void SetEnvironmentVariable(string key, string value)
        {
            Environment.SetEnvironmentVariable(key, value);
        }

        public virtual void TerminateScriptExecution(int exitCode)
        {
            Environment.Exit(exitCode);
        }

        // Properties
        public virtual string CurrentDirectory
        {
            get { return Environment.CurrentDirectory; }
        }

        public virtual StringComparer PathComparer
        {
            get { return StringComparer.Ordinal; }
        }
    }
}
