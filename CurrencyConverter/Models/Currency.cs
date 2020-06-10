using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Models
{
    public class Currency
    {
        public string Name { get; set; }
        public string Rate { get; set; }

        public double Scale { get; set; }
        public string NumCode { get; set; }
    }
}
