(function () {
    'use strict';

    APP.directive('gzPortfolioChart', [directiveFactory]);

    function directiveFactory() {
        return {
            restrict: 'A',
            scope: {
                gzConservative: '=',
                gzModerate: '=',
                gzAggressive: '=',
                gzAverage: '='
            },
            link: function (scope, element, attrs) {
                var plans = [
                    { text: 'Aggressive', percent: scope.gzAggressive, color: '#227B46' },
                    { text: 'Moderate', percent: scope.gzModerate, color: '#64BF89' },
                    { text: 'Conservative', percent: scope.gzConservative, color: '#B4DCC4' },
                ];
                
                var root = d3.select(element[0]);
                var rootWidth = root.node().getBoundingClientRect().width;
                var rootHeight = root.node().getBoundingClientRect().height;
                var minDimension = Math.min(rootWidth, rootHeight) / 2;
                var externalPadding = 10;
                var donutThickness = 50;
                var outerRadius = minDimension - externalPadding;
                var extension = 5;
                var innerRadius = minDimension - externalPadding - donutThickness;
                var fontSize = 18 * outerRadius / 100;
                var duration = 400;

                var canvas =
                    root.append("svg")
                        .attr("width", minDimension * 2)
                        .attr("height", minDimension * 2);

                var group = canvas.append("g").attr("transform", "translate(" + minDimension + "," + minDimension + ")");
                var arc = d3.svg.arc()
                            .innerRadius(outerRadius - extension)
                            .padRadius(outerRadius - extension)
                            .padAngle(0.02);

                var pie = d3.layout.pie()
                    .value(function (d) { return d.percent; })
                    .sort(function (d, i) { return i; });

                var arcs =
                    group.selectAll(".arc")
                        .data(pie(plans))
                    .enter().append("g")
                        .attr("class", "arc")
                    .each(function (d) { d.outerRadius = outerRadius - extension; })
                    .attr("d", arc)
                    .on("mouseover", function () {
                        var el = d3.select(this);
                        el.select("path")
                            .transition()
                            .delay(duration / 4)
                            .duration(duration / 2)
                            .ease('linear')
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
                            .attr("font-size", fontSize*1.2 + "px");
                            //.attr("transform", function (d) {
                            //    return "translate(" + arc.centroid(d) + ")";
                            //});
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
                            //.style('opacity', 0)
                            //.attr("transform", function (d) {
                            //    var c = arc.centroid(d);
                            //    return "translate(" + 0 + "," + 0 + ")";
                            //});
                    });

                arcs.append("path")
                    .attr("fill", function (d) { return d.data.color; })
                    .transition()
                    .delay(function(d, i) {
                        var totalPercent = d3.sum(plans.slice(0, i), function(x) {
                            return x.percent;
                        });
                        return duration * totalPercent / 100;
                    })
                    .duration(function(d, i) { return duration * d.data.percent / 100; })
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
                        //return "translate(" + c + ")";
                    })
                    .text(function (d) { return d.data.percent; });

                group.append("text")
                    //.on("mouseover", function () {
                    //    d3.select(this)
                    //        .transition()
                    //        .delay(duration / 4)
                    //        .duration(duration / 2)
                    //        .attr("font-size", "42px");
                    //})
                    //.on("mouseout", function () {
                    //    d3.select(this)
                    //        .transition()
                    //        .delay(duration / 4)
                    //        .duration(duration / 2)
                    //        .attr("font-size", "38px");
                    //})
                    .style("text-anchor", "middle")
                    .attr("font-weight", "bold")
                    .style("font-size", "0px")
                    .attr("fill", "#27A95C")
                    .transition()
                    .delay(duration * 3)
                    .duration(duration * 2)
                    .ease('elastic')
                    .style("font-size", "38px")
                    .attr("dy", ".35em")
                    .text(scope.gzAverage)
                ;
            }
        };
    }
})();