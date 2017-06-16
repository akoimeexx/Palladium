using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.akoimeexx.network.palladium.engine {
    using com.akoimeexx.network.palladium.protocol;

    public partial class TransmissionEventArgs : EventArgs {
        public Packet Packet {
            get; internal set;
        } = default(Packet);
        public TransmissionStatus Status {
            get; internal set;
        } = default(TransmissionStatus);
        public bool Success {
            get { return Status.HasFlag(TransmissionStatus.Success); }
        }
    }
    public partial class TransmissionEventArgs {
        public TransmissionEventArgs() : this(default(Packet)) { }
        public TransmissionEventArgs(
            Packet packet
        ) : this(packet, default(TransmissionStatus)) { }
        public TransmissionEventArgs(
            TransmissionStatus status
        ) : this(default(Packet), status) { }
        public TransmissionEventArgs(
            Packet packet, 
            TransmissionStatus status
        ) {
            Packet = packet; 
            Status = status;
        }
    }
}
