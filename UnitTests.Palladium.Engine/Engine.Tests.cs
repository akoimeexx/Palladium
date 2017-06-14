using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Palladium.Engine {
    using protocol = com.akoimeexx.network.palladium.protocol;
    using engine = com.akoimeexx.network.palladium.engine;
    [TestClass]
    public class EngineTests {
        private protocol.Packet receivedPacket;
        private engine.TransmissionStatus receiveStatus;
        private protocol.Packet transmittedPacket;
        private engine.TransmissionStatus transmitStatus;
        
        /// <summary>
        /// Engine setup; resets test class fields and configures listeners for send/receive
        /// </summary>
        /// <returns>A properly configured engine with event listeners to update test class fields for testing</returns>
        [Ignore]
        private engine.Engine startYourEngine() {
            transmittedPacket = default(protocol.Packet);
            transmitStatus = default(engine.TransmissionStatus);
            engine.Engine e = new engine.Engine();
            e.Receive += (sender, args) => {
                receivedPacket = args.Packet;
                receiveStatus = args.Status;
            };
            e.Transmit += (sender, args) => {
                transmittedPacket = args.Packet;
                transmitStatus = args.Status;
            };
            return e;
        }
        [TestMethod]
        public void SendReceivePacket() {
            engine.Engine e = startYourEngine();
            protocol.Packet p = new protocol.Packet() {
                Contents = new protocol.DataUri() {
                    Data = "Hello world"
                }
            };
            e.Send(p);
            Assert.AreEqual(p, transmittedPacket);
            Assert.AreEqual(
                engine.TransmissionStatus.Sent | 
                engine.TransmissionStatus.Success, 
                transmitStatus
            );
            int i = 0;

            while (
                receiveStatus == default(engine.TransmissionStatus) && 
                i < 100
            ) {
                System.Threading.Thread.Sleep(10);
                i++;
            }
            Assert.AreEqual(
                p.Contents.Data, 
                receivedPacket.Contents.Data, 
                String.Format("Receive Status: {0}", receiveStatus.ToString())
            );
            Assert.AreEqual(
                engine.TransmissionStatus.Received |
                engine.TransmissionStatus.Success,
                receiveStatus
            );
            e.Close();
        }
        [TestMethod]
        [ExpectedException(
            typeof(System.Security.Cryptography.CryptographicException)
        )]
        public void ReceiveEncryptedPacket() {
            engine.Engine e = startYourEngine();
            protocol.Key receiverKeys = new protocol.Key();
            protocol.Key senderKey = new protocol.Key() {
                Public = receiverKeys.Public
            };

            protocol.Key thirdPartyKeys = new protocol.Key();
            
            string input = "Hello world";
            protocol.Packet p = new protocol.Packet() {
                Contents = new protocol.DataUri() {
                    Data = senderKey.Encrypt(input)
                }
            };

            e.Send(p);
            int i = 0;

            while (
                receiveStatus == default(engine.TransmissionStatus) && 
                i < 100
            ) {
                System.Threading.Thread.Sleep(10);
                i++;
            }
            Assert.AreEqual(
                input, 
                receiverKeys.Decrypt(
                    receivedPacket.Contents.Data as string ?? String.Empty
                ),
                String.Format("Receive Status: {0}", receiveStatus.ToString())
            );
            Assert.AreNotEqual(
                input, 
                thirdPartyKeys.Decrypt(
                    receivedPacket.Contents.Data as string ?? String.Empty
                ),
                String.Format("Receive Status: {0}", receiveStatus.ToString())
            );
            e.Close();
        }
    }
}
