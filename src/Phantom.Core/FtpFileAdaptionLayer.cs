﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Core
{
    using System.IO;
    using FtpLib;

    [Serializable, ExcludeFromCoverage]
    internal class FtpFileAdaptionLayer : IFileAdaptionLayer
    {
        private FtpConnection _ftpConnection;
        public FtpFileAdaptionLayer(FtpConnection ftpConnection) {
            _ftpConnection = ftpConnection;
        }
        // Methods
        public virtual bool DirectoryExists(string path)
        {
            return _ftpConnection.DirectoryExists(path);
        }

        public virtual bool FileExists(string path)
        {
            return _ftpConnection.FileExists(path);
        }

        public virtual string[] GetDirectories(string path, string searchPattern)
        {
            return _ftpConnection.GetDirectories(path).Select(info => info.FullName).ToArray();
        }

        public virtual string GetFileName(string file)
        {
            return Path.GetFileName(file);
        }

        public string[] GetFiles(string path, string searchPattern)
        {
            return _ftpConnection.GetFiles(path).Select(file => file.FullName).ToArray();
        }
       
    }
}
