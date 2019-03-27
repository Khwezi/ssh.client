using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
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

        [TestCase("localhost", "sshuser", "12345", "", "/")]
        [TestCase("localhost", "sshuser", "12345", @"Samples\client-key.bkp", "/")]
        public void SSHClient_KeyPasswordAuthentication(string host, string username, string password, string filePath, string workingDirectory)
        {
            SetClientDetails(host, username, password, filePath, workingDirectory);

            Assert.That(Client.Connect(), Is.True);
        }

        [TestCase("localhost", "", "12345", "", "/")]
        [TestCase("localhost", "sshuser", "", "", "/")]
        public void SSHClient_IncompleteCredentials_ShouldThrowArgumentException(string host, string username, string password, string filePath, string workingDirectory)
        {
            SetClientDetails(host, username, password, filePath, workingDirectory);
            Assert.Throws<ArgumentException>(() => Client.Connect());
        }

        [Test]
        public void SSHClient_InvalidCredentials_MustThrowSSHAuthenticationException()
        {
            SetClientDetails("localhost", "sshuser", "123456", @"Samples\client-key.bkp", "/");

            Assert.Multiple(() =>
            {
                Assert.Throws<Renci.SshNet.Common.SshAuthenticationException>(() => Client.Connect());
                Assert.That(Client.Connected, Is.False);
            });
        }

        [TestCase("small-file.txt")]
        [TestCase("large-file.txt")]
        public void SSHClient_PublishFile(string name)
        {
            SetClientDetails("localhost", "sshuser", "12345", @"Samples\client-key.bkp", @"\Desktop");

            Client.Connect();
            Assert.That(Client.Connected, Is.True);

            var bytesSent = Client.Send(name, ReadEmbeddedFile(name));

            Assert.That(bytesSent, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void SSHClient_Send_NullDataStream_ShouldThrowArgumentException()
            => Assert.Throws<ArgumentException>(() => Client.Send("small-file.txt", null));

        [Test]
        public void SSHClient_Send_AbsentFileName_ShouldThrowArgumentException()
            => Assert.Throws<ArgumentException>(() => Client.Send(string.Empty, new MemoryStream()));

        [Test]
        public void SSHClient_Send_AbsentWorkingDirectory_ShouldThrowArgumentException()
        {
            var name = "small-file.txt";
            var fileStream = ReadEmbeddedFile(name);

            Assert.Throws<ArgumentException>(() => Client.Send(name, fileStream));
        }

        [Test]
        public void SSHClient_Send_EmptyFile_ShouldThrowArgumentException()
        {
            SetClientDetails("localhost", "sshuser", "12345", @"Samples\client-key.bkp", "/");

            Client.Connect();

            Assert.Multiple(() =>
            {
                Assert.That(Client.Connected, Is.True);

                var fileStream = ReadEmbeddedFile("empty-file.txt");
                Assert.Throws<ArgumentException>(() => Client.Send("empty-file", fileStream));
                
                fileStream.Dispose();
            });
        }

        #region Helpers

        private string FindRealPath(string path) => Path.Combine(workingDirectory, path);

        private void SetClientDetails(string host, string username, string password, string filePath, string workingDirectory)
        {
            var realPath = FindRealPath(filePath);

            Client.Host = host;
            Client.Username = username;
            Client.Password = password;
            Client.ClientKey = realPath;
            Client.WorkingDirectory = workingDirectory;
        }

        private Stream ReadEmbeddedFile(string name)
        {
            var assembly = typeof(SshClientLibraryTests).GetTypeInfo().Assembly;
            var resourceKey = assembly.GetManifestResourceNames().First(r => r.Contains(name));

            return assembly.GetManifestResourceStream(resourceKey);
        }

        #endregion Helpers
    }
}