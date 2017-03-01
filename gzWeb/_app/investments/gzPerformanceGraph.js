(function () {
    'use strict';

    APP.directive('gzPerformanceGraph', ['$rootScope', '$filter', '$location', '$timeout', 'helpers', 'iso4217', directiveFactory]);

    function directiveFactory($rootScope, $filter, $location, $timeout, helpers, iso4217) {
        return {
            restrict: 'EA',
            scope: {
                gzPlans: '=',
                gzMonthlyContribution: '=',
                gzCurrency: '@'
            },
            templateUrl: function() {
                return helpers.ui.getTemplate('_app/investments/gzPerformanceGraph.html');
            },
            controller: ['$scope', '$element', '$attrs', function ($scope, $element, $attrs) {
                // #region Variables
                $scope.plans = $scope.gzPlans;
                for (var j = 0; j < $scope.plans.length; j++)
                    $scope.plans[j].returnRate = Math.round($scope.plans[j].ROI * 100) / 10000;
                $scope.plan = $filter('filter')($scope.plans, { Selected: true })[0];
                $scope.year = 5;
                $scope.monthlyContribution = $scope.gzMonthlyContribution;
                $scope.projectedValue = 0;
                $scope.profit = 0;
                //$scope.principalAmount = $scope.gzPrincipalAmount > 0 ? $scope.gzPrincipalAmount : $scope.annualContribution;
                var duration = 300;
                var divergence = 0.2;
                var data;
                var totalYears = 10;
                var aspect = 2;
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
                    for (var t = 0; t <= totalYears; t++) {
                        data.push({
                            x: t,
                            y111: project($scope.plan.returnRate + $scope.plan.returnRate * divergence * 3, t, $scope.monthlyContribution),
                            y11: project($scope.plan.returnRate + $scope.plan.returnRate * divergence * 2, t, $scope.monthlyContribution),
                            y1: project($scope.plan.returnRate + $scope.plan.returnRate * divergence, t, $scope.monthlyContribution),
                            y: project($scope.plan.returnRate, t, $scope.monthlyContribution),
                            y0: project($scope.plan.returnRate - $scope.plan.returnRate * divergence, t, $scope.monthlyContribution),
                            y00: project($scope.plan.returnRate - $scope.plan.returnRate * divergence * 2, t, $scope.monthlyContribution),
                            y000: project($scope.plan.returnRate - $scope.plan.returnRate * divergence * 3, t, $scope.monthlyContribution)
                        });
                    }
                    x.domain(d3.extent(data, function (d) { return d.x; }));
                    y.domain(d3.extent(data, function (d) { return d.y.amount; }));
                }
                $scope.getYear = function() {
                    return Math.round($scope.year * 2) / 2;
                }
                $scope.getProjectedValue = function() {
                    return Math.round($scope.projectedValue);
                }
                $scope.getProfit = function() {
                    return Math.round($scope.profit);
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

                        var projection = project($scope.plan.returnRate, $scope.year, $scope.monthlyContribution);
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

                function project(rate, year, monthly) {
                    //var pow = Math.pow(1 + rate, year);
                    //var compoundInterestForPrincipal = principal * pow;
                    //var futureValues = annual * ((pow - 1) / rate);
                    //var totalContributions = principal + annual * year;
                    //var projectedAmount = compoundInterestForPrincipal + futureValues;
                    //var projectedProfit = projectedAmount - totalContributions;
                    var r = rate / 12;
                    var P = monthly;
                    var n = year * 12;
                    var projectedAmount = (1 + r) * P * ((Math.pow(1 + r, n) - 1) / r);
                    var projectedProfit = projectedAmount - (year * 12 * monthly);

                    return {
                        amount: projectedAmount,
                        profit: projectedProfit
                    };
                }

                function defineGradient(svgElement, name, opacityStart, opacityMiddle, opacityEnd) {
                    var gradient = svgElement.append("defs")
                      .append("linearGradient")
                        .attr("id", name)
                        .attr("x1", "0%")
                        .attr("y1", "0%")
                        .attr("x2", "100%")
                        .attr("y2", "0%")
                        .attr("spreadMethod", "pad")
                        .attr("gradientUnits", "userSpaceOnUse");

                    gradient.append("stop")
                        .attr("stop-opacity", opacityStart)
                        .attr("stop-color", "#27A95C")
                        .attr("offset", "0%");

                    gradient.append("stop")
                        .attr("stop-opacity", opacityMiddle)
                        .attr("stop-color", "#27A95C")
                        .attr("offset", "30%");

                    gradient.append("stop")
                        .attr("stop-opacity", opacityEnd)
                        .attr("stop-color", "#27A95C")
                        .attr("offset", "100%");
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


                    x = d3.scaleLinear()
                        //.domain(d3.extent(this.axisData.x, function (d) { return d; }))
                        .range([0, width]);
                    y = d3.scaleLinear()
                        //.domain([0, d3.max(this.monthlyLineData, function (d) { return d[1]; }) << 2])
                        .range([height, 0]).nice();
                    xAxis = d3.axisBottom(x)
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
                    yAxis = d3.axisLeft(y)
                        //.ticks(4)
                        .tickFormat(function (d) {
                            return iso4217.getCurrencyByCode($scope.gzCurrency).symbol + $filter('number')(d, 0);// d3.format(",f")(d);
                        });

                    avg = d3.line()
                        .curve(d3.curveCardinal)
                        .x(function (d) { return x(d.x); })
                        .y(function (d) { return y(d.y.amount); });

                    area = d3.area()
                        .curve(d3.curveCardinal)
                        .x(function (d) { return x(d.x); })
                        .y0(function (d) { return y(d.y0.amount); })
                        .y1(function (d) { return y(d.y1.amount); });

                    area2 = d3.area()
                        .curve(d3.curveCardinal)
                        .x(function (d) { return x(d.x); })
                        .y0(function (d) { return y(d.y00.amount); })
                        .y1(function (d) { return y(d.y11.amount); });

                    area3 = d3.area()
                        .curve(d3.curveCardinal)
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

                    x.domain(d3.extent(data, function (d) { return d.x; }));
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
                        .attr("style", "fill:url(" + $location.absUrl() + "#gradient3")
                        .attr("d", area3(data));
                    area2Element = svg.append("path")
                        .attr("style", "fill:url(" + $location.absUrl() + "#gradient2")
                        .attr("d", area2(data));
                    areaElement = svg.append("path")
                        .attr("style", "fill:url(" + $location.absUrl() + "#gradient1")
                        .attr("d", area(data));

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
                        })
                        .on("touchstart", function () {
                            var startTime = new Date().getTime();
                            var initialTouchPosition = d3.touches(this)[0];

                            projections.classed("active", true);
                            graphRect.on("touchmove", onTouchMove);
                            d3.event.preventDefault();
                            d3.event.stopPropagation();

                            function calcNewMonthlyContribution(initialY, currentY) {
                                var newMonthlyContribution;
                                initialY = graphRectHeight / 2;
                                if (initialY < currentY) {
                                    var belowDiff = currentY - initialY;
                                    var belowWhole = graphRectHeight - initialY;
                                    var belowPercent = belowDiff / belowWhole;
                                    newMonthlyContribution = (1 - belowPercent) * $scope.monthlyContribution;
                                    if (newMonthlyContribution < 0)
                                        newMonthlyContribution = 0;
                                }
                                else {
                                    var currentTime = new Date().getTime();
                                    var timeDiff = currentTime - startTime;
                                    var factor = timeDiff <= 3000 ? 3 : Math.ceil(timeDiff / 1000);
                                    var aboveDiff = initialY - currentY;
                                    var aboveWhole = initialY;
                                    var aboveMax = $scope.monthlyContribution * factor;
                                    var abovePercent = aboveDiff / aboveWhole;
                                    newMonthlyContribution = $scope.monthlyContribution + abovePercent * aboveMax;
                                }
                                return newMonthlyContribution;
                            }

                            function onTouchMove() {
                                playLoop = false;
                                triangles.transition().duration(400).attr("d", triangleSymbol.size(function () { return 64; }));
                                var touchPosition = d3.touches(this)[0];
                                var xDiff = Math.abs(initialTouchPosition[0] - touchPosition[0]);
                                var yDiff = Math.abs(initialTouchPosition[1] - touchPosition[1]);
                                if (xDiff >= yDiff) {
                                    playLoop = false;
                                    triangles.transition().duration(400).attr("d", triangleSymbol.size(function () { return 64; }));
                                    trianglesVertical.classed('inactive', true);
                                    $scope.year = x.invert(touchPosition[0]);
                                    calculateProjection(handlerAnimationStates.dragging);
                                    graphRect.on("mouseup", function () {
                                        playLoop = true;
                                        trianglesVertical.classed('inactive', false);
                                        projections.classed("active", false);
                                        graphRect.on("touchmove", null).on("mouseup", null);
                                        triangles.transition().duration(400).attr("d", triangleSymbol.size(function () { return 25; }));
                                    });
                                }
                                else {
                                    trianglesHorizontal.classed('inactive', true);
                                    $scope.monthlyContribution = calcNewMonthlyContribution(initialTouchPosition[1], touchPosition[1]);
                                    calculateProjection(handlerAnimationStates.dragging);
                                    graphRect.on("mouseup", function () {
                                        playLoop = true;
                                        trianglesHorizontal.classed('inactive', false);
                                        projections.classed("active", false);
                                        graphRect.on("touchmove", null).on("mouseup", null);
                                        triangles.transition().duration(400).attr("d", triangleSymbol.size(function () { return 25; }));
                                    });
                                }
                            }
                        });

                    xProjection = svg.append("path")
                        .attr("id", 'x-projection')
                        .attr("class", 'projection');
                    yProjection = svg.append("path")
                        .attr("id", 'y-projection')
                        .attr("class", 'projection');

                    handler = svg.append("g")
                        .attr("class", "handler")
                        .attr("width", 60)
                        .attr("height", 60)
                        .attr("transform", function () {
                            return "translate(" + handlerPosition.x + "," + handlerPosition.y + ")";
                        });

                    handler.append("circle")
                        .attr("r", 10)
                        .style("fill", function () { return $scope.plan.Color; });
                    handler.append("circle")
                        .attr("r", 4)
                        .style("fill", "#fff");

                    handler.append('rect')
                        .attr("class", "handlerRect")
                        .style("fill", "transparent")
                        .attr("x", -30)
                        .attr("y", -30)
                        .attr("width", 60)
                        .attr("height", 60);


                    var playLoop = true;
                    var loopDuration = 800;
                    function loop(el, transformTo, transformFrom) {
                        (function repeat() {
                            el.transition()
                                .duration(loopDuration)
                                .attr("transform", transformTo)
                                .on("end", function () {
                                    if (playLoop)
                                        el.transition()
                                            .duration(loopDuration)
                                            .attr("transform", transformFrom)
                                            .on("end", repeat);
                                });
                        })();
                    }
                    function startLoops() {
                        loop(triangleTop, "translate(0, -25)", "translate(0, -20)");
                        loop(triangleRight, "translate(25, 0)rotate(90)", "translate(20, 0)rotate(90)");
                        loop(triangleLeft, "translate(-25, 0)rotate(-90)", "translate(-20, 0)rotate(-90)");
                        loop(triangleBottom, "translate(0, 25)rotate(180)", "translate(0, 20)rotate(180)");
                    }

                    var horizontal, vertical;
                    var triangleSymbol = d3.symbol().type(d3.symbolTriangle);
                    var triangleTop = handler.append("path")
                        .attr("d", triangleSymbol.size(function () { return 25; }))
                        .attr("transform", "translate(0, -20)")
                        .attr("class", "triangle triangle-top triangle-vertical")
                        .style("stroke", function () { return $scope.plan.Color; });
                    var triangleRight = handler.append("path")
                        .attr("d", triangleSymbol.size(function () { return 25; }))
                        .attr("transform", "translate(20, 0)rotate(90)")
                        .attr("class", "triangle triangle-right triangle-horizontal")
                        .style("stroke", function () { return $scope.plan.Color; });
                    var triangleLeft = handler.append("path")
                        .attr("d", triangleSymbol.size(function () { return 25; }))
                        .attr("transform", "translate(-20, 0)rotate(-90)")
                        .attr("class", "triangle triangle-left triangle-horizontal")
                        .style("stroke", function () { return $scope.plan.Color; });
                    var triangleBottom = handler.append("path")
                        .attr("d", triangleSymbol.size(function () { return 25; }))
                        .attr("transform", "translate(0, 20)rotate(180)")
                        .attr("class", "triangle triangle-bottom triangle-vertical")
                        .style("stroke", function () { return $scope.plan.Color; });

                    startLoops();
                    
                    var trianglesVertical = d3.selectAll(".triangle-vertical");
                    var trianglesHorizontal = d3.selectAll(".triangle-horizontal");
                    var triangles = d3.selectAll(".triangle");
                    var projections = d3.selectAll(".projection");

                    function onVerticalMouseDown() {
                        var startTime = new Date().getTime();
                        var initialMousePosition = d3.mouse(graphRect.node());
                        var initialMonthlyContribution = $scope.monthlyContribution;

                        projections.classed("active", true);
                        graphRect.on("mousemove", onVerticalMouseMove).on("mouseup", onVerticalMouseUp);
                        triangles.on("mouseup", onVerticalMouseUp);
                        projections.on("mouseup", onVerticalMouseUp);
                        d3.event.preventDefault();

                        function calcNewMonthlyContribution(initialY, mouseY) {
                            var newMonthlyContribution;
                            initialY = graphRectHeight / 2;
                            if (initialY < mouseY) {
                                var belowDiff = mouseY - initialY;
                                var belowWhole = graphRectHeight - initialY;
                                var belowPercent = belowDiff / belowWhole;
                                newMonthlyContribution = (1 - belowPercent) * initialMonthlyContribution;
                                if (newMonthlyContribution < 0)
                                    newMonthlyContribution = 0;
                            }
                            else {
                                var currentTime = new Date().getTime();
                                var timeDiff = currentTime - startTime;
                                var factor = timeDiff <= 3000 ? 3 : Math.ceil(timeDiff / 1000);
                                var aboveDiff = initialY - mouseY;
                                var aboveWhole = initialY;
                                var aboveMax = initialMonthlyContribution * factor;
                                var abovePercent = aboveDiff / aboveWhole;
                                newMonthlyContribution = initialMonthlyContribution + abovePercent * aboveMax;
                            }
                            return newMonthlyContribution;
                        }

                        function onVerticalMouseMove() {
                            playLoop = false;
                            triangles.transition().duration(400).attr("d", triangleSymbol.size(function () { return 64; }));
                            trianglesHorizontal.classed('inactive', true);
                            var mousePosition = d3.mouse(graphRect.node());
                            $scope.monthlyContribution = calcNewMonthlyContribution(initialMousePosition[1], mousePosition[1]);
                            calculateProjection(handlerAnimationStates.dragging);
                        }

                        function onVerticalMouseUp() {
                            playLoop = true;
                            trianglesHorizontal.classed('inactive', false);
                            projections.classed("active", false);
                            graphRect.on("mousemove", null).on("mouseup", null);
                            triangles.transition().duration(400).attr("d", triangleSymbol.size(function () { return 25; }));
                            triangles.on("mouseup", null);
                            projections.on("mouseup", null);
                        }
                    }
                    function onHorizontalMouseDown() {
                        var initialMousePosition = d3.mouse(graphRect.node());

                        projections.classed("active", true);
                        graphRect.on("mousemove", onHorizontalMouseMove).on("mouseup", onHorizontalMouseUp);
                        triangles.on("mouseup", onHorizontalMouseUp);
                        projections.on("mouseup", onHorizontalMouseUp);
                        d3.event.preventDefault();

                        function onHorizontalMouseMove() {
                            playLoop = false;
                            triangles.transition().duration(400).attr("d", triangleSymbol.size(function () { return 64; }));
                            trianglesVertical.classed('inactive', true);
                            var mousePosition = d3.mouse(graphRect.node());
                            $scope.year = x.invert(mousePosition[0]);
                            calculateProjection(handlerAnimationStates.dragging);
                        }

                        function onHorizontalMouseUp() {
                            playLoop = true;
                            trianglesVertical.classed('inactive', false);
                            projections.classed("active", false);
                            graphRect.on("mousemove", null).on("mouseup", null);
                            triangles.transition().duration(400).attr("d", triangleSymbol.size(function () { return 25; }));
                            projections.on("mouseup", null);
                        }
                    }

                    trianglesVertical.on("mouseover", function () {
                        playLoop = false;
                        trianglesHorizontal.classed('inactive', true);
                    })
                    .on("mouseleave", function () {
                        playLoop = true;
                        startLoops();
                        trianglesHorizontal.classed('inactive', false);
                    })
                    .on("mousedown", onVerticalMouseDown);

                    trianglesHorizontal.on("mouseover", function () {
                        playLoop = false;
                        trianglesVertical.classed('inactive', true);
                    })
                    .on("mouseleave", function () {
                        playLoop = true;
                        startLoops();
                        trianglesVertical.classed('inactive', false);
                    })
                    .on("mousedown", onHorizontalMouseDown);

                    handler.on("mouseover", function () {
                        triangles.transition().duration(400).attr("d", triangleSymbol.size(function () { return 64; }));
                    }).on("mouseleave", function () {
                        triangles.transition().duration(400).attr("d", triangleSymbol.size(function () { return 25; }));
                    });

                    //handler.on("mouseover", function () {
                    //    triangleTop.attr("transform", "translate(0, -20)");
                    //    triangleRight.attr("transform", "translate(20, 0)rotate(90)");
                    //    triangleLeft.attr("transform", "translate(-20, 0)rotate(-90)");
                    //    triangleBottom.attr("transform", "translate(0, 20)");
                    //});



                    ////handler.on("mouseover", function () {
                    ////    var loopDuration = 800;
                    ////    function move(el, transformTo, transformFrom) {
                    ////        (function repeat() {
                    ////            el.transition()
                    ////                .duration(loopDuration)
                    ////                .attr("transform", transformTo)
                    ////                .each("end", function () {
                    ////                    el.transition()
                    ////                        .duration(loopDuration)
                    ////                        .attr("transform", transformFrom)
                    ////                        .each("end", repeat);
                    ////                });
                    ////        })();
                    ////    }
                    ////    move(triangleTop, "translate(0, -25)", "translate(0, -20)");
                    ////    move(triangleRight, "translate(25, 0)rotate(90)", "translate(20, 0)rotate(90)");
                    ////    move(triangleLeft, "translate(-25, 0)rotate(-90)", "translate(-20, 0)rotate(-90)");
                    ////    move(triangleBottom, "translate(0, 25)", "translate(0, 20)");
                    ////    d3.select(this).on("mouseover", null);
                    ////});
                    ////handler.on("mouseleave", function () {
                    ////    triangleTop.attr("transform", "translate(0, -20)");
                    ////    triangleRight.attr("transform", "translate(20, 0)rotate(90)");
                    ////    triangleLeft.attr("transform", "translate(-20, 0)rotate(-90)");
                    ////    triangleBottom.attr("transform", "translate(0, 20)");
                    ////});

                    //handler.on("mousedown", function () {
                    //    var startTime = new Date().getTime();
                    //    var initialMousePosition = d3.mouse(graphRect.node());
                    //    var initialMonthlyContribution = $scope.monhtlyContribution;
                    //    var triangles = d3.selectAll(".triangle").classed("active", true);
                    //    var projections = d3.selectAll(".projection").classed("active", true);
                    //    var r = graphRect.on("mousemove", mousemove).on("mouseup", mouseup);//.on("mouseout", mouseRectOut);
                    //    triangles.on("mouseup", mouseup);//.on("mouseout", mouseTrianglesOut);
                    //    projections.on("mouseup", mouseup);//.on("mouseout", mouseProjectionsOut);
                    //    //var rectOut = false;
                    //    //var trianglesOut = false;
                    //    //var projectionsOut = false;
                    //    d3.event.preventDefault();

                    //    function calcNewMonthlyContribution(initialY, mouseY) {
                    //        var newMonthlyContribution;
                    //        initialY = graphRectHeight / 2;
                    //        if (initialY < mouseY) {
                    //            var belowDiff = mouseY - initialY;
                    //            var belowWhole = graphRectHeight - initialY;
                    //            var belowPercent = belowDiff / belowWhole;
                    //            newMonthlyContribution = (1 - belowPercent) * initialMonthlyContribution;
                    //            if (newMonthlyContribution < 0)
                    //                newMonthlyContribution = 0;
                    //        }
                    //        else {
                    //            var currentTime = new Date().getTime();
                    //            var timeDiff = currentTime - startTime;
                    //            var factor = timeDiff <= 3000 ? 3 : Math.ceil(timeDiff / 1000);
                    //            var aboveDiff = initialY - mouseY;
                    //            var aboveWhole = initialY;
                    //            var aboveMax = initialMonthlyContribution * factor;
                    //            var abovePercent = aboveDiff / aboveWhole;
                    //            newMonthlyContribution = initialMonthlyContribution + abovePercent * aboveMax;
                    //        }

                    //        return newMonthlyContribution;
                    //    }

                    //    function mousemove() {
                    //        var mousePosition = d3.mouse(graphRect.node());
                    //        $scope.year = x.invert(mousePosition[0]);
                    //        //$scope.monthlyContribution = calcNewMonthlyContribution(initialMousePosition[1], mousePosition[1]);
                    //        calculateProjection(handlerAnimationStates.dragging);
                    //    }

                    //    function mouseup() {
                    //        triangles.classed("active", false);
                    //        projections.classed("active", false);
                    //        r.on("mousemove", null).on("mouseup", null);//.on("mouseout", null);
                    //        triangles.on("mouseup", null);//.on("mouseout", null);
                    //        projections.on("mouseup", null);//.on("mouseout", null);

                    //        //if (angular.isDefined(recalcTimer))
                    //        //    $timeout.cancel(recalcTimer);
                    //    }

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
                    //});
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
            }]
        };
    }
})();