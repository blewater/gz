(function () {
    'use strict';

    APP.directive('gzPlansAllocationChart', ['iso4217', '$filter', directiveFactory]);

    function directiveFactory(iso4217, $filter) {
        return {
            restrict: 'A',
            scope: {
                gzPlans: '=',
                gzInvestmentAmount: '=',
                gzCurrency: '@',
            },
            link: function (scope, element, attrs) {
                scope.$watch('gzPlans', function (newValue, oldValue) {
                    drawDonut(newValue);
                });

                function drawDonut (plans) {
                    var root = d3.select(element[0]);
                    root.select("svg").remove();
                    var rootWidth = root.node().getBoundingClientRect().width;
                    var rootHeight = root.node().getBoundingClientRect().height;
                    var minDimension = Math.min(rootWidth, rootHeight) / 2;
                    var externalPadding = 10;
                    var donutThickness = 50;
                    var outerRadius = minDimension - externalPadding;
                    var extension = 8;
                    var innerRadius = minDimension - externalPadding - donutThickness;
                    var fontSize = 14 * outerRadius / 100;
                    var duration = 400;
                    for (var k = 0; k < plans.length; k++)
                        plans[k].Gamma = plans[k].AllocatedPercent / 30;
                    var canvas =
                        root.append("svg")
                            .attr("width", minDimension * 2)
                            .attr("height", minDimension * 2);

                    var group = canvas.append("g").attr("transform", "translate(" + minDimension + "," + minDimension + ")");

                    //var tooltip = d3.tip().attr('class', 'tooltip');
                    //canvas.call(tooltip);
                    
                    var tooltip = d3.select("body").append("div").attr('class', 'portfolio-chart-tooltip');
                    
                    var arc = d3.arc()
                                .innerRadius(outerRadius - extension)
                                .padRadius(outerRadius - extension)
                                .padAngle(0.02);

                    var pie = d3.pie()
                        .value(function (d) { return d.AllocatedPercent === 0 ? 3 : d.AllocatedPercent; })
                        .sort(function (d, i) { return i; });

                    function showTooltip(plan) {
                        var html =
                            '<div class="row title">' +
                                '<div class="col-xs-12 text-center">' + plan.Title + '</div>' +
                            '</div>' +
                            '<div class="row details">' +
                                '<div class="percent">' +
                                    '<div class="col-xs-6">Allocation:</div>' +
                                    '<div class="col-xs-6 text-right">% ' + $filter('number')(plan.AllocatedPercent, 1) + '</div>' +
                                '</div>' +
                                '<div class="amount">' +
                                    '<div class="col-xs-6">Amount:</div>' +
                                    '<div class="col-xs-6 text-right">' + iso4217.getCurrencyByCode(scope.gzCurrency).symbol + ' ' + $filter('number')(plan.AllocatedAmount, 1) + '</div>' +
                                '</div>'
                            '</div>';

                        tooltip.html(html)
                            .style("background-color", d3.rgb("#27A95C").darker(plan.Gamma))
                            .style("opacity", 1);
                    };
                    function hideTooltip() {
                        tooltip.style("opacity", 0);
                    };
                    function moveTooltip() {
                        tooltip
                            .style("left", function () { return (d3.event.pageX - 90) + "px"; })
                            .style("top", function () { return (d3.event.pageY - 160) + "px"; });
                    };

                    function overTooltip(d) {
                        var el = d3.select(this);
                        el.select("path")
                            .transition()
                            .delay(duration / 4)
                            .duration(duration / 2)
                            .ease(d3.easeLinear)
                            .attrTween("d", function (d) {
                                var i = d3.interpolate(d.outerRadius, outerRadius);
                                return function (t) {
                                    return arc.outerRadius(i(t))(d);
                                };
                            });
                        el.select("text")
                            .transition()
                            .delay(duration / 4)
                            .duration(duration / 2)
                            .attr("font-size", fontSize * 1.2 + "px");
                        showTooltip(d.data);
                    }
                    function outTooltip() {
                        var el = d3.select(this);
                        el.select("path")
                            .transition()
                            .delay(duration / 4)
                            .duration(duration / 2)
                            .ease(d3.easeLinear)
                            .attrTween("d", function (d) {
                                var i = d3.interpolate(outerRadius, outerRadius - extension);
                                return function (t) {
                                    return arc.outerRadius(i(t))(d);
                                };
                            });
                        el.select("text")
                            .transition()
                            .delay(duration / 4)
                            .duration(duration / 2)
                            .attr("font-size", fontSize + "px");
                        hideTooltip();
                    }

                    var arcs =
                        group.selectAll(".arc")
                            .data(pie(plans))
                        .enter().append("g")
                            .attr("class", "arc")
                            .attr('pointer-events', 'none')
                        .each(function (d) { d.outerRadius = outerRadius - extension; })
                            .attr("d", arc)
                            .on("mouseover", overTooltip)
                            .on("mouseout", outTooltip)
                            .on("mousemove", moveTooltip);

                    arcs.transition()
                        .delay(duration * 3)
                        .duration(0)
                        .attr('pointer-events', '');

                    arcs.append("path")
                        .attr("fill", function (d, i) {
                            return d3.rgb("#27A95C").darker(d.data.Gamma);
                        })
                        .transition()
                        .delay(function (d, i) {
                            var totalPercent = d3.sum(plans.slice(0, i), function (x) {
                                return x.percent;
                            });
                            return duration * totalPercent / 100;
                        })
                        .duration(function (d, i) { return duration * d.data.AllocatedPercent / 100; })
                        .ease(d3.easeLinear)
                        .attrTween('d', function (d) {
                            var interpolateAngle = d3.interpolate(d.startAngle, d.endAngle);
                            var interpolateRadius = d3.interpolate(innerRadius, d.outerRadius);
                            return function (t) {
                                d.endAngle = interpolateAngle(t);
                                return arc.innerRadius(interpolateRadius(0)).outerRadius(interpolateRadius(t))(d);
                            }
                        });

                    arcs.append("text")
                        .attr("text-anchor", "middle")
                        .style('fill', '#fff')
                        .attr("font-size", 0 + "px")
                        .style('opacity', 0)
                        .attr('pointer-events', 'none')
                        .transition()
                        .delay(duration * 2)
                        .duration(duration)
                        .attr("font-size", fontSize + "px")
                        .style('opacity', 1)
                        .attr("transform", function (d) {
                            var c = arc.centroid(d);
                            return "translate(" + c[0] * 0.8 + "," + c[1] * 0.8 + ")";
                        })
                        .text(function (d) { return $filter('number')(d.data.AllocatedPercent, 0); });
                }
            }
        };
    }
})();