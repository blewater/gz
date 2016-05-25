(function () {
    'use strict';

    APP.directive('gzPortfolioChart', [directiveFactory]);

    function directiveFactory() {
        return {
            restrict: 'A',
            scope: {
                //gzConservative: '=',
                //gzModerate: '=',
                //gzAggressive: '=',
                //gzAverage: '='
                gzPlans: '=',
                //gzRoi: '='
            },
            link: function (scope, element, attrs) {
                //var plans = [
                //    { text: scope.gzAggressive.Title, percent: scope.gzAggressive.AllocatedPercent, color: '#227B46' },
                //    { text: scope.gzModerate.Title, percent: scope.gzModerate.AllocatedPercent, color: '#64BF89' },
                //    { text: scope.gzConservative.Title, percent: scope.gzConservative.AllocatedPercent, color: '#B4DCC4' }
                //];
                var plans = scope.gzPlans;
                //var roi = scope.gzRoi;

                var root = d3.select(element[0]);
                var rootWidth = root.node().getBoundingClientRect().width;
                var rootHeight = root.node().getBoundingClientRect().height;
                var minDimension = Math.min(rootWidth, rootHeight) / 2;
                var externalPadding = 10;
                var donutThickness = 50;
                var outerRadius = minDimension - externalPadding;
                var extension = 8;
                var innerRadius = minDimension - externalPadding - donutThickness;
                var fontSize = 18 * outerRadius / 100;
                var duration = 400;

                var canvas =
                    root.append("svg")
                        .attr("width", minDimension * 2)
                        .attr("height", minDimension * 2);

                var group = canvas.append("g").attr("transform", "translate(" + minDimension + "," + minDimension + ")");

                var tooltip = d3.tip().attr('class', 'tooltip');

                canvas.call(tooltip);

                var arc = d3.svg.arc()
                            .innerRadius(outerRadius - extension)
                            .padRadius(outerRadius - extension)
                            .padAngle(0.02);

                var pie = d3.layout.pie()
                    .value(function (d) { return d.AllocatedPercent; })
                    .sort(function (d, i) { return i; });

                function showTooltip(text) {
                    tooltip.html('<span>' + text + '</span>').style("opacity", 1).show();
                };
                function hideTooltip() {
                    tooltip.hide().style("opacity", 0);
                };
                function moveTooltip() {
                    tooltip.style("left", function() { return (d3.event.pageX) + "px"; })
                           .style("top", function() { return (d3.event.pageY - 30) + "px"; });
                };

                var arcs =
                    group.selectAll(".arc")
                        .data(pie(plans))
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
                            showTooltip("% " + d.data.Title);
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
                    .attr("fill", function (d) { return d.data.Color; })
                    .transition()
                    .delay(function(d, i) {
                        var totalPercent = d3.sum(plans.slice(0, i), function(x) {
                            return x.percent;
                        });
                        return duration * totalPercent / 100;
                    })
                    .duration(function (d, i) { return duration * d.data.AllocatedPercent / 100; })
                    .ease('linear')
                    .attrTween('d', function(d) {
                        var interpolateAngle = d3.interpolate(d.startAngle, d.endAngle);
                        var interpolateRadius = d3.interpolate(innerRadius, d.outerRadius);
                        return function(t) {
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
                        return "translate(" + c[0]*0.8 + "," + c[1]*0.8 + ")";
                    })
                    .text(function (d) { return d.data.AllocatedPercent; });

                //group.append("text")
                //    .style("text-anchor", "middle")
                //    .attr("font-weight", "bold")
                //    .style("font-size", "0px")
                //    .attr("fill", "#27A95C")
                //    .on("mouseover", function () {
                //        showTooltip(roi.Title);
                //        d3.select(this)
                //            .transition()
                //            .delay(duration / 4)
                //            .duration(duration / 2)
                //            .style("font-size", "45px")
                //            .attr("dy", ".38em");
                //    })
                //    .on("mouseout", function () {
                //        hideTooltip();
                //        d3.select(this)
                //            .transition()
                //            .delay(duration / 4)
                //            .duration(duration / 2)
                //            .style("font-size", "38px")
                //            .attr("dy", ".35em");
                //    })
                //    .on("mousemove", moveTooltip)
                //    .transition()
                //    .delay(duration * 3)
                //    .duration(duration * 2)
                //    .ease('elastic')
                //    .style("font-size", "38px")
                //    .attr("dy", ".35em")
                //    .text(roi.Percent)
                //;
            }
        };
    }
})();