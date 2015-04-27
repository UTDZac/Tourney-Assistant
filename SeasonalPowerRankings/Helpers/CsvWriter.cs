using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ChallongeMatchViewer.Helpers
{
    class CsvWriter
    {
        public static void WriteLine(string filepath, string str, params object[] args)
        {
            WriteLine(filepath, string.Format(str, args));
        }

        public static void WriteLine(string filepath, string str)
        {
            using (StreamWriter writer = new StreamWriter(filepath, true))
            {
                writer.WriteLine(str);
            }
        }
    }
}
