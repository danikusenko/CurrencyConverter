using CurrencyConverter.Models.Chart;
using CurrencyConverter.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.ViewComponents
{
    [ViewComponent(Name = "chartjs")]
    public class ChartJsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var chartData = @"
            {
                type: 'line',
                responsive: true,
                data:
                {
                    labels: ['Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь'],                    
                    datasets: [{
                        label: '# of Votes',
                        lineTension: 0,
                        data: [48, 49, 38, 41, 39, 42],
                        backgroundColor: [
                        'rgba(2, 204, 39, 0.2)',                        
                            ],
                        borderColor: [
                        'rgba(0, 171, 31, 1)',
                        
                            ],
                        borderWidth: 2
                    }]
                },
                options:
                {                    
                    scales:
                    {
                        yAxes: [{
                            ticks:
                            {
                                beginAtZero: true
                            }
                        }]
                    },
                    elements: {
                        point:{
                            radius: 0
                        }
                    }
                }
            }";

            var chart = JsonConvert.DeserializeObject<ChartJs>(chartData);

            var chartModel = new ChartJsViewModel
            {
                Chart = chart,
                ChartJson = JsonConvert.SerializeObject(chart, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                })
            };

            return View(chartModel);
        }
    }
}
