using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace assigment1
{
    public class ReadFile
    {
        internal string[] readFile(string p)
        {
            if (!File.Exists(p))
                Console.WriteLine("file not found");
            return System.IO.Directory.GetFiles(p);
        }
    }
}
