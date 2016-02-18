$(function() {
        // Create the chart
        chart = new Highcharts.Chart({
            chart: {
                renderTo: 'chart',
                type: 'pie'
            },
            title: {
                text: ''
            },
            yAxis: {
                title: {
                    text: ''
                }
            },
            plotOptions: {
                pie: {
                    shadow: false
                }
            },
            tooltip: {
                shadow:false,
                formatter: function() {
                    return '<b>'+ this.y +' %</b>';    
                }            
            },
            series: [{
                name: 'Browsers',
                data: [["TEST1",65],["TEST2",25],["TEST3",10]],
                colors: ['#64BF89', '#B4DCC4', '#227B46'],
                size: '95%',
                innerSize: '50%',
                showInLegend:false,
                dataLabels: {
                    enabled: false
                }
            }]
        });
    });
