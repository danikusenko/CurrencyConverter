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
        public List<Currency> Currencies { get; set; }
        
        public SelectList CurrencyNames { get; set; }

        public string Name1 { get; set; }

        public string Name2 { get; set; }

        public string Value1 { get; set; }

        public string Value2 { get; set; }
    }
}
