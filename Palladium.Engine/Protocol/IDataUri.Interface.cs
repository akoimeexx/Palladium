namespace com.akoimeexx.network.palladium.protocol {
    /// <summary>
    /// DataUri interface, very loosely modeled after the standard System.Uri class definition
    /// </summary>
    /// <seealso cref="https://en.wikipedia.org/wiki/Data_URI_scheme"/>
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;

    public interface IDataUri {
        object Data { get; set; }
        bool IsBase64 { get; }
        bool IsFile { get; }
        IReadOnlyDictionary<string, string> Metadata { get; }
        string Scheme { get; }

        bool ToFile(string path);
        string ToString();
    }
}
