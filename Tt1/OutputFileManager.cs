using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tt1
{
    public class OutputFileManager
    {
        private string _path;
        Dictionary<string, FileStream> _streamDict;
        public OutputFileManager(string path)
        {
            _path = path;
            _streamDict = new Dictionary<string, FileStream>();
        }

        public void WriteRun(IEnumerable<string> runLines, IEnumerable<string> crsCodes)
        {
            foreach (var crs in crsCodes)
            {
                foreach (var line in runLines)
                {
                    WriteLine(crs, line);
                }
            }
        }

        public void WriteLine(string crsCode, string line)
        {
            var result = _streamDict.TryGetValue(crsCode, out var filestream);
            if (!result)
            {
                Directory.CreateDirectory(_path);
                var path = Path.Combine(_path, crsCode);
                filestream = File.OpenWrite(path + ".ttab");
                _streamDict.Add(crsCode, filestream);
            }
            var bytes = Encoding.ASCII.GetBytes(line);
            filestream.Write(bytes, 0, bytes.Length);
            filestream.WriteByte(0x0D);
            filestream.WriteByte(0x0A);
        }
    }
}
