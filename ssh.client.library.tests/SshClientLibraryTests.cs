using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ssh.client.library.tests
{
    [TestFixture]
    public class SshClientLibraryTests
    {
        private Proxy Client { get; set; }

        private string workingDirectory => Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory));

        [SetUp]
        public void Init() => Client = new Proxy();

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

            Assert.That(Client.Connect(), Is.True);
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
            SetClientDetails("localhost", "sshuser", "12345", @"Samples\client-key.bkp");

            Client.Connect();
            Assert.That(Client.Connected, Is.True);

            var fullPath = FindRealPath(filePath);
            var fileName = Path.GetFileName(fullPath);

            var fileStream = new FileStream(filePath, FileMode.Open);
            var result = Client.Send(fileName, fileStream);
        
            Assert.That(result, Is.True);
        }

        [Test]
        public void SSHClient_Send_NullDataStream_ShouldThrowArgumentException() 
            => Assert.Throws<ArgumentException>(() => Client.Send("small-file.txt", null));

        [Test]
        public void SSHClient_Send_AbsentFileName_ShouldThrowArgumentException()
            => Assert.Throws<ArgumentException>(() => Client.Send(string.Empty, new MemoryStream()));

        [Test]
        public void SSHClient_Send_AbsentWorkingDirectory_ShouldThrowArgumentException()
            => Assert.Throws<ArgumentException>(() => Client.Send("small-file.txt", new MemoryStream()));

        [Test]
        public void SSHClient_Send_EmptyFile_ShouldThrowArgumentException()
        {
            SetClientDetails("localhost", "sshuser", "12345", @"Samples\client-key.bkp");

            var testFilePath = "empty-file.txt";

            Client.Connect();
            Assert.That(Client.Connected, Is.True);

            var fullPath = FindRealPath(testFilePath);
            var fileName = Path.GetFileName(fullPath);

            var fileStream = new FileStream(testFilePath, FileMode.Open);
            var result = Client.Send(fileName, fileStream);

            Assert.That(result, Is.True);
        }

        #region Helpers

        private string FindRealPath(string path) => Path.Combine(workingDirectory, path);

        private void SetClientDetails(string host, string username, string password, string filePath)
        {
            var realPath = FindRealPath(filePath);

            Client.Host = host;
            Client.Username = username;
            Client.Password = password;
            Client.ClientKey = realPath;
        }

        #endregion Helpers
    }
}