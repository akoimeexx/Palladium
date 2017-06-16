namespace com.akoimeexx.network.palladium.protocol {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text.RegularExpressions;

    public partial class DataUri {
#region Components
        /// <summary>
        /// MediaType Values
        /// </summary>
        public enum MediaTypes {
            // Custom protocol-based type
            ObjectType, // C# Object/<InstanceType>
            // Standard web-based types
            TextPlain, // Plain text
            ImageGif, // Image/Gif
            ImageJpg, // Image/Jpg
            ImagePng, // Image/Png
            ImageSvg, // Image/Svg
            ApplicationOctet // Binary File Transfer
        }
        internal static readonly Dictionary<MediaTypes, string> mediaTypes = 
            new Dictionary<MediaTypes, string>() {
                // Custom protocol-based type
                { MediaTypes.ObjectType, "object/C#" }, 
                // Standard web-based types
                { MediaTypes.TextPlain, "text/plain;charset=utf-8" }, 
                { MediaTypes.ImageGif, "image/gif" },
                { MediaTypes.ImageJpg, "image/jpg" },
                { MediaTypes.ImagePng, "image/png" },
                { MediaTypes.ImageSvg, "image/svg+xml" },
                { MediaTypes.ApplicationOctet, "application/octet" },
            };
        internal enum metaDataTypes {
            Filename, 
        }
#endregion Components
    }
    public partial class DataUri {
#region Properties
        // https://msdn.microsoft.com/en-us/library/system.net.mime(v=vs.110).aspx
        private Dictionary<metaDataTypes, string> _metaData = 
            new Dictionary<metaDataTypes, string>();
        /// <summary>
        /// Data content type descriptor
        /// </summary>
        public MediaTypes MediaType { get; private set; } = default(MediaTypes);
        /// <summary>
        /// 
        /// </summary>
        public bool IsBase64 { get { return !(_data is string); }}
        /// <summary>
        /// 
        /// </summary>
        public object Data {
            get { return _data; }
            set {
                MediaType = mediaTypeFromData(value);
                _data = value;
            }
        } private object _data = default(object);
#endregion Properties
    }
    public partial class DataUri {
#region Methods
        /// <summary>
        /// Creates an object from a binary-formatted memory stream byte array
        /// </summary>
        /// <param name="bytes">byte array representation of an object</param>
        /// <returns>reverted object</returns>
        internal static object byteArrayToData(byte[] bytes) {
            object o = default(object);
            try {
                BinaryFormatter bf = new BinaryFormatter();
                using (var ms = new MemoryStream(bytes)) {
                    ms.Seek(0, SeekOrigin.Begin);
                    o = bf.Deserialize(ms);
                }
            } finally { }
            return o;
        }
        /// <summary>
        /// Creates a binary-formatted memory stream byte array from an object
        /// </summary>
        /// <param name="data">object to convert</param>
        /// <returns>byte array representation of the object</returns>
        internal static byte[] dataToByteArray(object data) {
            byte[] b = default(byte[]);
            try {
                BinaryFormatter bf = new BinaryFormatter();
                using (var ms = new MemoryStream()) {
                    bf.Serialize(ms, data);
                    b = ms.ToArray();
                }
            } finally { }
            return b;
        }
        /// <summary>
        /// Creates a binary-formatted memory stream byte array from DataUri instance Data
        /// </summary>
        /// <returns>byte array representation of DataUri instance Data</returns>
        internal byte[] dataToByteArray() {
            return DataUri.dataToByteArray(Data);
        }
        /// <summary>
        /// Selects the appropriate media type for an object
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static MediaTypes mediaTypeFromData(object data) {
            MediaTypes m = MediaTypes.ObjectType;//default(MediaTypes);
            try {
                if (data is string) m = MediaTypes.TextPlain;
            } finally { }
            return m;
        }
        /// <summary>
        /// Loads a file into a datauri-style format; specific to Palladium protocol.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>DataUri instance containing the file</returns>
        public static DataUri FromFile(string path) {
            DataUri o = default(DataUri);
            try {
                using (FileStream fs = File.Open(
                    path, FileMode.Open, FileAccess.Read
                )) {
                    byte[] b = new byte[fs.Length];
                    fs.Read(b, 0, Convert.ToInt32(fs.Length));
                    o = new DataUri() {
                        Data = Convert.ToBase64String(b), 
                        MediaType = MediaTypes.ApplicationOctet
                    };
                    //int count, sum = 0;
                    //while ((count = fs.Read(
                    //    (byte[])o.Data, sum, (int)fs.Length - sum
                    //)) > 0) { sum += count; }
                    o._metaData.Add(
                        metaDataTypes.Filename, Path.GetFileName(path)
                    );
                }
            } finally { }
            return o;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="path"></param>
        public static bool ToFile(DataUri d, string path) {
            bool b = default(bool);
            try {
                using (FileStream fs = File.Open(
                    path, FileMode.OpenOrCreate, FileAccess.Write
                )) {
                    fs.SetLength(0);
                    byte[] data = Convert.FromBase64String(
                        d.Data as string ?? String.Empty
                    );
                    fs.Write(data, 0, data.Length);
                }
            } finally { b = File.Exists(path); }
            return b;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public bool ToFile(string path) { return DataUri.ToFile(this, path); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataUri"></param>
        /// <returns></returns>
        public static DataUri FromString(string dataUri) {
            DataUri o = default(DataUri);
            try {
                bool isBase64 = false;
                List<string> segments = null;
                var data = default(object);

                // Simple uri pattern matching, not 100% accurate
                if (!Regex.IsMatch(
                    dataUri,
                    @"^data:.*(;base64)?,\S*$", 
                    RegexOptions.CultureInvariant | 
                    RegexOptions.IgnoreCase | 
                    RegexOptions.Multiline
                )) throw new UriFormatException("Data URI is invalid");

                // Remove data:, split by ',' into two segments (meta data and data)
                segments = new List<string>(
                    dataUri.Substring(5).Split(new char[] { ',' }, 2)
                );
                if (segments.Count < 2) throw new UriFormatException(
                    "Not enough Data URI segments to deserialize from string"
                );

                // Validate whether data is base64-encoded...
                isBase64 = segments[0].Contains(";base64");
                // ... then remove ';base64' tag, if applicable
                segments[0] = segments[0].Replace(";base64", String.Empty);

                // Attempt to decode into a new object
                MediaTypes m = default(MediaTypes);
                foreach(KeyValuePair<MediaTypes, string> kvp in mediaTypes) {
                    if (segments[0].Contains(kvp.Value)) {
                        m = kvp.Key;
                        break;
                    }
                }
                o = new DataUri() {
                    _data = (isBase64) ? byteArrayToData(
                        Convert.FromBase64String(segments[1])
                    ) : Uri.UnescapeDataString(segments[1]), 
                    MediaType = m
                    //MediaType = mediaTypes.FirstOrDefault(
                    //    x => segments[0].StartsWith(x.Value)
                    //).Key
                };

                // If this is a transferred file, add meta data
                if (o.MediaType == MediaTypes.ApplicationOctet) {

                }
            } finally { }
            return o;
        }
        /// <summary>
        /// Generates a Data URI-based string, with optional meta data for media types such as Application/Octet (downloadable file)
        /// </summary>
        /// <returns>string representation of a DataUri instance</returns>
        public override string ToString() {
            return String.Format(
                "data:{0}{1},{2}",
                // Media Type
                mediaTypes[MediaType],
                // Base64-Encoded?
                (IsBase64) ? ";base64" : String.Empty, 
                // Data
                (IsBase64) ? Convert.ToBase64String(
                    (Data is byte[]) ? (byte[])Data : dataToByteArray()
                ) : Uri.EscapeDataString((string)Data)
            );
        }
#endregion Methods
    }
}
