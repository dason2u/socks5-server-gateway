namespace SocksGateway.Models
{
    public class SocksRequestInfo
    {
        public string Address { get; set; }
        public int Port { get; set; }
        public byte[] OriginalRequest { get; set; }
    }
}
