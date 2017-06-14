using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.akoimeexx.network.palladium.engine {
    [Flags]
    public enum TransmissionStatus {
        Fail      = 0, 
        Success   = 1 << 0, 
        Receiving = 1 << 1, 
        Received  = 1 << 2, 
        Sending   = 1 << 3, 
        Sent      = 1 << 4
    }
}
