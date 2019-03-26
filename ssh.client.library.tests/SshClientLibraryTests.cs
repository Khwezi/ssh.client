using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ssh.client.library.tests
{
    [TestFixture]
    public class SshClientLibraryTests
    {
        private proxy Client { get; set; }

        private string CurrentPath
        {
            get => Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory));
        }

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

        [Test]
        public void SSHClient_InvalidCredentials_MustThrowSSHAuthenticationException()
        {
            SetClientDetails("localhost", "sshuser", "123456", @"Samples\client-key.bkp");

            Assert.Multiple(() =>
            {
                Assert.Throws<Renci.SshNet.Common.SshAuthenticationException>(() => Client.Connect());
                Assert.That(Client.Connected, Is.False);
            });
        }

        [TestCase(@"Samples\small-file.txt")]
        [TestCase(@"Samples\large-file.txt")]
        public void SSHClient_PublishFile(string filePath)
        {
            SetClientDetails("localhost", "sshuser", "12345", @"Samples\client - key.bkp");

            Client.Connect();
            Assert.That(Client.Connected, Is.True);

            var fullPath = FindRealPath(filePath);
            var fileStream = new FileStream(fullPath, FileMode.Open);
            var result = Client.Send(fullPath, fileStream);

            Assert.That(result, Is.True);
        }

        #region Helpers

        private string FindRealPath(string path) => Path.Combine(CurrentPath, path);

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