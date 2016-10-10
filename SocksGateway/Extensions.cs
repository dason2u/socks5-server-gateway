using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace SocksGateway
{
    public static class Extensions
    {
        public static void WriteAllData(this NetworkStream networkStream, byte[] data)
        {
            networkStream.Write(data, 0, data.Length);
        }

        public static byte[] ReadDataChunk(this Stream clientStream, int bufferSize = 2048)
        {
            var buffer = new byte[bufferSize];
            var received = clientStream.Read(buffer, 0, bufferSize);
            return buffer.Take(received).ToArray();
        }
    }
}
