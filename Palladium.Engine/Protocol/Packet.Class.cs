namespace com.akoimeexx.network.palladium.protocol {
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;

    /// <summary>
    /// Pre-made packets for Palladium Engine communication
    /// </summary>
    public static partial class Packets {
        /// <summary>
        /// 
        /// </summary>
        public static Packet ChannelMessage {
            get {
                return new Packet() {
                    Source = null, 
                    Destination = "{0}", // Channel.ToString()
                    Contents = new DataUri() { Data = "ENCODEDMESSAGE" }.ToString()
                };
            }
        }
        public static Packet InviteMessage {
            get {
                return new Packet() {
                    Source = null, // Channel.ToString()
                    Destination = "{0}", // User.ToString()
                    Contents = new DataUri() { Data = "INVITE {0} {1}" }.ToString() // Channel Public Key, Channel Private Key
                };
            }
        }
        /// <summary>
        /// Announces a new user has logged in and clients should request an updated list of online users
        /// </summary>
        public static Packet LoginAnnouncement {
            get {
                return new Packet() {
                    Source = null,
                    Destination = System.Net.IPAddress.Broadcast.ToString(), 
                    Contents = new DataUri() { Data = "LOGIN" }.ToString()
                };
            }
        }
        /// <summary>
        /// Announces a user has logged out and clients should accept an updated list of online users
        /// </summary>
        public static Packet LogoutAnnouncement {
            get {
                return new Packet() {
                    Source = null, 
                    Destination = System.Net.IPAddress.Broadcast.ToString(), 
                    Contents = new DataUri() { Data = "LOGOUT" }.ToString()
                };
            }
        }
        /// <summary>
        /// Announces receipt and compliance of a message Packet. I know "Roger" and "Wilco" are generally not used together (Wilco inferring Roger already), but goddamnit it's my protocol and I can slip in Space Quest references if I want.
        /// </summary>
        public static Packet MessageReceived {
            get {
                return new Packet() {
                    Source = null,
                    Destination = System.Net.IPAddress.Broadcast.ToString(),
                    Contents = new DataUri() { Data = "ROGER WILCO" }.ToString() // Heh, Space Quest...
                };
            }
        }
        /// <summary>
        /// Announces a user has changed their nickname and clients should update accordingly
        /// </summary>
        public static Packet NickAnnouncement {
            get {
                return new Packet() {
                    Source = null, 
                    Destination = System.Net.IPAddress.Broadcast.ToString(), 
                    Contents = new DataUri() { Data = "NICK" }.ToString()
                };
            }
        }
        /// <summary>
        /// Requests an updated list of online users
        /// </summary>
        public static Packet RequestOnlineUsers {
            get {
                return new Packet() {
                    Source = null, 
                    Destination = System.Net.IPAddress.Broadcast.ToString(), 
                    Contents = new DataUri() { Data = "MARCO" }.ToString()
                };
            }
        }
        /// <summary>
        /// Responds as an online user; source should be Palladium FQDN, contents should be "POLO <public.key>"
        /// </summary>
        public static Packet ResponseOnlineUsers {
            get {
                return new Packet() {
                    Source = null, 
                    Destination = System.Net.IPAddress.Broadcast.ToString(), 
                    Contents = new DataUri() { Data = "POLO" }.ToString()
                };
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static Packet UserMessage {
            get {
                return new Packet() {
                    Source = null, 
                    Destination = "{0}", // User.ToString()
                    Contents = new DataUri() { Data = "MESSAGE" }.ToString()
                };
            }
        }
    }

    /// <summary>
    /// JSON-Serializable data packet
    /// </summary>
    [DataContract]
    public partial class Packet {
        [DataMember]
        public Guid Id { get; private set; } = Guid.NewGuid();
        [DataMember]
        public DateTime Timestamp { get; private set; } = DateTime.Now;
        [IgnoreDataMember]
        public User Source { get; set; } = default(User);
        [DataMember(Name="Source")]
        /// <summary>
        /// Ugly "hack" to properly serialize User Source as string representation via DataContractJsonSerializer
        /// </summary>
        private string source {
            get { return Source?.ToString() ?? String.Empty; }
            set { Source = User.FromString(value); }
        }
        [DataMember]
        public string Destination { get; set; } = default(string);
        //[IgnoreDataMember]
        //public DataUri Contents { get; set; } = default(DataUri);
        ///// <summary>
        ///// Ugly "hack" to properly serialize DataUri Contents as string representation via DataContractJsonSerializer
        ///// </summary>
        //[DataMember(Name="Contents")]
        //private string contents {
        //    get { return Contents.ToString(); }
        //    set { Contents = DataUri.Parse(value); }
        //}
        [DataMember]
        public string Contents { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public partial class Packet {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Packet FromJson(string json) {
            Packet p = default(Packet);
            using (MemoryStream ms = new MemoryStream(
                Encoding.UTF8.GetBytes(json)
            )) {
                DataContractJsonSerializer s = 
                    new DataContractJsonSerializer(typeof(Packet));
                ms.Seek(0, SeekOrigin.Begin);
                p = (Packet)s.ReadObject(ms);
            }
            return p;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static string ToJson(Packet packet) {
            string json = String.Empty;
            using (MemoryStream ms = new MemoryStream()) {
                DataContractJsonSerializer s = 
                    new DataContractJsonSerializer(typeof(Packet));
                s.WriteObject(ms, packet);
                ms.Seek(0, SeekOrigin.Begin);
                using (StreamReader sr = new StreamReader(ms)) {
                    json = sr.ReadToEnd();
                }
            }
            return json;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToJson() {
            return ToJson(this);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static bool TryParse(string json, out Packet packet) {
            Packet p = null;
            try {
                p = Packet.FromJson(json);
            } finally { packet = p; }
            return !Packet.Equals(p, null);
        }
    }
    public partial class Packet {
        /// <summary>
        /// 
        /// </summary>
        public Packet() { }
    }
}
