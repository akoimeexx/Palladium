using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace com.akoimeexx.network.palladium.engine {
    using com.akoimeexx.network.palladium.protocol;

    public partial class Client : IDisposable {
#region Properties
        private Engine _client { get; set; } = default(Engine);
        public User CurrentUser { get; private set; } = default(User);
        private bool disposedValue = false;
        public IPAddress Host {
            get { return _client.Host; }
            internal set { _client.Host = value; }
        }
        public ObservableCollection<Packet> Messages {
            get; private set;
        } = default(ObservableCollection<Packet>);
        public int Port {
            get { return _client.Port; }
            internal set { _client.Port = value; }
        }
        public ObservableCollection<User> Users {
            get; private set;
        } = default(ObservableCollection<User>);
#endregion Properties
    }
    public partial class Client {
#region Events
        public event EventHandler<TransmissionEventArgs> Receive {
            add { _client.Receive += value; }
            remove { _client.Receive -= value; }
        }
        public event EventHandler<TransmissionEventArgs> Transmit {
            add { _client.Transmit += value; }
            remove { _client.Transmit -= value; }
        }
#endregion Events
    }
    public partial class Client {
#region Methods
        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects).
                    Logout();
                    _client.Close();
                    _client = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }
        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden below.
            // GC.SuppressFinalize(this);
        }
        public void Login() {
            Packet p = Packets.LoginAnnouncement;
            p.Source = CurrentUser;
            sendPacket(p);
        }
        public void Logout() {
            Packet p = Packets.LogoutAnnouncement;
            p.Source = CurrentUser;
            sendPacket(p);
            _client.Close();
        }
        public void Message(User recipient, DataUri message) {
            if (User.Equals(recipient, default(User)))
                throw new ArgumentNullException(nameof(recipient));
            if (DataUri.Equals(message, default(DataUri)))
                throw new ArgumentNullException(nameof(message));
            
            Packet p = Packets.UserMessage;
            p.Destination = recipient.ToString();
            p.Source = CurrentUser;
            p.Contents = recipient.Keys.Encrypt(
                message.ToString()
            );
            sendPacket(p);
        }
        public void Message(object channel, DataUri message) {
            throw new NotImplementedException();
        }
        private void rxAddUser(object sender, TransmissionEventArgs args) {
            // Verify this is a login message before attempting to add a user
            if (
                args.Status.HasFlag(TransmissionStatus.Success) &&
                args.Packet.Source != default(User) && 
                args.Packet.Destination ==
                    Packets.LoginAnnouncement.Destination && 
                String.Equals(
                    new DataUri(args.Packet.Contents).Data.ToString(),
                    new DataUri(Packets.LoginAnnouncement.Contents).Data.ToString()
                )
            ) {
                Users.Add(args.Packet.Source);
            }
        }
        private void rxReadMessage(object sender, TransmissionEventArgs args) {
            if (
                args.Status.HasFlag(TransmissionStatus.Success) &&
                args.Packet.Source != default(User) && 
                String.Equals(
                    args.Packet.Destination.ToString(),
                    String.Format(
                        Packets.UserMessage.Destination.ToString(), 
                        CurrentUser.ToString()
                    )
                )
            ) {
                Packet p = args.Packet;
                p.Contents = CurrentUser.Keys.Decrypt(args.Packet.Contents);
                
                //new DataUri(p.Contents).Data = new DataUri(CurrentUser.Keys.Decrypt(
                //    args.Packet.Contents
                //)).Data;
                Messages.Add(p);
                Packet r = Packets.MessageReceived;
                r.Source = CurrentUser;
                sendPacket(r);
            }
        }
        private void rxRemoveUser(object sender, TransmissionEventArgs args) {
            if (
                args.Status.HasFlag(TransmissionStatus.Success) &&
                args.Packet.Source != default(User) && 
                args.Packet.Destination ==
                    Packets.LogoutAnnouncement.Destination && 
                String.Equals(
                    new DataUri(args.Packet.Contents).Data.ToString(),
                    new DataUri(Packets.LogoutAnnouncement.Contents).Data.ToString()
                )
            ) {
                Users.Remove(args.Packet.Source);
            }
        }
        private void sendPacket(Packet packet) {
            if (Packet.Equals(packet, default(Packet)))
                throw new ArgumentNullException(nameof(packet));
            _client.Send(packet);
        }
        //private void txSendLogin(object sender, TransmissionEventArgs args) {

        //}
        //private void txSendLogout(object sender, TransmissionEventArgs args) {

        //}
        //private void txSendMessage(object sender, TransmissionEventArgs args) {

        //}
#endregion Methods
    }
    public partial class Client {
#region Constructors & Destructor
        public Client() : this(new User()) { }
        public Client(User user) : this(
            user, IPAddress.Broadcast, Engine.PORT
        ) { }
        public Client(IPAddress host) : this(host, Engine.PORT) { }
        public Client(int port) : this(IPAddress.Broadcast, port) { }
        public Client(IPAddress host, int port) : this(
            new User(), host, port
        ) { }
        public Client(User user, IPAddress host, int port) {
            CurrentUser = user;
            Users = new ObservableCollection<User>();
            Messages = new ObservableCollection<Packet>();
            _client = new Engine(host, port);

            Receive += rxAddUser;
            Receive += rxReadMessage;
            Receive += rxRemoveUser;

            //Transmit += txSendLogin;
            Login();
        }
        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Client() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }
#endregion Constructors & Destructor
    }
}
