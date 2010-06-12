using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Core.Builtins
{
    
    public class FtpDirectory
    {
        public FtpDirectory(string host) 
        {
            Host = host;
        }

        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        string _baseDirectory = "./";
        public string BaseDirectory 
        {
            get { return _baseDirectory; }
            set { _baseDirectory = value; }
        }
        
        int _port = 21;
        public int Port 
        {
            get { return _port; }
            set { _port = value; }
            
        }
        
    }
}
