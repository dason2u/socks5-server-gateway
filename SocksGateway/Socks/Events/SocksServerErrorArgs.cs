using System;

namespace SocksGateway.Socks.Events
{
    public class SocksServerErrorArgs
    {
        public SocksServerErrorArgs(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            Exception = exception;
        }

        public Exception Exception { get; set; }
    }
}