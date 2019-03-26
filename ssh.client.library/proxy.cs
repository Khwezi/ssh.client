﻿using System;
using Renci.SshNet;

namespace ssh.client.library
{
    public class proxy : IDisposable
    {
        public string Host { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string ClientKey { get; set; }

        private SftpClient Client { get; set; }

        public bool Connected { get; private set; }

        public bool Connect()
        {
            if(string.IsNullOrEmpty(Username))
                throw new ArgumentException("Username is required.");

            if (string.IsNullOrEmpty(Password))
                throw new ArgumentException("Password is required.");

            Client = string.IsNullOrEmpty(ClientKey) ? 
                new SftpClient(new ConnectionInfo(Host, Username, new PasswordAuthenticationMethod(Username, Password))) : 
                new SftpClient(new ConnectionInfo(Host, Username, new PasswordAuthenticationMethod(Username, Password), new PrivateKeyAuthenticationMethod(ClientKey)));

            Client.Connect();

            return Connected = Client.IsConnected;
        }

        public void Disconnect()
        {
            if (Connected)
            {
                Client.Dispose();
                Connected = false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Client?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}