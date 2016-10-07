using SocksGateway.Socks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocksGateway
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new SocksServer(8080);
            server.Username = "admin";
            server.Password = "admin";
            server.Start();

            Console.ReadKey();
        }
    }
}
