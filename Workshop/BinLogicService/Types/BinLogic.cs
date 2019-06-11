using Module.Common.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinLogicService.Types
{
    class Bin
    {
        public Bin(int minEfficiency, int maxEfficiency, string binClass)
        {            
            this.MinEfficiency = minEfficiency;
            this.MaxEfficiency = maxEfficiency;
            this.BinClass = binClass;
        }

        public int MaxEfficiency { get;  }
        public int MinEfficiency { get;  }
        public string BinClass { get;  }
    }

    internal static class BinLogic
    {
        internal static string CalculateBin(SimResult simResult)
        {
            if (simResult == null)
            {
                return "CATCHALL-SimMissing";
            }

            var bin = GetBins().Where(b => (simResult.Efficiency * 100) >= b.MinEfficiency &&
                                 (simResult.Efficiency * 100) < b.MaxEfficiency).FirstOrDefault();

            return bin != null ? bin.BinClass : "CATCHALL-NoBin";
        }

        static List<Bin> GetBins()
        {
            return new List<Bin>()
            {
                new Bin(0, 5, "Scrap"),
                new Bin(5, 10, "300"),
                new Bin(10, 15, "350"),
                new Bin(15, 20, "400"),
                new Bin(20, 100, "450")
            };
        }
    }
}
