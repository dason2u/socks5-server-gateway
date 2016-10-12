using System;
using System.Configuration;

namespace SocksGateway
{
    public static class Configuration
    {
        public static readonly string ServerUsername;
        public static readonly string ServerPassword;
        public static readonly int ServerPort;
        public static readonly bool ServerIsSecured;


        static Configuration()
        {
            ServerUsername = ConfigurationManager.AppSettings["ServerUsername"];
            ServerPassword = ConfigurationManager.AppSettings["ServerPassword"];
            ServerPort = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);
            ServerIsSecured = bool.Parse(ConfigurationManager.AppSettings["ServerIsSecured"]);

            if (string.IsNullOrEmpty(ServerUsername))
                throw new Exception("Username is empty in configuration file.");
            if (string.IsNullOrEmpty(ServerPassword))
                throw new Exception("Password is empty in configuration file.");

            if ((ServerPort < 0) || (ServerPort > 65535))
                throw new Exception("Unknown port in configuration file.");
        }
    }
}