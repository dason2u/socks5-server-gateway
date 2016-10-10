using System;
using System.Configuration;

namespace SocksGateway
{
    public static class Configuration
    {
        public static readonly string Username;
        public static readonly string Password;

        static Configuration()
        {
            if (string.IsNullOrEmpty(Username))
                throw new Exception("Username is empty in configuration file.");
            if (string.IsNullOrEmpty(Password))
                throw new Exception("Password is empty in configuration file.");


            Username = ConfigurationManager.AppSettings["username"];
            Password = ConfigurationManager.AppSettings["password"];
        }
    }
}