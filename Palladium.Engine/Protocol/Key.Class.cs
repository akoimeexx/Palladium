namespace com.akoimeexx.network.palladium.protocol {
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;

    // See the following for references:
    // https://msdn.microsoft.com/en-us/library/5e9ft273(v=vs.110).aspx
    // http://stackoverflow.com/questions/18850030/aes-256-encryption-public-and-private-key-how-can-i-generate-and-use-it-net

    /// <summary>
    /// Palladium Engine Public/Private Keystore for encrypting secure messages
    /// </summary>
    public sealed partial class Key {
#region Properties
        /// <summary>
        /// Public Key token 
        /// </summary>
        public string Public { get; set; }
        /// <summary>
        /// Private Key token, readonly on class initialization
        /// </summary>
        private SecureString Private { get; set; }
#endregion Properties
    }
    public sealed partial class Key {
#region Methods
        /// <summary>
        /// Decrypt data encrypted with the public key token for this Key instance, using this Key instance Private Key token
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <returns>Decrypted string</returns>
        public string Decrypt(string encryptedData) {
            return Key.Decrypt(this, encryptedData);
        }
        /// <summary>
        /// Decrypt data encrypted with Key instance Public Key token, using the provided Private Key token
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="encryptedData"></param>
        /// <returns>Decrypted string</returns>
        public static string Decrypt(
            Key key, string encryptedData
        ) {
            string sReturn = String.Empty, sPrivate = String.Empty;
            try {
                sPrivate = Key.retrievePrivateKey(key);

                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(
                    1024, new CspParameters() {
                        ProviderType = (int)CspProviderFlags.UseMachineKeyStore
                    }
                );
                rsa.ImportCspBlob(Convert.FromBase64String(sPrivate));
                sReturn = Encoding.UTF8.GetString(
                    rsa.Decrypt(Convert.FromBase64String(encryptedData), false)
                );
            } finally {
                sPrivate = String.Empty;
            }
            return sReturn;
        }
        /// <summary>
        /// Encrypt data using the current instance Public Key token
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Encrypt(string data) {
            return Key.Encrypt(Public, data);
        }
        /// <summary>
        /// Encrypt data Using the supplied Public Key token
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Encrypt(string publicKey, string data) {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(
                1024, new CspParameters() {
                    ProviderType = (int)CspProviderFlags.UseMachineKeyStore
                }
            );
            rsa.ImportCspBlob(Convert.FromBase64String(publicKey));
            return Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(data), false));
        }
        /// <summary>
        /// Retrieves the private key from current Key instance
        /// </summary>
        /// <returns>Key.Private string representation</returns>
        internal string retrievePrivateKey() {
            return Key.retrievePrivateKey(this);
        }
        /// <summary>
        /// Retrieves the private key from supplied Key instance
        /// </summary>
        /// <param name="key">Key instance to retrieve the private key from</param>
        /// <returns>Key.Private string representation</returns>
        internal static string retrievePrivateKey(Key key) {
            string s = default(string);
            IntPtr p = IntPtr.Zero;
            try {
                // Retrieve contents in cleartext (unsafe, but only point of SecureString exposure), see:
                // http://stackoverflow.com/questions/818704/how-to-convert-securestring-to-system-string
                p = Marshal.SecureStringToGlobalAllocUnicode(key.Private);
                s = Marshal.PtrToStringUni(p);
            } finally { Marshal.ZeroFreeGlobalAllocUnicode(p); }
            return s;
        }
#endregion Methods
    }
    public sealed partial class Key {
#region Constructors & Destructor
        /// <summary>
        /// Default constructor, auto-propagates a new Public/Private Key token pair
        /// </summary>
        public Key() {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(
                1024, new CspParameters() {
                    ProviderType = (int)CspProviderFlags.UseMachineKeyStore
                }
            );
            Public = Convert.ToBase64String(rsa.ExportCspBlob(false));
            Private = new SecureString();
            Private.Clear();
            foreach (
                char c in Convert.ToBase64String(
                    rsa.ExportCspBlob(true)
                ).ToCharArray()
            ) Private.AppendChar(c);
            Private.MakeReadOnly();
        }
        /// <summary>
        /// Create a key instance using already-existing Public/Private Key token pairs
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="privateKey"></param>
        public Key(string publicKey, string privateKey) {
            if (
                String.IsNullOrWhiteSpace(publicKey)
            ) throw new ArgumentNullException(
                "publicKey cannot be null or whitespace"
            );
            if (
                String.IsNullOrWhiteSpace(privateKey)
            ) throw new ArgumentNullException(
                "privateKey cannot be null or whitespace"
            );
            Public = publicKey;
            Private = new SecureString();
            Private.Clear();
            foreach (char c in privateKey.ToCharArray())
                Private.AppendChar(c);
            Private.MakeReadOnly();
        }
        /// <summary>
        /// Destructor, handles Key cleanup
        /// </summary>
        ~Key() {
            Public = String.Empty;
            Private.Dispose();
        }
#endregion Constructors & Destructor
    }
}
