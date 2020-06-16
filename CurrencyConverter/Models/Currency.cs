﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Models
{
    public class Currency
    {
        public int Cur_ID { get; set; }
        public string Cur_Name { get; set; }
        public double Cur_OfficialRate { get; set; }

        public double Cur_Scale { get; set; }
        //public string NumCode { get; set; }
    }
}
