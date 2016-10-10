﻿using System;
using System.Net.Sockets;

namespace SocksGateway.Socks.Events
{
    public class SocksErrorArgs : EventArgs
    {
        public SocksErrorArgs(TcpClient client, Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            Client = client;
            Exception = exception;
        }

        public TcpClient Client { get; }
        public Exception Exception { get; }
    }
}