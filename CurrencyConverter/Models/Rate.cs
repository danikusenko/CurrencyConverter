using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Models
{
    public class Rate
    {
        public int Cur_ID { get; set; }

        public int Cur_Scale { get; set; }

        public string Cur_Name { get; set; }

        public double Cur_OfficialRate { get; set; }
    }
}
