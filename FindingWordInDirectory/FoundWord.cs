using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FindingWordInDirectory
{
    public class FoundWord
    {
        //Full path + name
        public string FileName { get; set; }
        public string PathFolder { get; set; }
        public int NumberOfOccurrences { get; set; }
        //Only name of file
        public string FileNamePath => Path.GetFileName(FileName);
    }
}
