using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Palladium.Engine {
    using engine = com.akoimeexx.network.palladium.engine;
    using protocol = com.akoimeexx.network.palladium.protocol;
    [TestClass]
    public class ClientTests {
        private const int PORT = 55555;

        [TestMethod]
        public void SendReceiveUserMessage() {
            string input = "Hello world";
            using (engine.Client c = new engine.Client(PORT)) {
                int i = 0;
                // Initial wait just allows the login message to hit the client
                // and allow the client to record a new user in the user list.
                while (
                    i < 100
                ) {
                    System.Threading.Thread.Sleep(10);
                    i++;
                }
                // Send a test message to the first user in the list. In this 
                // test, the first user also happens to be the current user as 
                // we are running multiple clients.
                c.Message(
                    c.Users[0],
                    new protocol.DataUri() {
                        Data = input
                    }
                );
                i = 0;
                // Wait for 1 second or message received, whichever comes first
                while (
                    c.Messages.Count < 1 &&
                    i < 100
                ) {
                    System.Threading.Thread.Sleep(10);
                    i++;
                }

                Assert.AreEqual(
                    1, 
                    c.Messages.Count, 
                    "Timeout exceeded or no message received"
                );
                Assert.AreEqual(
                    input,
                    c.Messages[0].Contents.Data.ToString()
                );
            }
        }
        [TestMethod]
        public void SendReceiveFileMessage() {
            string inputPath = "../../HelloWorld.txt";
            using (engine.Client c = new engine.Client(PORT)) {
                int i = 0;
                // Initial wait just allows the login message to hit the client
                // and allow the client to record a new user in the user list.
                while (
                    i < 100
                ) {
                    System.Threading.Thread.Sleep(10);
                    i++;
                }
                // Send a test message to the first user in the list. In this 
                // test, the first user also happens to be the current user as 
                // we are running multiple clients.
                c.Message(
                    c.Users[0],
                    protocol.DataUri.FromFile(inputPath)
                );
                i = 0;
                // Wait for 1 second or message received, whichever comes first
                while (
                    c.Messages.Count < 1 &&
                    i < 100
                ) {
                    System.Threading.Thread.Sleep(10);
                    i++;
                }

                Assert.AreEqual(
                    1,
                    c.Messages.Count,
                    "Timeout exceeded or no message received"
                );

                string tmpFile = String.Format(
                    "{0}.txt",
                    System.IO.Path.GetTempFileName()
                );
                c.Messages[0].Contents.ToFile(tmpFile);
                Assert.AreEqual(
                    true, 
                    System.IO.File.Exists(tmpFile)
                );
                Assert.AreNotEqual(
                    0, new System.IO.FileInfo(tmpFile).Length
                );
                Assert.AreEqual(
                    System.IO.File.ReadAllBytes(inputPath).Length,
                    System.IO.File.ReadAllBytes(tmpFile).Length
                );
                System.IO.File.Delete(tmpFile);
            }
        }
    }
}
