using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace com.akoimeexx.network.palladium.engine {
    using com.akoimeexx.network.palladium.protocol;

    public partial class Engine {
#region Properties
        /// <summary>
        /// Picked as a joke...
        /// </summary>
        internal const int PORT = 13370;
        private readonly UdpClient _receiver;
        private IAsyncResult _rxResult;

        public IPAddress Host { get; internal set; } = IPAddress.Broadcast;
        public int Port { get; internal set; } = PORT;
#endregion Properties
    }
    public partial class Engine {
#region Events
        public event EventHandler<TransmissionEventArgs> Receive;
        public event EventHandler<TransmissionEventArgs> Transmit;
#endregion Events
    }
    public partial class Engine {
#region Methods
        public void Close() {
            _receiver?.Close();
        }
        private void listen() {
            _rxResult = _receiver.BeginReceive(
                receiveTransmission, 
                TransmissionStatus.Receiving
            );
        }
        private void receiveTransmission(IAsyncResult result) {
            Packet packet = default(Packet);
            TransmissionStatus status = default(TransmissionStatus);
            try {
                status = 
                    (TransmissionStatus)result.AsyncState |
                    TransmissionStatus.Fail;

                if (
                    !((TransmissionStatus)result.AsyncState).HasFlag(
                        TransmissionStatus.Receiving
                    )
                ) throw new Exception(String.Format(
                    "invalid state: {0}",
                    ((TransmissionStatus)result.AsyncState).ToString()
                ));

                IPEndPoint rxEndPoint = new IPEndPoint(IPAddress.Any, Port);
                if (
                    Packet.TryParse(
                        Encoding.UTF8.GetString(
                            _receiver.EndReceive(result, ref rxEndPoint)
                        ), 
                        out packet
                    ) == false
                ) throw new Exception("could not deserialize the packet");

                status = 
                    TransmissionStatus.Received | 
                    TransmissionStatus.Success;
            } finally {
                Receive?.Invoke(
                    this,
                    new TransmissionEventArgs(
                        packet,
                        status
                    )
                );
                listen();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        public void Send(Packet packet) {
            TransmissionStatus status = default(TransmissionStatus);
            try {
                status =
                    TransmissionStatus.Sending | 
                    TransmissionStatus.Fail;

                using (UdpClient transmitter = new UdpClient()) {
                    byte[] txBytes = Encoding.UTF8.GetBytes(packet.ToJson());
                    transmitter.Send(
                        txBytes,
                        txBytes.Length,
                        new IPEndPoint(Host, Port)
                    );
                    transmitter.Close();
                }
                status =
                    TransmissionStatus.Sent |
                    TransmissionStatus.Success;
            } finally {
                Transmit?.Invoke(
                    this, new TransmissionEventArgs(
                        packet, 
                        status
                    )
                );
            }
        }
#endregion Methods
    }
    public partial class Engine {
#region Constructors & Destructor
        public Engine() : this(PORT) { }
        public Engine(int port) : this(default(IPAddress), port) { }
        public Engine(IPAddress host) : this(host, PORT) { }
        public Engine(IPAddress host, int port) {
            Host = host ?? IPAddress.Broadcast;
            Port = port;

            _receiver = new UdpClient(Port);
            listen();
        }
        ~Engine() {
            _receiver?.Close();
            foreach (
                var d in Receive?.GetInvocationList() ?? new Delegate[] { }
            ) Receive -= (EventHandler<TransmissionEventArgs>)d;
            foreach (
                var d in Transmit?.GetInvocationList() ?? new Delegate[] { }
            ) Transmit -= (EventHandler<TransmissionEventArgs>)d;
        }
#endregion Constructors & Destructor
    }
}
