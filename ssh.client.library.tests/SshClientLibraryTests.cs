using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ssh.client.library.tests
{
    [TestFixture]
    public class SshClientLibraryTests
    {
        private proxy Client { get; set; }

        [SetUp]
        public void Init() => Client = new proxy();

        [TearDown]
        public void Dispose()
        {
            Task.Delay(1000);
            Client.Disconnect();
            Client.Dispose();
            Client = null;
        }

        [TestCase("localhost", "sshuser", "12345", "")]
        [TestCase("localhost", "sshuser", "12345", @"Samples\client-key.bkp")]
        public void SSHClient_KeyPasswordAuthentication(string host, string username, string password, string filePath)
        {
            SetClientDetails(host, username, password, filePath);

            var isConnected = Client.Connect();
            Assert.That(isConnected, Is.True);
        }

        [TestCase("localhost", "", "12345", "")]
        [TestCase("localhost", "sshuser", "", "")]
        public void SSHClient_IncompleteCredentials_ShouldThrowArgumentException(string host, string username, string password, string filePath)
        {
            SetClientDetails(host, username, password, filePath);
            Assert.Throws<ArgumentException>(() => Client.Connect());
        }

        #region Helpers
        private string FindRealPath(string path)
        {
            var realPath = path;
            if (!string.IsNullOrEmpty(path)) realPath = Path.GetFullPath(path);

            return realPath;
        }

        private void SetClientDetails(string host, string username, string password, string filePath)
        {
            var realPath = FindRealPath(filePath);

            Client.Host = host;
            Client.Username = username;
            Client.Password = password;
            Client.ClientKey = realPath;
        }
        #endregion
    }
}