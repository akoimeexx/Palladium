using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace com.akoimeexx.network.palladium.engine.protocol {
    // Some properties based off System.Uri class;
    // see https://msdn.microsoft.com/en-us/library/system.uri(v=vs.110).aspx for more details
    public interface IDataUri : ISerializable {
        bool IsBase64 { get; }
        bool IsFile { get; }
        string OriginalString { get; }
        bool UserEscaped { get; }


    }

    [Serializable]
    //[TypeConverterAttribute(typeof(DataUriTypeConverter))]
    public class DropInReplacementDataUri : IDataUri {
        public bool IsBase64 { get; }
        public bool IsFile { get; }
        public string OriginalString { get; }
        public bool UserEscaped { get; }

        public static int Compare() { return default(int); }
        public override bool Equals(object obj) {
            return base.Equals(obj);
        }
        public static string EscapeDataString(string input) {
            return default(string);
        }
        public static string EscapeUriString(string input) {
            return default(string);
        }
        public static int FromHex(char c) {
            return default(int);
        }
        public object GetComponents() {
            return default(object);
        }
        public override int GetHashCode() {
            return base.GetHashCode();
        }
        public string GetLeftPart() { return default(string); }

        [SecurityPermission(
            SecurityAction.LinkDemand, 
            SerializationFormatter=true
        )]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {

        }
        public static string HexEscape(char c) { return default(string); }
        public static char HexUnescape(string pattern, ref int index) { return default(char); }
        public static bool IsHexDigit(char c) { return default(bool); }
        public static bool IsHexEncoding(string s, int i) { return default(bool); }
        public bool IsWellFormedOriginalString() { return default(bool); }
        public static bool IsWellFormedDataUriString(string s) { return default(bool); }
        public override string ToString() {
            return base.ToString();
        }
        public static bool TryCreate(string s, out DropInReplacementDataUri o) {
            o = default(DropInReplacementDataUri);
            return default(bool);
        }
        public static string UnescapeDataString(string s) { return default(string); }

        DropInReplacementDataUri(string dataUri) { }
        DropInReplacementDataUri(string dataUri, bool escapeChars) { }
    }
}
