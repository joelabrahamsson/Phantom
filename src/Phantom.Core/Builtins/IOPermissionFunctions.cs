using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.AccessControl;
using System.Runtime.CompilerServices;

namespace Phantom.Core.Builtins
{
    [CompilerGlobalScope]
    public static class IOPermissionFunctions
    {
        public static void SetFilePermission(string path, string accountName, PermissionLevel permissionLevel)
        {
            AddFileSecurity(path, accountName, GetRightsFromPermissionLevel(permissionLevel));
        }

        public static void SetDirectoryPermission(string path, string accountName, PermissionLevel permissionLevel)
        {
            AddDirectorySecurity(path, accountName, GetRightsFromPermissionLevel(permissionLevel));
        }

        private static void AddFileSecurity(string path, string account, FileSystemRights rights)
        {
            FileInfo fileInfo = new FileInfo(path.Replace('\\', '/'));
            FileSecurity fileSecurity = fileInfo.GetAccessControl();
            fileSecurity.AddAccessRule(new FileSystemAccessRule(account,
                                                            rights,
                                                            AccessControlType.Allow
                                                            ));

            fileInfo.SetAccessControl(fileSecurity);
        }

        private static void AddDirectorySecurity(string path, string account, FileSystemRights rights)
        {
            DirectoryInfo direcotryInfo = new DirectoryInfo(path.Replace('\\', '/'));
            DirectorySecurity directorySecurity = direcotryInfo.GetAccessControl();
            directorySecurity.AddAccessRule(new FileSystemAccessRule(account,
                                                            rights, InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly,
                                                            AccessControlType.Allow));
            directorySecurity.AddAccessRule(new FileSystemAccessRule(account,
                                                            rights, InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly,
                                                            AccessControlType.Allow));
            directorySecurity.AddAccessRule(new FileSystemAccessRule(account,
                                                            rights, AccessControlType.Allow));
            direcotryInfo.SetAccessControl(directorySecurity);

        }

        private static FileSystemRights GetRightsFromPermissionLevel(PermissionLevel level)
        {
            if (level == PermissionLevel.Full)
                return FileSystemRights.FullControl;

            if (level == PermissionLevel.Modify)
                return FileSystemRights.Modify;

            if (level == PermissionLevel.Write)
                return FileSystemRights.Write;

            if (level == PermissionLevel.ReadAndExecute)
                return FileSystemRights.ReadAndExecute;

            return FileSystemRights.Read;
        }

    }

    public enum PermissionLevel
    {
        Read,
        ReadAndExecute,
        Write,
        Modify,
        Full
    }
}
