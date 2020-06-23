using System;

namespace CurrencyConverter.Models
{
    public class Rate
    {
        public int Cur_ID { get; set; }

        public int Cur_Scale { get; set; }

        public string Cur_Name { get; set; }

        public double Cur_OfficialRate { get; set; }

        public string Cur_Abbreviation { get; set; }

        public DateTime Date { get; set; }
    }
}
