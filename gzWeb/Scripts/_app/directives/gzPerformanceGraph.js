(function () {
    'use strict';

    APP.directive('gzPerformanceGraph', ['$filter', '$location', '$interval', directiveFactory]);

    function directiveFactory($filter, $location, $interval) {
        return {
            restrict: 'EA',
            scope: {
                gzAggressiveRate: '=',
                gzModerateRate: '=',
                gzConservativeRate: '=',
                gzCurrency: '@',
            },
            templateUrl: function () { return 'partials/directives/gzPerformanceGraph.html'; },
            controller: ['$scope', '$element', '$attrs', function ($scope, $element, $attrs) {
                // #region Variables
                $scope.plans = {
                    aggressive: {
                        title: 'Aggressive',
                        returnRate: $scope.gzAggressiveRate,
                        selected: true
                    },
                    moderate: {
                        title: 'Moderate',
                        returnRate: $scope.gzModerateRate,
                        selected: false
                    },
                    conservative: {
                        title: 'Conservative',
                        returnRate: $scope.gzConservativeRate,
                        selected: false
                    }
                }
                $scope.plan = $scope.plans.aggressive;
                $scope.year = 0;
                $scope.annualContribution = 100;
                $scope.projectedValue = 0;
                $scope.profit = 0;
                var divergence = 0.2;
                $scope.principalAmount = 0;
                var totalYears = 30;
                var data = [];
                $scope.rate = $scope.plan.returnRate;
                for (var t = 0; t < totalYears; t++) {
                    data.push({
                        x: t,
                        y111: project($scope.principalAmount, $scope.rate + $scope.rate * divergence * 3, t, $scope.annualContribution),
                        y11: project($scope.principalAmount, $scope.rate + $scope.rate * divergence * 2, t, $scope.annualContribution),
                        y1: project($scope.principalAmount, $scope.rate + $scope.rate * divergence, t, $scope.annualContribution),
                        y: project($scope.principalAmount, $scope.rate, t, $scope.annualContribution),
                        y0: project($scope.principalAmount, $scope.rate - $scope.rate * divergence, t, $scope.annualContribution),
                        y00: project($scope.principalAmount, $scope.rate - $scope.rate * divergence * 2, t, $scope.annualContribution),
                        y000: project($scope.principalAmount, $scope.rate - $scope.rate * divergence * 3, t, $scope.annualContribution)
                    });
                }

                var aspect = 2;
                var margin = { top: 120, right: 20, bottom: 80, left: 100 };
                var canvas = d3.select('#canvas');
                var canvasWidth, canvasHeight, width, height;
                var now = new Date();
                var thisYear = now.getFullYear();
                var x, y, xAxis, yAxis;
                var avg, area, area2, area3;
                // #endregion

                // #region Methods
                $scope.selectPlan = function (plan) {
                    var plans = $filter('toArray')($scope.plans);
                    var index = plans.indexOf(plan);
                    for (var i = 0; i < plans.length; i++)
                        plans[i].selected = index === i;
                    $scope.plan = plans[index];
                    $scope.calculateProjection();
                }

                $scope.calculateProjection = function () {
                    //$scope.rate = $scope.plan.returnRate;
                    var projection = project($scope.principalAmount, $scope.rate, $scope.year, $scope.annualContribution);
                    $scope.projectedValue = projection.amount;
                    $scope.profit = projection.profit;
                }

                function project(principal, rate, year, annual) {
                    var pow = Math.pow(1 + rate, year);
                    var compoundInterestForPrincipal = principal * pow;
                    var futureValues = annual * ((pow - 1) / rate);
                    var totalContributions = principal + annual * year;
                    var projectedAmount = Math.round(compoundInterestForPrincipal + futureValues);
                    var projectedProfit = projectedAmount - totalContributions;
                    return {
                        amount: projectedAmount,
                        profit: projectedProfit
                    };
                }

                function defineGradient(name, opacityStop0, opacityStop50, opacityStop100) {
                    var svg = d3.select('#canvas').select("svg");

                    var gradient = svg.append("defs")
                      .append("linearGradient")
                        .attr("id", name)
                        .attr("x1", "0%")
                        .attr("y1", "0%")
                        .attr("x2", "100%")
                        .attr("y2", "0%")
                        .attr("spreadMethod", "pad");

                    gradient.append("stop")
                        .attr("offset", "0%")
                        .attr("stop-color", "#27A95C")
                        .attr("stop-opacity", opacityStop0);

                    gradient.append("stop")
                        .attr("offset", "50%")
                        .attr("stop-color", "#27A95C")
                        .attr("stop-opacity", opacityStop50);

                    gradient.append("stop")
                        .attr("offset", "100%")
                        .attr("stop-color", "#27A95C")
                        .attr("stop-opacity", opacityStop100);
                }

                function initGraph() {
                    canvasWidth = canvas.node().getBoundingClientRect().width;
                    canvasHeight = Math.round(canvasWidth / aspect);
                    width = canvasWidth - margin.left - margin.right;
                    height = canvasHeight - margin.top - margin.bottom;

                    x = d3.scale.linear()
                        //.domain(d3.extent(this.axisData.x, function (d) { return d; }))
                        .range([0, width]);
                    y = d3.scale.linear()
                        //.domain([0, d3.max(this.monthlyLineData, function (d) { return d[1]; }) << 2])
                        .range([height, 0]).nice();
                    xAxis = d3.svg.axis()
                        .scale(x)
                        .orient("bottom")
                        //.tickValues(d3.range(totalYears))
                        .tickFormat(function (d) {
                            var year = thisYear + Math.ceil(d / 6);
                            return year;
                            //if (d === 0)
                            //    return "Now";
                            //else {
                            //    var year = thisYear + Math.floor(d / 6);
                            //    //var month = ;
                            //    return year;
                            //    //var month = 
                            //    //var date = new Date();
                            //    //if (i < 4) {
                            //    //    var date = new Date();
                            //    //    date.setMonth(d.getMonth() - 1);
                            //    //    var q = Math.ceil((date.getMonth()) / 3);
                            //    //    return "Q" + q;
                            //    //} else {
                            //    //    return ().toString();
                            //    //}
                            //}

                            ////return (thisYear + Math.floor(d / 10)).toString();
                        });
                    yAxis = d3.svg.axis()
                        .scale(y)
                        .orient("left")
                        //.ticks(4)
                        .tickFormat(function (d) {
                            return $scope.gzCurrency + d3.format(",f")(d);
                        });

                    avg = d3.svg.line()
                        .interpolate("cardinal")
                        .x(function (d) { return x(d.x); })
                        .y(function (d) { return y(d.y.amount); });

                    area = d3.svg.area()
                        .interpolate("cardinal")
                        .x(function (d) { return x(d.x); })
                        .y0(function (d) { return y(d.y0.amount); })
                        .y1(function (d) { return y(d.y1.amount); });

                    area2 = d3.svg.area()
                        .interpolate("cardinal")
                        .x(function (d) { return x(d.x); })
                        .y0(function (d) { return y(d.y00.amount); })
                        .y1(function (d) { return y(d.y11.amount); });

                    area3 = d3.svg.area()
                        .interpolate("cardinal")
                        .x(function (d) { return x(d.x); })
                        .y0(function (d) { return y(d.y000.amount); })
                        .y1(function (d) { return y(d.y111.amount); });
                }

                function drawGraph () {
                    canvas.select("svg").remove();

                    var svg = d3.select('#canvas').append("svg")
                        .attr("width", width + margin.left + margin.right)
                        .attr("height", height + margin.top + margin.bottom)
                      .append("g")
                        .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

                    defineGradient("gradient1", 0.4, 0.2, 0);
                    defineGradient("gradient2", 0.3, 0.15, 0);
                    defineGradient("gradient3", 0.2, 0.1, 0);

                    x.domain(d3.extent(data, function (d) { return d.x; }));
                    y.domain(d3.extent(data, function (d) { return d.y.amount; }));

                    var xAxisLabels = svg.append("g")
                        .attr("class", "x axis")
                        .attr("transform", "translate(0," + height + ")")
                        .call(xAxis);

                    xAxisLabels.selectAll("text")
                        .attr("transform", function (d) {
                            return "translate(" + -this.getBBox().width + "," + this.getBBox().height + ")rotate(-45)";
                        });

                    var xAxisTitle = xAxisLabels.append("text")
                        .attr("class", "axis-title")
                        .attr("transform", "translate(0," + 60 + ")")
                        .style("text-anchor", "middle")
                        .text("Years");
                    xAxisTitle.attr("x", function () {
                        return width / 2 - this.getBBox().width;
                    });

                    svg.append("g")
                        .attr("class", "y axis")
                        .call(yAxis)
                    .append("text")
                        .attr("class", "axis-title")
                        .attr("transform", "rotate(-90)translate(" + -height / 2 + "," + -80 + ")")
                        .attr("y", 6)
                        .attr("dy", ".71em")
                        .style("text-anchor", "middle")
                        .text("Projected Value");

                    svg.append("path")
                        .style("fill", "url(" + $location.absUrl() + "#gradient3")
                        .attr("d", function (d) { return area3(data); });
                    svg.append("path")
                        .style("fill", "url(" + $location.absUrl() + "#gradient2")
                        .attr("d", function (d) { return area2(data); });
                    svg.append("path")
                        .style("fill", "url(" + $location.absUrl() + "#gradient1")
                        .attr("d", function (d) { return area(data); });

                    svg.append("path")
                        .datum(data)
                        .attr("class", "line")
                        .attr("d", avg(data));
                }

                function setClock() {
                    var tickInterval = 1000;
                    var tick = function() { $scope.clock = Date.now(); }
                    $interval(tick, tickInterval);
                    tick();
                }

                function init () {
                    setClock();
                    $scope.calculateProjection();
                    initGraph();
                    drawGraph();
                }

                init();
                // #endregion

                // #region Events
                d3.select(window).on("resize", function () {
                    initGraph();
                    drawGraph();
                });
                $scope.isFullscreen = false;
                $scope.toggleFullScreen = function () {
                    $scope.isFullscreen = !$scope.isFullscreen;
                }
                // #endregion
            }]
        };
    }
})();