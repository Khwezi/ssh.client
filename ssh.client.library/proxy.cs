using Renci.SshNet;
using System;
using System.IO;

namespace ssh.client.library
{
    public class Proxy : IDisposable
    {
        public string Host { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string ClientKey { get; set; }

        public string WorkingDirectory { get; set; }

        private SftpClient Client { get; set; }

        public bool Connected { get; private set; }

        public bool Connect()
        {
            if (string.IsNullOrEmpty(Username))
            {
                throw new ArgumentException("Username is required.");
            }

            if (string.IsNullOrEmpty(Password))
            {
                throw new ArgumentException("Password is required.");
            }

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

        public bool Send(string fileName, Stream dataStream)
        {
            if (dataStream == null)
            {
                throw new ArgumentException("Specify file to be sent.");
            }

            if (dataStream.Length == 0)
            {
                throw new ArgumentException("The file is empty.");
            }

            if (string.IsNullOrEmpty(WorkingDirectory))
            {
                throw new ArgumentException("Please specify the remote working directory.");
            }

            ulong bytesUploaded = 0;

            if (!string.IsNullOrEmpty(WorkingDirectory))
            {
                Client.ChangeDirectory(WorkingDirectory);
            }

            Client.UploadFile(dataStream, fileName, (length) => bytesUploaded = length);

            return bytesUploaded >= (ulong)dataStream.Length;
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