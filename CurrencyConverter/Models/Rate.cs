using System;
using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Models
{
    public class Rate
    {
        [Key]
        public int Cur_ID { get; set; }

        public int Cur_Scale { get; set; }

        public string Cur_Name { get; set; }

        public decimal? Cur_OfficialRate { get; set; }

        public string Cur_Abbreviation { get; set; }

        public DateTime Date { get; set; }
    }
}
