using CurrencyConverter.Models;
using CurrencyConverter.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CurrencyConverter
{
    public class CustomStringLocalizer : IStringLocalizer
    {
        Dictionary<string, Dictionary<string, string>> resources;
        private IMemoryCache _memoryCache;

        public CustomStringLocalizer(IMemoryCache memoryCache)
        {
            List<Currency> currencies; 
            _memoryCache = memoryCache;
            if (!_memoryCache.TryGetValue("currencies", out currencies))
            {
                currencies = CurrencyService.GetCurrencies().Result;                
            }
            Dictionary<string, string> enDict = new Dictionary<string, string>();
            Dictionary<string, string> ruDict = new Dictionary<string, string>();
            Dictionary<string, string> beDict = new Dictionary<string, string>();

            foreach (var currency in currencies)
            {
                if (enDict.ContainsKey(currency.Cur_Abbreviation))
                {
                    enDict[currency.Cur_Abbreviation] = currency.Cur_Name_Eng;
                    ruDict[currency.Cur_Abbreviation] = currency.Cur_Name;
                    beDict[currency.Cur_Abbreviation] = currency.Cur_Name_Bel;
                }
                else
                {
                    enDict.Add(currency.Cur_Abbreviation, currency.Cur_Name_Eng);
                    ruDict.Add(currency.Cur_Abbreviation, currency.Cur_Name);
                    beDict.Add(currency.Cur_Abbreviation, currency.Cur_Name_Bel);
                }
            }

            resources = new Dictionary<string, Dictionary<string, string>>
            {
                {"en", enDict },
                {"ru", ruDict },
                {"be", beDict }
            };
        }

        public LocalizedString this[string name]
        {
            get
            {
                var currentCulture = CultureInfo.CurrentUICulture;
                string val = "";
                if (resources.ContainsKey(currentCulture.Name))
                {
                    if (resources[currentCulture.Name].ContainsKey(name))
                    {
                        val = resources[currentCulture.Name][name];
                    }
                }
                return new LocalizedString(name, val);
            }
        }

        public LocalizedString this[string name, params object[] arguments] => throw new NotImplementedException();

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return this;
        }
    }
}

