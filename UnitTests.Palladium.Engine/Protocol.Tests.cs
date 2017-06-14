using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Palladium.Engine {
    using Protocol = com.akoimeexx.network.palladium.protocol;

    [TestClass]
    public class ProtocolTests {
        [TestMethod]
        public void DataUriStringData() {
            string inputData = "Hello world";
            string expected = String.Format(
                "data:text/plain;charset=utf-8,{0}",
                Uri.EscapeDataString(inputData)
            );
            Protocol.DataUri d = new Protocol.DataUri() {
                Data = inputData
            };
            Assert.AreEqual(expected, d.ToString());

            Assert.AreEqual(
                inputData, 
                Protocol.DataUri.FromString(d.ToString()).Data
            );
        }
        [TestMethod]
        public void DataUriObjectData() {
            string[] inputData = "Hello world".Split(' ');
            string expected = "data:object/C#;base64,AAEAAAD/////AQAAAAAAAAARAQAAAAIAAAAGAgAAAAVIZWxsbwYDAAAABXdvcmxkCw==";

            Protocol.DataUri d = new Protocol.DataUri() {
                Data = inputData
            };
            Assert.AreEqual(expected, d.ToString());

            Assert.AreEqual(
                String.Join(" ", inputData),
                String.Join(
                    " ", 
                    (string[])(Protocol.DataUri.FromString(d.ToString()).Data)
                )
            );
        }
        [TestMethod]
        public void DataUriFromFile() {
            Protocol.DataUri d = Protocol.DataUri.FromFile(
                "../../HelloWorld.txt"
            );
            Assert.AreNotEqual(default(Protocol.DataUri), d);
        }
        [TestMethod]
        public void DataUriToFile() {
            Protocol.DataUri d = Protocol.DataUri.FromFile(
                "../../HelloWorld.txt"
            );
            string tmpFile = String.Format(
                "{0}.txt", 
                System.IO.Path.GetTempFileName()
            );
            Assert.AreEqual(true, d.ToFile(tmpFile));
            Assert.AreNotEqual(
                0, new System.IO.FileInfo(tmpFile).Length
            );
            System.IO.File.Delete(tmpFile);
        }
        [TestMethod]
        [ExpectedException(
            typeof(System.Security.Cryptography.CryptographicException)
        )]
        public void KeyEncryption() {
            string input = "Hello world";
            Protocol.Key receiver = new Protocol.Key();
            Protocol.Key sender = new Protocol.Key() {
                Public = receiver.Public
            };

            string encrypted = sender.Encrypt(input);
            Assert.AreEqual(
                input,
                receiver.Decrypt(encrypted)
            );
            Assert.AreNotEqual(
                input,
                sender.Decrypt(encrypted)
            );
        }
        [TestMethod]
        public void PacketSerialization() {
            string input = "Hello world";
            Protocol.Packet p = new Protocol.Packet() {
                Contents = new Protocol.DataUri() {
                    Data = input
                }
            };
            string expected = String.Join(
                "", 
                "{\"Contents\":\"data:text\\/plain;charset=utf-8,", 
                Uri.EscapeDataString(input), 
                "\",\"Destination\":null,\"Id\":\"", 
                p.Id.ToString(), 
                "\",\"Source\":\"\",\"Timestamp\":\"\\/Date(", 
                new DateTimeOffset(p.Timestamp).ToUnixTimeMilliseconds().ToString(), 
                (TimeZone.CurrentTimeZone.GetUtcOffset(p.Timestamp) < TimeSpan.Zero) ? "-" : "", 
                TimeZone.CurrentTimeZone.GetUtcOffset(p.Timestamp).ToString("hhmm"), 
                ")\\/\"}"
            );
            
            Assert.AreEqual(expected, p.ToJson());
            Assert.AreEqual(
                input,
                Protocol.Packet.FromJson(
                    p.ToJson()
                ).Contents.Data
            );
        }
        [TestMethod]
        public void CreateUserInstance() {
            string expected = @"Domain\username@machinename:Nickname";
            Protocol.User u = new Protocol.User(
                "Nickname", 
                "Domain", 
                "username", 
                "machinename"
            );
            Assert.AreEqual(expected, u.ToString());
        }
    }
}
