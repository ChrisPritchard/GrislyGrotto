using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrislyGrotto.Backup.GG10.Data
{
    public class Asset
    {
        public int ID { get; set; }
        public string Key { get; set; }
        public byte[] Data { get; set; }
    }
}
