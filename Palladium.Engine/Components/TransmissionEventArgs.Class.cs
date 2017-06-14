using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.akoimeexx.network.palladium.engine {
    public partial class TransmissionEventArgs : EventArgs {
        public protocol.Packet Packet {
            get; internal set;
        } = default(protocol.Packet);
        public TransmissionStatus Status {
            get; internal set;
        } = default(TransmissionStatus);
        public bool Success {
            get { return Status.HasFlag(TransmissionStatus.Success); }
        }
    }
    public partial class TransmissionEventArgs {
        public TransmissionEventArgs() : this(default(protocol.Packet)) { }
        public TransmissionEventArgs(
            protocol.Packet packet
        ) : this(packet, default(TransmissionStatus)) { }
        public TransmissionEventArgs(
            TransmissionStatus status
        ) : this(default(protocol.Packet), status) { }
        public TransmissionEventArgs(
            protocol.Packet packet, 
            TransmissionStatus status
        ) {
            Packet = packet; 
            Status = status;
        }
    }
}
