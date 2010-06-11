using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Core.Builtins
{
    public class FtpFolder
    {
        public FtpFolder(string serverAddress, string username, string password) 
        {
            ServerAddress = serverAddress;
            Username = username;
            Password = password;
        }
        public string ServerAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Folder { get; set; }
        
    }
}
