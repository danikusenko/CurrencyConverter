using CurrencyConverter.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.ViewComponents
{
    [ViewComponent(Name = "chartjs")]
    public class ChartJsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Dictionary<string, decimal?> chart)
        {
            JArray colors = new JArray();
            JArray borderColor = new JArray();
            string[] labels = chart.Keys.ToArray();
            decimal?[] data = chart.Values.ToArray();
            double k = labels.Length > 300 ? 4 : 1;
            
            if (data.Length > 0)
            {
                if (data[0] > data[data.Length - 1])
                {
                    colors.Add("rgba(230, 74, 25, 0.2)");
                    borderColor.Add("rgba(230, 74, 25, 1)");
                }
                else
                {
                    colors.Add("rgba(15, 157, 88, 0.2)");
                    borderColor.Add("rgba(15, 157, 88, 1)");
                }
            }

            var chartData = @"
            {
                type: 'line',
                responsive: true,
                data:
                {
                    labels:" + new JArray(labels) + @",                    
                    datasets: [{
                        label: '',
                        lineTension: 0,
                        data:" + new JArray(data) + @", 
                        backgroundColor:" + colors + @",
                        borderColor:" + borderColor + @",
                        borderWidth: 2
                    }]
                },
                options:
                {      
                    scales:
                    {
                        x: {                                         
                            display: true,                                                    
                            scaleLabel:
                            {
                                display: true
                            },
                            ticks:
                            {                               
                                beginAtZero: false
                            }
                        },      
                        xAxes: [{
                            ticks:
                            {
                                maxRotation: 0,
                                minRotation: 0,
                                callback: function(tick, index, array) {                                                    
                                    return (index % " + k + @") ? '' : tick;                                              
                                }
                            }
                        }],
                        y: {
                            ticks:
                            {
                                beginAtZero: false,
                                display: true
                            },
                            scaleLabel:
                            {
                                display: true,
                                labelString: 'Slow SQL Queries'
                            }
                        }                     
                    },
                    elements: {
                        point:{
                            radius: 0
                        }
                    },
                    legend: {
                        display: false
                    },
                    tooltips: {
                        enabled: true,
                        callbacks: {
                            label: function(tooltipItem, data) {
                            var label = data.datasets[tooltipItem.datasetIndex].label || '';
                            if (label) {
                                label += ': ';
                            }
                            label += Math.round(tooltipItem.yLabel * 100) / 100;
                            return label;
                        }}
                    }
                }
            }";
           
            var chartModel = new ChartJsViewModel
            {                
                ChartJson = chartData                
            };

            return View(chartModel);
        }
    }
}
