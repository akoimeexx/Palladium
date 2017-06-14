namespace com.akoimeexx.network.palladium.protocol {
    using System;
    using System.Collections.Generic;

    public partial class User {
#region Properties
        /// <summary>
        /// Domain to which the user is part of, according to environment; set during instance construction only
        /// </summary>
        public string Domain { get; private set; } = default(string);
        /// <summary>
        /// UserName as reported by the environment; set during instance construction only
        /// </summary>
        public string Name { get; private set; } = default(string);
        /// <summary>
        /// Machine as reported by the environment; set during instance construction only
        /// </summary>
        public string Machine { get; private set; } = default(string);
        /// <summary>
        /// User-supplied nickname, if any
        /// </summary>
        public string Nick { get; set; } = default(string);
        /// <summary>
        /// 
        /// </summary>
        public Key Keys { get; private set; } = default(Key);
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> Properties { get; private set; } = 
            default(Dictionary<string, string>);
        /// <summary>
        /// 
        /// </summary>
        public Guid InstanceId { get; private set; } = default(Guid);
#endregion Properties
    }
    public partial class User {
#region Methods
        public override string ToString() {
            return String.Format(@"{0}\{1}@{2}{3}", 
                Domain, 
                Name, 
                Machine, 
                !String.IsNullOrWhiteSpace(Nick) ? 
                    String.Format(":{0}", Nick) : 
                    String.Empty
            );
        }
#endregion Methods
    }
    public partial class User {
#region Constructors & Destructor
        public User() : this(String.Empty) { }
        public User(string nickname) : this(
            nickname,
            Environment.UserDomainName, 
            Environment.UserName, 
            Environment.MachineName
        ) { }
        public User(
            string nickname, 
            string domain, 
            string username, 
            string machinename, 
            Dictionary<string, string> properties = null
        ) {
            Nick = nickname;
            Domain = domain;
            Name = username;
            Machine = machinename;
            InstanceId = Guid.NewGuid();

            Keys = new Key();
            Properties = properties ?? new Dictionary<string, string>();
        }
#endregion Constructors & Destructor
    }
}
