namespace com.akoimeexx.network.palladium.protocol {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text.RegularExpressions;

    public sealed partial class DataUri : IDataUri {
#region Components
        private const string SCHEME = "data";
        private enum DataUriMediaTypes {
            // Custom protocol-based type
            ObjectType, // C# Object/<InstanceType>
            // Standard web-based types
            TextPlain,  // Plain text
            ImageGif,   // Image/Gif
            ImageJpg,   // Image/Jpg
            ImagePng,   // Image/Png
            ImageSvg,   // Image/Svg
            ApplicationOctet // Binary File Transfer
        }
        private enum DataUriMetadataTypes {
            Charset,  // TextPlain encoding charset
            Filename, // ApplicationOctet file name
            Syntax,   // TextPlain optional language syntax
            Type      // ObjectType class instance type
        }
        private static readonly Dictionary<DataUriMediaTypes, string> mediaTypes = 
            new Dictionary<DataUriMediaTypes, string>() {
                // Custom protocol-based type
                { DataUriMediaTypes.ObjectType, "object/C#" }, // metadata: ;type=TypeName (;type=System.String)
                // Standard web-based types
                { DataUriMediaTypes.TextPlain, "text/plain" }, // metadata: ;charset=CHARSET (;charset=utf-8)
                                                               // optional: ;syntax=language (;syntax=(asm|c|cpp|cs|css|csv|html|js|json|h|xaml|xml|xsd|xsl|xslt))
                { DataUriMediaTypes.ImageGif, "image/gif" },
                { DataUriMediaTypes.ImageJpg, "image/jpg" },
                { DataUriMediaTypes.ImagePng, "image/png" },
                { DataUriMediaTypes.ImageSvg, "image/svg+xml" },
                { DataUriMediaTypes.ApplicationOctet, "application/octet" }, // metadata: ;filename=file.ext (;filename=helloworld.exe)
            };
#endregion Components
    }
    public sealed partial class DataUri {
#region Properties
        public object Data {
            get { return _data; }
            set {
                _mediaType = mediaTypeFromData(value);
                _metadata.Clear();
                switch (_mediaType) {
                    case DataUriMediaTypes.TextPlain:
                        _metadata.Add(DataUriMetadataTypes.Charset, "utf-8");
                        break;
                    case DataUriMediaTypes.ObjectType:
                        _metadata.Add(
                            DataUriMetadataTypes.Type, 
                            value.GetType().FullName
                        );
                        break;
                    default:
                        break;
                }
                _data = value;
            }
        } private object _data = default(object);
        public bool IsBase64 {
            get {
                return (_data == null) ? false : !(_data is string);
            }
        }
        public bool IsFile {
            get { return _metadata.ContainsKey(DataUriMetadataTypes.Filename); }
        }
        private DataUriMediaTypes? _mediaType = default(DataUriMediaTypes?);
        public IReadOnlyDictionary<string, string> Metadata {
            get {
                Dictionary<string, string> d = new Dictionary<string, string>();
                foreach (var m in _metadata)
                    d.Add(m.Key.ToString(), m.Value);
                return d;
            }
        } private Dictionary<DataUriMetadataTypes, string> _metadata = 
            new Dictionary<DataUriMetadataTypes, string>();
        public string Scheme { get { return SCHEME; } }
#endregion Properties
    }
    public sealed partial class DataUri {
#region Methods
        /// <summary>
        /// Creates an object from a binary-formatted memory stream byte array
        /// </summary>
        /// <param name="bytes">byte array representation of an object</param>
        /// <returns>reverted object</returns>
        private static object byteArrayToData(byte[] bytes) {
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
        private static byte[] dataToByteArray(object data) {
            byte[] b = default(byte[]);
            if (!object.Equals(default(object), data)) try {
                BinaryFormatter bf = new BinaryFormatter();
                using (var ms = new MemoryStream()) {
                    bf.Serialize(ms, data);
                    b = ms.ToArray();
                }
            } finally { }
            return b;
        }
        /// <summary>
        /// Selects the appropriate media type for an object
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static DataUriMediaTypes? mediaTypeFromData(object data) {
            DataUriMediaTypes? m = default(DataUriMediaTypes);
            if (!object.Equals(default(object), data)) try {
                m = DataUriMediaTypes.ObjectType;

                if (data is string) m = DataUriMediaTypes.TextPlain;
            } finally { }
            return m;
        }
        public static DataUri Parse(string dataUriString) {
            DataUri d = default(DataUri);
            Regex pattern = new Regex(
                @"^data:(([a-zA-Z]+\/[^\s,;]+){1}(;[a-zA-Z]+=[^\s,;]+)*)?(;base64)?,(\S*)$"
            );
            if (!pattern.IsMatch(dataUriString)) throw new FormatException(
                String.Format(
                    "{0} is not a valid DataUri string format", 
                    nameof(dataUriString)
                )
            );

            Match m = pattern.Match(dataUriString);
            // Get Media Type, if any
            DataUriMediaTypes? mediaType = default(DataUriMediaTypes?);
            List<DataUriMediaTypes> keys = new List<DataUriMediaTypes>(mediaTypes.Keys);
            List<string> values = new List<string>(mediaTypes.Values);
            if (
                values.Contains(m.Groups[2].Value)
            ) mediaType = keys[
                values.IndexOf(m.Groups[2].Value)
            ];

            // Get Metadata, if any
            Dictionary<DataUriMetadataTypes, string> metaData = new Dictionary<DataUriMetadataTypes, string>();
            foreach (var datum in m.Groups[3].Captures) {
                string[] keyvalue = datum.ToString().Split('=');
                DataUriMetadataTypes metaType = default(DataUriMetadataTypes);
                if (
                    (keyvalue?.Length ?? 0) > 1 && 
                    Enum.TryParse(keyvalue[0].Substring(1), true, out metaType)
                ) metaData.Add(metaType, keyvalue[1]);
            }

            // Check if Base64-Encoded
            bool isBase64 = !String.IsNullOrWhiteSpace(m.Groups[4].Value);

            // Get Data, if any
            string data = m.Groups[5].Value;
            // Build DataUri object
            d = new DataUri() {
                _mediaType = mediaType
            };
            foreach (var datum in metaData)
                d._metadata.Add(datum.Key, datum.Value);

            if (d._mediaType == DataUriMediaTypes.ObjectType) {
                d._data = DataUri.byteArrayToData(
                    Convert.FromBase64String(data)
                );
            } else if (isBase64) {
                d._data = Convert.FromBase64String(data);
            } else {
                d._data = (String.IsNullOrEmpty(data)) ? 
                    null : 
                    Uri.UnescapeDataString(data ?? String.Empty);
            }
            return d;
        }
        public static DataUri ParseFile(string path) {
            DataUri d = default(DataUri);
            if (File.Exists(path)) {
                try {
                    d = new DataUri() {
                        _data = File.ReadAllBytes(path),
                        _mediaType = DataUriMediaTypes.ApplicationOctet
                    };
                    d._metadata.Add(
                        DataUriMetadataTypes.Filename, 
                        Path.GetFileName(path)
                    );
                } finally { }
            }
            return d;
        }
        public static bool ToFile(DataUri dataUri, string directory) {
            bool b = default(bool);
            if (dataUri.IsFile) {
                try {
                    File.WriteAllBytes(
                        Path.Combine(
                            directory, 
                            dataUri._metadata[DataUriMetadataTypes.Filename]
                        ), 
                        dataUri._data as byte[]
                    );
                } finally {
                    b = File.Exists(Path.Combine(
                        directory, 
                        dataUri._metadata[DataUriMetadataTypes.Filename]
                    ));
                }
            }
            return b;
        }
        public bool ToFile(string directory) {
            return DataUri.ToFile(this, directory);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// String formatting syntax: 
        /// <![CDATA[
        /// data:[<mediatype>[;attribute=value]...][;base64],<data>
        /// ]]>
        /// </remarks>
        /// <returns></returns>
        public override string ToString() {
            // Get mediatype+metadata string
            string mediaMetaString = default(string);
            if (
                !object.Equals(default(DataUriMediaTypes?), _mediaType) &&
                mediaTypes.ContainsKey((DataUriMediaTypes)_mediaType)
            ) {
                mediaMetaString = mediaTypes[(DataUriMediaTypes)_mediaType];
                if (_metadata.Count > 0) {
                    List<string> metadata = new List<string>();
                    foreach (var m in _metadata) metadata.Add(String.Format(
                        "{0}={1}", 
                        m.Key.ToString().ToLower(), 
                        m.Value
                    ));
                    mediaMetaString = String.Format(
                        "{0};{1}",
                        mediaTypes[(DataUriMediaTypes)_mediaType],
                        String.Join(";", metadata)
                    );
                }
            }

            // Get data string
            string datastring = Uri.EscapeDataString(
                _data?.ToString() ?? String.Empty
            );
            if (IsBase64) {
                datastring = "";
                byte[] b = (_data is byte[]) ? 
                    _data as byte[] : 
                    dataToByteArray(_data);
                if (!object.Equals(default(byte[]), b))
                    datastring = Convert.ToBase64String(b);
            }
            string dataUriString = String.Format("{0}:{1}{2},{3}",
                Scheme,                              // DataUri Scheme
                mediaMetaString,                     // Media Type + Metadata
                IsBase64 ? ";base64" : String.Empty, // Base64 Encoding
                datastring                           // Data
            );
            return dataUriString;
        }
        public static bool TryParse(string dataUriString, out DataUri dataUri) {
            bool b = default(bool);
            dataUri = default(DataUri);
            try {
                dataUri = DataUri.Parse(dataUriString);
            } finally { b = !object.Equals(default(DataUri), dataUri); }
            return b;
        }
        public static bool TryParseFile(string path, out DataUri dataUri) {
            bool b = default(bool);
            dataUri = default(DataUri);
            try {
                dataUri = DataUri.ParseFile(path);
            } finally { b = !object.Equals(default(DataUri), dataUri); }
            return b;
        }
#endregion Methods
    }
    public sealed partial class DataUri {
#region Constructors & Destructor
        public DataUri() { }
        public DataUri(string dataUriString) {
            DataUri d = DataUri.Parse(dataUriString);
            _data = d._data;
            _metadata = d._metadata;
            _mediaType = d._mediaType;
            d = null;
        }
#endregion Constructors & Destructor
    }
}
