using System;
using System.Text;

namespace gzWeb.Utilities
{
    public class DebugTextWriter : System.IO.TextWriter
    {
        public override void Write(char[] buffer, int index, int count)
        {
            System.Diagnostics.Debug.Write(new String(buffer, index, count));
        }
        public override void WriteLine(char[] buffer, int index, int count)
        {
            System.Diagnostics.Debug.WriteLine(new String(buffer, index, count));
        }
        public override void Write(string value) {
            System.Diagnostics.Debug.Write(value);
        }
        public override void WriteLine(string value)
        {
            System.Diagnostics.Debug.WriteLine(value);
        }
        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
    }
}
