(function () {
    'use strict';

    APP.directive('gzPortfolioChart', ['iso4217', '$filter', directiveFactory]);

    function directiveFactory(iso4217, $filter) {
        return {
            restrict: 'A',
            scope: {
                gzPlan: '=',
                gzCurrency: '@',
            },
            link: function (scope, element, attrs) {
                scope.$watch('gzPlan', function (newValue, oldValue) {
                    drawDonut();
                });

                function drawDonut () {
                    var plan = scope.gzPlan;
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

                    var canvas =
                        root.append("svg")
                            .attr("width", minDimension * 2)
                            .attr("height", minDimension * 2);

                    var group = canvas.append("g").attr("transform", "translate(" + minDimension + "," + minDimension + ")");

                    var tooltip = d3.tip().attr('class', 'holding-info');//.direction('n');

                    canvas.call(tooltip);

                    var arc = d3.svg.arc()
                                .innerRadius(outerRadius - extension)
                                .padRadius(outerRadius - extension)
                                .padAngle(0.02);

                    var pie = d3.layout.pie()
                        .value(function (d) { return d.Weight === 0 ? 3 : d.Weight; })
                        .sort(function (d, i) { return i; });

                    function showTooltip(holding) {
                        var name = '<div class="row"><div class="col-xs-6">Holding Name: </div><div class="col-xs-6 text-right">' + holding.Name + '</div></div>';
                        var weight = '<div class="row"><div class="col-xs-6">Portfolio Weight: </div><div class="col-xs-6 text-right">% ' + $filter('number')(holding.Weight, 2) + '</div></div>';
                        var value = '<div class="row"><div class="col-xs-6">Current Value: </div><div class="col-xs-6 text-right">' + iso4217.getCurrencyByCode(scope.gzCurrency).symbol + ' ' + $filter('number')(plan.AllocatedAmount * holding.Weight / 100, 2) + '</div></div>';
                        var html = name + weight + value;
                        tooltip.html(html)
                            .style("background-color", d3.rgb("#27A95C").darker(holding.Weight / 10))
                            .style("opacity", 1)
                            .show();
                    };
                    function hideTooltip() {
                        tooltip.hide().style("opacity", 0);
                    };
                    function moveTooltip() {
                        tooltip
                            .style("left", function () { return (d3.event.pageX - 110) + "px"; })
                            .style("top", function () { return (d3.event.pageY - 80) + "px"; });
                    };

                    var arcs =
                        group.selectAll(".arc")
                            .data(pie(plan.Holdings))
                        .enter().append("g")
                            .attr("class", "arc")
                            .attr('pointer-events', 'none')
                        .each(function (d) { d.outerRadius = outerRadius - extension; })
                            .attr("d", arc)
                            .on("mouseover", function (d) {
                                var el = d3.select(this);
                                el.select("path")
                                    .transition()
                                    .delay(duration / 4)
                                    .duration(duration / 2)
                                    .ease('linear')
                                    .attrTween("d", function (_d) {
                                        var i = d3.interpolate(_d.outerRadius, outerRadius);
                                        return function (t) {
                                            return arc.outerRadius(i(t))(_d);
                                        };
                                    });
                                el.select("text")
                                    .transition()
                                    .delay(duration / 4)
                                    .duration(duration / 2)
                                    .attr("font-size", fontSize * 1.2 + "px");
                                //showHoldingInfo(d.data, "#27A95C");
                                showTooltip(d.data);
                            })
                            .on("mouseout", function () {
                                var el = d3.select(this);
                                el.select("path")
                                    .transition()
                                    .delay(duration / 4)
                                    .duration(duration / 2)
                                    .ease('linear')
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
                            })
                            .on("mousemove", moveTooltip);

                    arcs.transition()
                        .delay(duration * 3)
                        .duration(0)
                        .attr('pointer-events', '');

                    arcs.append("path")
                        .attr("fill", function (d, i) {
                            return d3.rgb("#27A95C").darker(d.data.Weight / 10);
                        })
                        .transition()
                        .delay(function (d, i) {
                            var totalPercent = d3.sum(plan.Holdings.slice(0, i), function (x) {
                                return x.percent;
                            });
                            return duration * totalPercent / 100;
                        })
                        .duration(function (d, i) { return duration * d.data.Weight / 100; })
                        .ease('linear')
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
                        .transition()
                        .delay(duration * 2)
                        .duration(duration)
                        .attr("font-size", fontSize + "px")
                        .style('opacity', 1)
                        .attr("transform", function (d) {
                            var c = arc.centroid(d);
                            return "translate(" + c[0] * 0.8 + "," + c[1] * 0.8 + ")";
                        })
                        .text(function (d) { return d.data.Weight; });

                    //var holdingWeight = group.append("text").style("text-anchor", "middle").attr("font-weight", "bold").style("font-size", "0px").attr("dy", "-15px");
                    //var holdingName = group.append("text").style("text-anchor", "middle").attr("font-weight", "bold").style("font-size", "0px").attr("dy", "0.35em");
                    //var holdingValue = group.append("text").style("text-anchor", "middle").attr("font-weight", "bold").style("font-size", "0px").attr("dy", "25px");
                    //function showHoldingElement(el, text, color) {
                    //    el.attr("fill", color)
                    //        .transition()
                    //        .duration(duration)
                    //        .ease('elastic')
                    //        .style("font-size", "14px")
                    //        .text(text);
                    //}
                    //function hideHoldingElement(el) {
                    //    el.transition()
                    //        .duration(duration)
                    //        .ease('elastic')
                    //        .style("font-size", "0");
                    //}
                    //function showHoldingInfo(holding, color) {
                    //    showHoldingElement(holdingName, holding.Name, color);
                    //    showHoldingElement(holdingWeight, "% " + holding.Weight + ".00", color);
                    //    showHoldingElement(holdingValue, "€ " +plan.AllocatedAmount * holding.Weight / 100, color);
                    //}
                    //function hideHoldingInfo() {
                    //    hideHoldingElement(holdingName);
                    //    hideHoldingElement(holdingWeight);
                    //    hideHoldingElement(holdingValue);
                    //}
                }
            }
        };
    }
})();