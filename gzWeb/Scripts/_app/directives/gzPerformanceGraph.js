(function () {
    'use strict';

    APP.directive('gzPerformanceGraph', ['$rootScope', '$filter', '$location', '$interval', '$timeout', 'helpers', 'iso4217', directiveFactory]);

    function directiveFactory($rootScope, $filter, $location, $interval, $timeout, helpers, iso4217) {
        return {
            restrict: 'EA',
            scope: {
                gzPlans: '=',
                gzCurrency: '@',
            },
            templateUrl: function() {
                return helpers.ui.getTemplate('partials/directives/gzPerformanceGraph.html');
            },
            controller: ['$scope', '$element', '$attrs', function ($scope, $element, $attrs) {
                // #region Variables
                $scope.plans = $scope.gzPlans;
                $scope.plan = $filter('filter')($scope.plans, { Selected: true })[0];
                $scope.year = 10;
                $scope.annualContribution = 100;
                $scope.projectedValue = 0;
                $scope.profit = 0;
                $scope.principalAmount = 100;
                var duration = 300;
                var divergence = 0.2;
                var data;
                var totalYears = 30;
                var aspect = 1.5;
                var canvas = d3.select('#canvas');
                var canvasWidth, canvasHeight, width, height, margin, xAxisPosition, yAxisPosition;
                var now = new Date();
                var thisYear = now.getFullYear();
                var x, y, xAxis, yAxis;
                var xAxisLabels, xAxisTitle, yAxisLabels, yAxisTitle;
                var avg, area, area2, area3;
                var avgElement, areaElement, area2Element, area3Element;
                var svg, graphRect, graphRectWidth, graphRectHeight, handler, xProjection, yProjection;
                var handlerPosition = { x: 0, y: 0 };
                var handlerAnimationStates = {
                    dragging: "dragging",
                    rescaling: "rescaling",
                    resizing: "resizing"
                }
                // #endregion

                // #region Methods
                function computeData() {
                    data = [];
                    for (var t = 0; t < totalYears; t++) {
                        data.push({
                            x: t,
                            y111: project($scope.principalAmount, $scope.plan.ROI + $scope.plan.ROI * divergence * 3, t, $scope.annualContribution),
                            y11: project($scope.principalAmount, $scope.plan.ROI + $scope.plan.ROI * divergence * 2, t, $scope.annualContribution),
                            y1: project($scope.principalAmount, $scope.plan.ROI + $scope.plan.ROI * divergence, t, $scope.annualContribution),
                            y: project($scope.principalAmount, $scope.plan.ROI, t, $scope.annualContribution),
                            y0: project($scope.principalAmount, $scope.plan.ROI - $scope.plan.ROI * divergence, t, $scope.annualContribution),
                            y00: project($scope.principalAmount, $scope.plan.ROI - $scope.plan.ROI * divergence * 2, t, $scope.annualContribution),
                            y000: project($scope.principalAmount, $scope.plan.ROI - $scope.plan.ROI * divergence * 3, t, $scope.annualContribution)
                        });
                    }
                    x.domain(d3.extent(data, function (d) { return d.x; }));
                    y.domain(d3.extent(data, function (d) { return d.y.amount; }));
                }
                $scope.getYear = function() {
                    return Math.ceil($scope.year);
                }
                $scope.selectPlan = function (plan) {
                    var index = $scope.plans.indexOf(plan);
                    for (var i = 0; i < $scope.plans.length; i++)
                        $scope.plans[i].Selected = index === i;
                    $scope.plan = $scope.plans[index];
                    calculateProjection(handlerAnimationStates.rescaling);
                }

                function getHandlerTransitionDuration(handlerAnimationState) {
                    switch (handlerAnimationState) {
                        case handlerAnimationStates.dragging:
                            return duration / 4;
                        case handlerAnimationStates.rescaling:
                            return duration;
                        case handlerAnimationStates.resizing:
                            return 0;
                        default:
                            return duration; 
                    }
                }

                function calculateProjection(handlerAnimationState) {
                    $timeout(function () {
                        computeData();

                        var projection = project($scope.principalAmount, $scope.plan.ROI, $scope.year, $scope.annualContribution);
                        $scope.projectedValue = projection.amount;
                        $scope.profit = projection.profit;

                        var x_ = x($scope.year);
                        var y_ = y(projection.amount);
                        
                        handlerPosition.x = x_;
                        handlerPosition.y = y_;

                        var handlerTransitionDuration = getHandlerTransitionDuration(handlerAnimationState);
                        handler.transition().duration(handlerTransitionDuration).attr("transform", function () {
                            return "translate(" + x_ + "," + y_ + ")";
                        });

                        //var xProjectionData = "M " + x_ + " " + y_ + " L " + 0 + " " + y_;
                        //var yProjectionData = "M " + x_ + " " + y_ + " L " + x_ + " " + height;
                        var xProjectionData = "M " + width + " " + y_ + " L " + 0 + " " + y_;
                        var yProjectionData = "M " + x_ + " " + -margin.top + " L " + x_ + " " + height;
                        xProjection.transition().duration(duration / 4).attr("d", xProjectionData);
                        yProjection.transition().duration(duration / 4).attr("d", yProjectionData);

                        area3Element.transition().duration(duration).attr("d", area3(data));
                        area2Element.transition().duration(duration).attr("d", area2(data));
                        areaElement.transition().duration(duration).attr("d", area(data));
                        avgElement.transition().duration(duration).attr("d", avg(data));
                        //xAxisLabels.call(xAxis);
                        yAxisLabels.transition().duration(duration).call(yAxis);
                    }, 0);
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

                function defineGradient(svgElement, name, opacityStop0, opacityStop10, opacityStop100) {
                    var gradient = svgElement.append("defs")
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
                        .attr("offset", "10%")
                        .attr("stop-color", "#27A95C")
                        .attr("stop-opacity", opacityStop10);

                    gradient.append("stop")
                        .attr("offset", "100%")
                        .attr("stop-color", "#27A95C")
                        .attr("stop-opacity", opacityStop100);
                }

                function initGraph() {
                    if ($rootScope.xs) {
                        canvasHeight = 360;
                        canvasWidth = canvas.node().getBoundingClientRect().width;
                    } else {
                        canvasWidth = canvas.node().getBoundingClientRect().width;
                        canvasHeight = Math.round(canvasWidth / aspect);
                    }
                    margin = {
                        top: (function () { return $rootScope.xs ? 80 : 120; })(),
                        right: 20,
                        bottom: (function () { return $rootScope.xs ? 60 : 80; })(),
                        left: (function () { return $rootScope.xs ? 70 : 100; })()
                    };

                    width = canvasWidth - margin.left - margin.right;
                    height = canvasHeight - margin.top - margin.bottom;
                    graphRectWidth = width;
                    graphRectHeight = canvasHeight - margin.bottom;
                    xAxisPosition = { x: 0, y: margin.bottom - 20 };
                    yAxisPosition = { x: -height / 2, y: -(margin.left - 20) };
                    //xAxisPosition = { x: 0, y: 60 };
                    //yAxisPosition = { x: -height / 2, y: -80 };


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
                            var year = thisYear + d;
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
                            return iso4217.getCurrencyByCode($scope.gzCurrency).symbol + d3.format(",f")(d);
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

                function drawGraph() {
                    canvas.select("svg").remove();

                    svg = d3.select('#canvas').append("svg")
                        .attr("width", canvasWidth)
                        .attr("height", canvasHeight)
                      .append("g")
                        .attr("transform", function() {
                            return "translate(" + margin.left + "," + margin.top + ")";
                        });

                    defineGradient(svg, "gradient1", 0.4, 0.2, 0);
                    defineGradient(svg, "gradient2", 0.3, 0.15, 0);
                    defineGradient(svg, "gradient3", 0.2, 0.1, 0);

                    x.domain(d3.extent(data, function (d) { return d.x + 1; }));
                    y.domain(d3.extent(data, function (d) { return d.y.amount; }));

                    xAxisLabels = svg.append("g")
                        .attr("class", "x axis")
                        .attr("transform", "translate(0," + height + ")")
                        .call(xAxis);
                    xAxisLabels.selectAll("text")
                        .attr("transform", function (d) {
                            return "translate(" + -this.getBBox().width + "," + this.getBBox().height + ")rotate(-45)";
                        });
                    xAxisTitle = xAxisLabels.append("text")
                        .attr("class", "axis-title")
                        .attr("transform", "translate(" + xAxisPosition.x + "," + xAxisPosition.y + ")")
                        .style("text-anchor", "middle")
                        .text("Years");
                    xAxisTitle.attr("x", function () {
                        return width / 2 - this.getBBox().width;
                    });

                    yAxisLabels = svg.append("g")
                        .attr("class", "y axis")
                        .call(yAxis);
                    yAxisTitle = yAxisLabels.append("text")
                        .attr("class", "axis-title")
                        .attr("transform", "rotate(-90)translate(" + yAxisPosition.x + "," + yAxisPosition.y + ")")
                        .attr("y", 6)
                        .attr("dy", ".71em")
                        .style("text-anchor", "middle")
                        .text("Projected Value");


                    area3Element = svg.append("path")
                        .style("fill", "url(" + $location.absUrl() + "#gradient3")
                        .attr("d", function (d) { return area3(data); });
                    area2Element = svg.append("path")
                        .style("fill", "url(" + $location.absUrl() + "#gradient2")
                        .attr("d", function (d) { return area2(data); });
                    areaElement = svg.append("path")
                        .style("fill", "url(" + $location.absUrl() + "#gradient1")
                        .attr("d", function (d) { return area(data); });

                    avgElement = svg.append("path")
                        .datum(data)
                        .attr("class", "line")
                        .attr("d", avg(data));


                    graphRect = svg.append('rect')
                        .style("fill", "transparent")
                        .attr("x", 0)
                        .attr("y", 0)
                        .attr("width", graphRectWidth)
                        .attr("height", graphRectHeight)
                        .attr("transform", function () {
                            return "translate(" + 0 + "," + -margin.top + ")";
                        });

                    xProjection = svg.append("path")
                        .attr("id", 'x-projection')
                        .attr("class", 'projection');
                    yProjection = svg.append("path")
                        .attr("id", 'y-projection')
                        .attr("class", 'projection');

                    handler = svg.append("g")
                        .attr("class", "handler")
                        .attr("transform", function () {
                            return "translate(" + handlerPosition.x + "," + handlerPosition.y + ")";
                        });

                    handler.append("circle")
                        .attr("r", 10)
                        .style("fill", "#27A95C");
                    handler.append("circle")
                        .attr("r", 4)
                        .style("fill", "#fff");

                    handler.append("path")
                        .attr("d", d3.svg.symbol().type("triangle-up").size(function () { return 25; }))
                        .attr("transform", "translate(0, -20)")
                        .attr("class", "triangle");
                    handler.append("path")
                        .attr("d", d3.svg.symbol().type("triangle-up").size(function () { return 25; }))
                        .attr("transform", "translate(20, 0)rotate(90)")
                        .attr("class", "triangle");
                    handler.append("path")
                        .attr("d", d3.svg.symbol().type("triangle-up").size(function () { return 25; }))
                        .attr("transform", "translate(-20, 0)rotate(-90)")
                        .attr("class", "triangle");
                    handler.append("path")
                        .attr("d", d3.svg.symbol().type("triangle-down").size(function () { return 25; }))
                        .attr("transform", "translate(0, 20)")
                        .attr("class", "triangle");

                    handler.on("mousedown", function () {
                        var startTime = new Date().getTime();
                        var initialMousePosition = d3.mouse(graphRect.node());
                        var initialAnnualContribution = $scope.annualContribution;
                        var triangles = d3.selectAll(".triangle").classed("active", true);
                        var projections = d3.selectAll(".projection").classed("active", true);
                        var r = graphRect.on("mousemove", mousemove).on("mouseup", mouseup);//.on("mouseout", mouseRectOut);
                        triangles.on("mouseup", mouseup);//.on("mouseout", mouseTrianglesOut);
                        projections.on("mouseup", mouseup);//.on("mouseout", mouseProjectionsOut);
                        //var rectOut = false;
                        //var trianglesOut = false;
                        //var projectionsOut = false;
                        d3.event.preventDefault();

                        //var recalcTimer;

                        function calcNewAnnualContribution(initialY, mouseY) {
                            var newAnnualContribution;

                            if (initialY < mouseY) {
                                //if (angular.isDefined(recalcTimer))
                                //     $timeout.cancel(recalcTimer);
                                var belowDiff = mouseY - initialY;
                                var belowWhole = graphRectHeight - initialY;
                                var belowPercent = belowDiff / belowWhole;
                                newAnnualContribution = (1 - belowPercent) * initialAnnualContribution;
                                if (newAnnualContribution < 0)
                                    newAnnualContribution = 0;
                            }
                            else {
                                //recalcTimer = $timeout(function () {
                                //    $scope.annualContribution = calcNewAnnualContribution(initialY, mouseY);
                                //    calculateProjection(handlerAnimationStates.dragging);
                                //}, 2000);
                                var currentTime = new Date().getTime();
                                var timeDiff = currentTime - startTime;
                                var factor = timeDiff <= 3000 ? 3 : Math.ceil(timeDiff / 1000);
                                console.log(factor);
                                var aboveDiff = initialY - mouseY;
                                var aboveWhole = initialY;
                                var aboveMax = initialAnnualContribution * factor;
                                var abovePercent = aboveDiff / aboveWhole;
                                newAnnualContribution = initialAnnualContribution + abovePercent * aboveMax;
                            }

                            return newAnnualContribution;
                        }

                        function mousemove() {
                            var mousePosition = d3.mouse(graphRect.node());
                            $scope.year = x.invert(mousePosition[0]);
                            $scope.annualContribution = calcNewAnnualContribution(initialMousePosition[1], mousePosition[1]);
                            calculateProjection(handlerAnimationStates.dragging);
                        }

                        function mouseup() {
                            triangles.classed("active", false);
                            projections.classed("active", false);
                            r.on("mousemove", null).on("mouseup", null);//.on("mouseout", null);
                            triangles.on("mouseup", null);//.on("mouseout", null);
                            projections.on("mouseup", null);//.on("mouseout", null);

                            //if (angular.isDefined(recalcTimer))
                            //    $timeout.cancel(recalcTimer);
                        }

                        //function mouseRectOut() {
                        //    rectOut = true;
                        //    mouseout();
                        //}
                        //function mouseTrianglesOut() {
                        //    trianglesOut = true;
                        //    mouseout();
                        //}
                        //function mouseProjectionsOut() {
                        //    projectionsOut = true;
                        //    mouseout();
                        //}
                        //function mouseout() {
                        //    if (rectOut && trianglesOut && projectionsOut)
                        //        mouseup();
                        //}
                    });
                }

                function init() {
                    initGraph();
                    computeData();
                    drawGraph();
                    calculateProjection(handlerAnimationStates.resizing);
                }

                init();
                // #endregion

                // #region Events
                d3.select(window).on("resize", function () {
                    init();
                });
                // #endregion

                //function setClock() {
                //    var tickInterval = 1000;
                //    var tick = function () { $scope.clock = Date.now(); }
                //    $interval(tick, tickInterval);
                //    tick();
                //}
                //$scope.isFullscreen = false;
                //$scope.toggleFullScreen = function () {
                //    $scope.isFullscreen = !$scope.isFullscreen;
                //}
            }]
        };
    }
})();