using CurrencyConverter.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.ViewModels
{
    public class CurrencyViewModel
    {
        public List<Rate> Rates { get; set; }

        public SelectList CurrencyNames { get; set; }

        public int? Cur_Id1 { get; set; }

        public int? Cur_Id2 { get; set; }

        public string Name1 { get; set; }

        public string Name2 { get; set; }

        public decimal? Value1 { get; set; }

        public decimal? Value2 { get; set; }

        public decimal?[] data { get; set; }

        public string[] labels { get; set; }

       // public string gap { get; set; }
    }
}
