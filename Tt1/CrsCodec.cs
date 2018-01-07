using System;
using System.Collections.Generic;
using System.IO;

namespace Tt1
{
    class CrsCodec
    {
        UInt32 _currentCount = 0;
        private Dictionary<string, UInt32> _crsToInt = new Dictionary<string, UInt32>();
        private Dictionary<UInt32, string> _intToCrs = new Dictionary<UInt32, string>();

        public CrsCodec()
        {
            _currentCount = 0;
        }
        public UInt32 AddCrs(string s)
        {
            UInt32 result;
            if (_crsToInt.TryGetValue(s, out var i))
            {
                result = i;
            }
            result = _currentCount++;
            _crsToInt[s] = result;
            return result;
        }

        public string GetCrs(UInt32 i)
        {
            if (!_intToCrs.TryGetValue(i, out var crs))
            {
                throw new Exception($"the value {i} does not correspond to a known CRS code");
            }
            return crs;
        }
        public UInt32 GetCompressedCrs(string crs)
        {
            if (!_crsToInt.TryGetValue(crs, out var result))
            {
                throw new Exception($"Unknown CRS: {crs}");
            }
            return result;
        }
        public void WriteCrsDictionary(BinaryWriter bw)
        {
            bw.Write((UInt16)_crsToInt.Count);
            foreach(var entry in _crsToInt)
            {
                var crs = entry.Key;
                if (crs.Length != 3)
                {
                    throw new Exception("Fatal error - CRS code must be three characters long");
                }
                UInt16 crsBase26 = (UInt16)((crs[0] - 'A') * 26 * 26 + (crs[1] - 'A') * 26 + (crs[2] - 'A'));
                bw.Write(crsBase26);
            }
        }

        public static CrsCodec CreateFromFile(string filename)
        {
            var codec = new CrsCodec();
            codec.ReadFromFile(filename);
            return codec;
        }

        public static CrsCodec CreateFromFile(BinaryReader br)
        {
            var codec = new CrsCodec();
            codec.ReadFromFile(br);
            return codec;
        }


        public void ReadFromFile(string filename)
        {
            _currentCount = 0;
            using (var stream = new FileStream(filename, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                var length = reader.ReadUInt16();
                for (UInt32 i = 0; i < length; ++i)
                {
                    var crsBase26 = reader.ReadUInt16();
                    string crs = "" + (char)('A' + crsBase26 / 26 / 26) + (char)(('A' + (crsBase26 / 26) % 26)) + (char)('A' + crsBase26 % 26);
                    _crsToInt.Add(crs, i);
                    _intToCrs.Add(i, crs);
                }
            }
        }

        public void ReadFromFile(BinaryReader reader)
        {
            var length = reader.ReadUInt16();
            for (UInt32 i = 0; i < length; ++i)
            {
                var crsBase26 = reader.ReadUInt16();
                string crs = "" + (char)('A' + crsBase26 / 26 / 26) + (char)(('A' + (crsBase26 / 26) % 26)) + (char)('A' + crsBase26 % 26);
                _crsToInt.Add(crs, i);
                _intToCrs.Add(i, crs);
            }
        }
    }
}
