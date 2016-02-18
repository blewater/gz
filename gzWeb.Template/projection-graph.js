/**
 * Projection Graph rendered in SVG with D3.js
 *
 * @author Miguel Mota <miguel@acorns.com>
 *
 * Misc notes:
 * Older versions of Safari require `fill` property to be an attribute,
 * rather than in CSS for it to work.
 */

var ProjectionGraph = (function() {

  /**
  * ProjectionGraph
  * @desc Projection Graph constructor
  * @constructor
  * @param {object} options - options
  * @return {object} - line graph instance
  */
  function ProjectionGraph(opts) {
    var defaults = {
      shadowDOM: null,
      el: null,
      axisData: {}, // {months, amounts}
      meanLineData: [], // main line [x,y]
      boundLineData: [], // offset lines [[x,y1,y2]*3]
      monthlyLineData: [], // [[]*2]
      monthlyValues: [], // [y]
      savingsRate: null,
      monthlyLineText: function(monthly) {
        return toCurrency(monthly, {decimal: false});
      },
      monthSelected: null,
      amountSelected: null,
      startYear: 0,
      disablePan: false,
      isSwiping: false,
      indicatorShown: false,
      verticalLineAnimation: {
        duration: 1000,
        delay: 0,
        easing: 'exp'
      },
      lineInterpolation: 'cardinal',
      onMonthPan: _.noop,
      onMonthlyPan: _.noop,
      onUpdate: _.noop,
      onSwipe: _.noop,
      onResize: _.noop
    };

    _.defaults(this, opts, defaults);

    this.state = {
      initialized: false
    };

    this.onInit();
  }

  /**
   * updateSize
   */
  ProjectionGraph.prototype.updateSize = function() {
    this.margin = {top: 0, right: 10, bottom: 40, left: 50};

    this.elWidth = $(this.shadowDOM.querySelectorAll(this.el)[0]).outerWidth();
    this.elHeight = $(this.shadowDOM.querySelectorAll(this.el)[0]).outerHeight();

    this.width =  this.elWidth - this.margin.left - this.margin.right;
    this.height = this.elHeight - this.margin.top - this.margin.bottom;
  };

  /**
   * renderContainer
   */
  ProjectionGraph.prototype.renderContainer = function() {
    this.outerSvg = this.outerSvg || d3.select(this.shadowDOM.querySelectorAll(this.el)[0]).append('svg');

    var padding = 40;

    this.outerSvg.attr('class', 'svg')
    .attr('width', this.width)
    .attr('height', this.height + padding);

    this.svg = this.svg || this.outerSvg.append('g');

    var yAxisWidth = 50;

    this.svg.attr('class', 'svg')
    .attr('width', this.width - yAxisWidth)
    .attr('height', this.height + padding)
    .attr('transform', 'translate(' + yAxisWidth + ', 0)');

    this.dragContainer = this.dragContainer || this.outerSvg.append('rect');

    this.dragContainer.attr('class', 'drag-container')
    .attr('width', this.width)
    .attr('height', this.height)
    .attr('transform', 'translate(' + yAxisWidth + ', 0)')
    .attr('fill', 'transparent');
  };

  /**
   * renderScales
   */
  ProjectionGraph.prototype.renderScales = function(scales) {
    this.xScale = d3.scale.linear()
    .domain(d3.extent(this.axisData.x, function(v) { return v; }))
    .range([0, this.width]);

    this.xDomain = _.map(this.meanLineData, function(d) {
      return d[0];
    });

    this.xScaleLine = d3.scale.linear()
     .domain(d3.extent(this.xDomain))
     .range([0, this.width]);

    this.yScaleLine = d3.scale.linear()
    .domain([0, d3.max(this.monthlyLineData, function(d) { return d[1]; })<<2])
    .range([this.height, 0]);

    this.yScale = this.yScaleLine;

    this.monthlyLinearScale = d3.scale.linear()
      .domain([0, this.height])
      .range([_.size(this.monthlyValues), 0])
      .clamp(true);

    this.monthlyPointToValueScale = d3.scale.threshold()
    .domain(this.monthlyValues)
    .range(_.range(_.first(this.monthlyValues), _.last(this.monthlyValues)));

    this.monthlyValueToPointScale = d3.scale.linear()
    .domain(this.monthlyValues)
    .range([0, this.height]);

    var dragContainerHeight = Number(this.dragContainer.attr('height'));
    var svgHeight = Number(this.svg.attr('height'));

    this.dragYScale = d3.scale.linear()
      .domain([svgHeight - dragContainerHeight, dragContainerHeight])
      .range([0, dragContainerHeight]);
  };

  /**
   * renderMeanLine
   */
  ProjectionGraph.prototype.renderMeanLine = function() {

    this.meanLineFn = d3.svg.line()
    .interpolate(this.lineInterpolation)
    .x(function(d) {
      return this.xScaleLine(d[0]);
    }.bind(this))
    .y(function(d) {
      return this.yScaleLine(d[1]);
    }.bind(this));

    this.linePath = this.linePath || this.svg.append('path');

    this.linePath.datum(this.meanLineData)
    .attr('class', 'line main-line mean-line')
    .attr('stroke', function(d, i) {
      return 'url(#graph-projection-grad1)';
     })
    .transition()
    .duration(function(d) {
      if (this.isResizing) {
        return 0;
      }
      return 500;
    }.bind(this))
    .ease('sin')
    .attr('d', this.meanLineFn);

  };

  /**
   * renderBoundLine
   */
  ProjectionGraph.prototype.renderBoundLine = function(data) {
    var line = d3.svg.line()
    .interpolate(this.lineInterpolation)
    .x(function(d) {
      return this.xScaleLine(d[0]);
    }.bind(this))
    .y(function(d) {
      return this.yScaleLine(d[1]);
    }.bind(this));

    var linePath = this.svg.append('path');

    linePath.datum(data)
    .attr('class', 'line offset-line bound-line')
    .attr('d', line);
  };

  /**
  * renderBoundLineArea
  * @desc Creates area for offset line
  * @param {array} data - line data
  * @return {object} - D3 line area svg object
  */
  ProjectionGraph.prototype.renderBoundLineArea = function(data) {
    var lineAreaFn = d3.svg.area()
    .interpolate(this.lineInterpolation)
    .x(function (d) {
      return this.xScaleLine(d[0]);
    }.bind(this))
    .y0(function (d) {
      return this.yScaleLine(d[1]);
    }.bind(this))
    .y1(function (d) {
      return this.yScaleLine(d[2]);
    }.bind(this));

    var lineAreaPath = this.svg.append('path');

    lineAreaPath
    .datum(data)
    .attr('class', function(d, i) {
      return 'area ' + 'area-' + _.size(this.boundLineAreas);
    }.bind(this))
    .attr('d', lineAreaFn)
    .attr('fill', function(d, i) {
      return 'url(#graph-projection-grad3)';
     });

    return lineAreaPath;
  };

  /**
   * renderBoundLineAreas
   */
  ProjectionGraph.prototype.renderBoundLineAreas = function() {
    if (_.size(this.boundLineAreas)) {
      _.each(this.boundLineAreas, function(line, i) {
        var lineAreaFn = d3.svg.area()
        .interpolate(this.lineInterpolation)
        .x(function (d) {
          return this.xScaleLine(d[0]);
        }.bind(this))
        .y0(function (d) {
          return this.yScaleLine(d[1]);
        }.bind(this))
        .y1(function (d) {
          return this.yScaleLine(d[2]);
        }.bind(this));

        line
        .datum(this.boundLineData[i])
        .transition()
        .duration(function(d) {
          if (this.isResizing) {
            return 0;
          }
          return 500;
        }.bind(this))
        .ease('sin')
        .attr('d', lineAreaFn);

      }.bind(this));
    } else {
      this.boundLineAreas = _.map(this.boundLineData, function(d, i) {
        //this.renderBoundLine(d);
        return this.renderBoundLineArea(d);
      }.bind(this));
    }
  };

  /**
  * renderBoundVerticalLine
  * @desc creates bound vertical line
  * @param {array} data - line data
  */
  ProjectionGraph.prototype.renderBoundVerticalLine = function(data) {
    var line = this.panLineContainer.append('line');

    line.attr('class', 'offset-vertical-line bound-vertical-line')
    .datum(data)
    .attr('x1', 0)
    .attr('x2', 0)
    .attr('y1', this.height)
    .attr('y2', this.height)
    .attr('stroke', function(d, i) {
      return 'url(#graph-projection-grad8)';
     });

    return line;
  };

  /**
   * renderBoundVerticalLines
   */
  ProjectionGraph.prototype.renderBoundVerticalLines = function(p) {
    if (_.size(this.boundVerticalLines)) {
      this.verticalLines = _.map(this.boundVerticalLines, function(line, i) {
        line.datum(this.boundLineData[i]);
        return line;
      }.bind(this));

      if (p && p.x) {
        _.each(this.boundVerticalLines, function(line, i) {
          var bisect = d3.bisector(function(d) { return d[0]; });
          var index = bisect.left(this.meanLineData, this.xScaleLine.invert(p.x));
          line
          .transition()
          .duration(function(d) {
            if (!this.state.initialized)  {
              return this.verticalLineAnimation.duration;
            }
            return 0;
          }.bind(this))
          .ease(this.verticalLineAnimation.easing)
          .attr('x1', p.x)
          .attr('x2', p.x)
          .attr('y1', function(d) {
            return this.yScaleLine(d[index][1]);
          }.bind(this))
          .attr('y2', function(d) {
            return this.yScaleLine(d[index][2]);
          }.bind(this));
        }, this);
      }
    } else {
      this.boundVerticalLines = _.map(this.boundLineData, function(d, i) {
        return this.renderBoundVerticalLine(d);
      }.bind(this));
    }

  };

  /**
   * renderMonthlyLine
   */
  ProjectionGraph.prototype.renderMonthlyLine = function() {
    this.monthlyLineFn = d3.svg.line()
    .interpolate(this.lineInterpolation)
    .x(function(d, i) {
      return this.xScaleLine(d[0]);
    }.bind(this))
    .y(function(d, i) {
      return this.yScaleLine(d[1]);
    }.bind(this));

    this.monthlyLinePath = this.monthlyLinePath || this.svg.append('path');
    this.monthlyLinePath.datum(this.monthlyLineData)
    .attr('class', 'line v-line Monthly-line')
    .attr('stroke', function(d, i) {
      return 'url(#graph-projection-grad7)';
     })
    .transition()
    .duration(function(d) {
      if (this.isResizing) {
        return 0;
      }
      return 500;
    }.bind(this))
    .ease('sin')
    .attr('d', this.monthlyLineFn);

    // OPTIMIZE: kinda ugly
    var rotateAngle = slopeAngle([this.xScaleLine(this.monthlyLineData[0][0]),this.yScaleLine(this.monthlyLineData[0][1])],[this.xScaleLine(this.monthlyLineData[1][0]),this.yScaleLine(this.monthlyLineData[1][1])]);

    this.monthlyLineLabel = this.monthlyLineLabel || this.svg.append('text');
    var label = this.monthlyLineLabel;
    if (this.state.initialized) {
      label = label
        .transition()
        .duration(function(d) {
          if (this.isResizing) {
            return 0;
          }
          return 500;
        }.bind(this))
        .ease('sin');
    }
    label.attr('class', 'v-line-text monthly-line-text')
      .attr('transform', function(d) {
        var offset = 20;
        return 'translate('+ this.xScaleLine(this.monthlyLineData[1][0]) +', '+ (this.yScaleLine(this.monthlyLineData[1][1]) + offset) +') rotate('+ rotateAngle +')';
      }.bind(this))
      .text(this.monthlyLineText(this.savingsRate));
  };

  /**
   * renderVerticalLine
   */
  ProjectionGraph.prototype.renderVerticalLine = function() {
    this.verticalLine = this.verticalLine || this.panLineContainer.append('rect');

    this.verticalLine.attr('class', 'vertical-line')
    .attr('x', 0)
    .attr('y', 0)
    .attr('height', this.height)
    .attr('width', 1)
    .attr('transform', 'translate(-1,0)')
    .attr('fill', function(d, i) {
      return 'url(#graph-projection-grad9)';
     });
  };

  /**
   * renderYAxis
   */
  ProjectionGraph.prototype.renderYAxis = function() {

    this.yAxis = d3.svg.axis()
    .scale(this.yScale)
    .orient('left')
    //.tickValues(_.map(this.axisData.y, function(v) { return v; }))
    .ticks(minusPercent(_.size(this.axisData.y), 30))
    .tickFormat(function(d) { return '$' + commafy(d); });

    this.yAxisLabels = this.yAxisLabels || this.outerSvg.append('g');

    var labelOffset = 15;

    this.yAxisLabels.attr('class', 'y axis')
    .attr('transform', function(d, i) {
      return 'translate(' + labelOffset + ',0)';
    })
    .transition()
    .duration(50)
    .ease('sin')
    .call(this.yAxis);

    this.yAxisLines = this.yAxisLines || this.svg.append('g');
    this.yAxisLines.attr('class', 'grid')
      .call(this.yAxis.tickSize(-this.width, 0, 0).tickFormat(''));

    d3.selectAll('.axis.y text')
    .style('text-anchor', 'start')
    .classed('highlight', function(d, i) {
      return i % 2 === 0;
    })
    .attr('index', function(r, i) {
      return i;
    })
    .attr('transform', function(d, i) {
      var t = d3.select(this).text();
      if (/\$0/.test(t)) {
        return 'translate(0,-20)';
      }
    });
  };

  /**
   * renderXAxis
   */
  ProjectionGraph.prototype.renderXAxis = function() {
    this.xAxis = d3.svg.axis()
    .scale(this.xScale)
    .orient('bottom')
    .tickValues(d3.range(_.size(this.meanLineData)));
  };

  /**
   * renderAxes
   */
  ProjectionGraph.prototype.renderAxes = function() {
    this.renderXAxis();
    this.renderYAxis();

    this.xAxisLabels = this.xAxisLabels || this.svg.append('g');

    this.xAxisLabels.attr('class', 'x axis')
    .attr('transform', 'translate(10,'+ this.height +')')
    .transition()
    .duration(function(d) {
      if (this.isResizing) {
        return 0;
      }
      return 500;
    }.bind(this))
    .ease('sin')
    .call(this.xAxis);

    var range = d3.range(20);

    this.xAxisLabels.selectAll('text')
    .text(function(d, i) {
      var n = range.indexOf(d/12);
      return this.startYear + n;
    }.bind(this))
    .style('display', function(d, i) {
      var n = range.indexOf(d/12);
      var v = parseInt(d3.select(this)[0][0].textContent);
      return n > -1 && v % 2 === 0 ? 'block' : 'none';
    })
    .classed('highlight', function(d, i) {
      var v = parseInt(d3.select(this)[0][0].textContent);
      return v % 10 === 0;
    });
  };

  /**
   * renderPanLineContainer
   */
  ProjectionGraph.prototype.renderPanLineContainer = function() {
    this.panLineContainer = this.panLineContainer || this.svg.append('g');
    this.panLineContainer
    .attr('class', 'pan-line-container');
  };

  /**
   * renderMarker
   */
  ProjectionGraph.prototype.renderMarker = function() {
    this.marker = this.marker || this.panLineContainer.append('circle');

    this.marker.attr('class', 'marker')
    .attr('cx', 0)
    .attr('cy', this.height)
    .attr('r', 6);
  };

  /**
   * renderOriginMarker
   */
  ProjectionGraph.prototype.renderOriginMarker = function() {
    this.originMarker = this.originMarker || this.panLineContainer.append('circle');

    this.originMarker
    .classed('origin-marker', true)
    .attr('cx', 0)
    .attr('cy', this.yScaleLine(this.meanLineData[0][1]))
    .attr('r', 6);
  };

  /**
   * renderMarkerDragIndicator
   */
  ProjectionGraph.prototype.renderMarkerDragIndicator = function () {
    if (!this.markerDragIndicator) {
      this.markerDragIndicator = this.markerDragIndicator || this.panLineContainer.append('g');
      this.markerDragIndicator
      .attr('class', 'marker-drag-indicator')
      .attr('opacity', 0);
    }

    // Indicator text
    this.markerDragIndicatorText = this.markerDragIndicatorText || this.markerDragIndicator.append('text');
    this.markerDragIndicatorText
    .attr('x', 0)
    .attr('y', this.height + 35)
    .attr('text-anchor', 'middle')
    .attr('class', 'marker-drag-indicator-text')
    .text('Drag Me');

    // Left arrow
    this.markerDragIndicatorLeftArrow = this.markerDragIndicatorLeftArrow || this.markerDragIndicator.append('path');
    this.markerDragIndicatorLeftArrow
    .attr('d', 'M0.218225795,13.5 C0.156576773,13.2674887 0.217696123,13.0103099 0.399920366,12.8280857 L12.6125684,0.615437608 C12.8878213,0.340184776 13.3269696,0.343160264 13.5996633,0.61585395 C13.8742581,0.890448807 13.8725434,1.33048506 13.6000796,1.6029488 L1.70302844,13.5 L13.6000796,25.3970512 C13.8725434,25.6695149 13.8742581,26.1095512 13.5996633,26.384146 C13.3269696,26.6568397 12.8878213,26.6598152 12.6125684,26.3845624 L0.399920366,14.1719143 C0.217696123,13.9896901 0.156576773,13.7325113 0.218225795,13.5 Z')
    .attr('fill', 'rgba(255,255,255,1)')
    .attr('transform', 'translate(-30,' + (this.height - 12) + ')');

    // Right arrow
    this.markerDragIndicatorRightArrow = this.markerDragIndicatorRightArrow || this.markerDragIndicator.append('path');
    this.markerDragIndicatorRightArrow
    .attr('d', 'M0.218225795,13.5 C0.156576773,13.2674887 0.217696123,13.0103099 0.399920366,12.8280857 L12.6125684,0.615437608 C12.8878213,0.340184776 13.3269696,0.343160264 13.5996633,0.61585395 C13.8742581,0.890448807 13.8725434,1.33048506 13.6000796,1.6029488 L1.70302844,13.5 L13.6000796,25.3970512 C13.8725434,25.6695149 13.8742581,26.1095512 13.5996633,26.384146 C13.3269696,26.6568397 12.8878213,26.6598152 12.6125684,26.3845624 L0.399920366,14.1719143 C0.217696123,13.9896901 0.156576773,13.7325113 0.218225795,13.5 Z')
    .attr('fill', 'rgba(255,255,255,1)')
    .attr('transform', 'scale(-1, 1) translate(-30,' + (this.height -12) + ')');
  };

  /**
   * getInitialLinePoint
   */
  ProjectionGraph.prototype.getInitialLinePoint = function() {
    return this.meanLineData[(this.meanLineData.length >> 1) | 0];
  };

  /**
   * bisect
   */
  ProjectionGraph.prototype.bisect = d3.bisector(function(datum) {
    return datum[0];
  }).right;

  /**
  * updatePointOnLine
  * @param data
  * @param x cordinate
  * @returns {object} - updated points
  */
  ProjectionGraph.prototype.updatePointOnLine = function(d, x) {
    if (!d || !x) return;

    this.marker
    .transition()
    .duration(function(d) {
      if (!this.state.initialized)  {
        return this.verticalLineAnimation.duration;
      }
      return 0;
    }.bind(this))
    .ease(this.verticalLineAnimation.easing)
    .attr('cx', x)
    .attr('cy', this.yScaleLine(d[1]));

    return this.getPointOnLinePosition();
  };
  /**
  * updateDragIndicatorPosition
  * @param data
  * @param x cordinate
  * @returns {object} - updated points
  */
  ProjectionGraph.prototype.updateDragIndicatorPosition = function(d, x) {
     if (!d || !x) return;

     // Translate text
    this.markerDragIndicatorText
     .transition()
    .duration(function(d) {
      if (!this.state.initialized)  {
        return this.verticalLineAnimation.duration;
      }
      return 0;
    }.bind(this))
    .ease(this.verticalLineAnimation.easing)
    .attr('x', x)
    .attr('y', this.yScaleLine(d[1]) + 35);

    // Translate left arrow
    this.markerDragIndicatorLeftArrow
     .transition()
    .duration(function(d) {
      if (!this.state.initialized)  {
        return this.verticalLineAnimation.duration;
      }
      return 0;
    }.bind(this))
    .ease(this.verticalLineAnimation.easing)
    .attr('transform', 'translate(' + (x - 30) + ', ' + (this.yScaleLine(d[1]) - 12) +')');

    // Translate right arrow
    this.markerDragIndicatorRightArrow
    .transition()
    .duration(function(d) {
      if (!this.state.initialized)  {
        return this.verticalLineAnimation.duration;
      }
      return 0;
    }.bind(this))
    .ease(this.verticalLineAnimation.easing)
    .attr('transform', 'scale(-1,1) translate(' + (-x - 30) + ', ' + (this.yScaleLine(d[1]) - 12) +')');
  };

  /**
  * fadeOutDragIndicatorArrows
  */
  ProjectionGraph.prototype.fadeOutDragIndicator = function () {
     // Fade out entire indicator
    this.markerDragIndicator
    .transition()
    .duration(500)
    .attr('opacity', 0);
  }

   /**
  * fadeInDragIndicatorArrows
  */
  ProjectionGraph.prototype.fadeInDragIndicator = function () {
    // Fade in entire indicator
    this.markerDragIndicator
    .transition()
    .duration(500)
    .attr('opacity', 1);
  }

  /**
  * getPointOnLinePosition
  * @desc returns x and y points for line markers current position
  * @return {object} - x and y points
  */
  ProjectionGraph.prototype.getPointOnLinePosition = function() {
    return {x: this.marker.attr('cx')|0, y: this.marker.attr('cy')|0};
  };

  /**
  * bisectPoint
  * @desc
  * @return
  */
  ProjectionGraph.prototype.bisectPoint = function(x, scale, data) {
    scale = scale || this.xScaleLine;
    data = data || this.meanLineData;

    var timestamp = scale.invert(x),
    index = this.bisect(data, timestamp);

    var startDatum = data[index - 1],
    endDatum = data[index];

    if (startDatum && endDatum) {
      var interpolate = d3.interpolateNumber(startDatum[1], endDatum[1]);
      var range = endDatum[0] - startDatum[0],
      valueY = interpolate((timestamp % range) / range);

      var d = timestamp - startDatum[1] > endDatum[1] - timestamp ? endDatum : startDatum;

      return d;
    }
  };

  /**
   * renderPanLines
   */
  ProjectionGraph.prototype.renderPanLines = function(p) {
    if (!p) return;
    p.x = p.x > 0 ? p.x : 0;

    this.verticalLine
    .transition()
    .duration(function(d) {
      if (!this.state.initialized)  {
        return this.verticalLineAnimation.duration;
      }
      return 0;
    }.bind(this))
    .ease(this.verticalLineAnimation.easing)
    .attr('x', p.x);

    this.updatePointOnLine(this.bisectPoint(p.x), p.x);
    this.updateDragIndicatorPosition(this.bisectPoint(p.x), p.x);
    this.renderBoundVerticalLines({x:p.x});
  };

  /**
  * onGraphPan
  * @desc called when the graph is being panned
  * @param {object} points
  * @param {number} points.x
  * @param {number} points.y
  * @return
  */
  ProjectionGraph.prototype.onGraphPan = function(p) {
    // return if dragging off main area
    // console.log(p.x, this.width);
    if ((p.x && !(p.x > 0 && p.x < this.width))) {
      return;
    }

    // normalize p.y since hammer does not respect containment
    p.y = this.dragYScale(p.y);

    var amount, monthly;

    var pts = {};

    if (p.x) {
      var bisect = d3.bisector(function(d) { return d[0]; });

      this.renderPanLines({x:p.x,y:p.y});

      var month = bisect.left(this.meanLineData, this.xScaleLine.invert(p.x));
      this.monthSelected = month;
    }

    if (p.y) {
      amount = this.yScale.invert(this.getPointOnLinePosition().y) | 0;
      this.amountSelected = amount;
      monthly = this.monthlyPointToValueScale.invertExtent(this.monthlyLinearScale(p.y)|0);

      if (monthly[1] === _.last(this.monthlyValues)) {
        monthly = monthly[1];
      } else {
        monthly = monthly[0];
      }

      if (monthly > 0) {
        this.savingsRate = monthly;

        this.onSavingsRatePan(monthly);
        this.renderMonthlyLine();
        this.renderScales();
        this.renderAxes();
      }
    }

    this.updateAmount(p);
  };

  /**
   * pandTo
   */
  ProjectionGraph.prototype.panTo = function(p) {
    this.lastPosition = p;
    this.onGraphPan({x:p[0], y:p[1]});
  };

  /**
   * updateAmount
   */
  ProjectionGraph.prototype.updateAmount = function(p) {
    if (!p) p = {};
    // delaying for the point to update
    _.delay(function() {
      var point = this.getPointOnLinePosition().y;
      var amount = this.yScale.invert(point) | 0;
      this.onMonthPan(this.monthSelected||0, this.amountSelected);
    }.bind(this), 200);
  };

  ProjectionGraph.prototype.panToMonth = function(month) {
    var cancelTimer = false;

    this.onGraphPan.call(this, {x: this.xScale(month), y: this.monthlyValueToPointScale(this.savingsRate)});

    _.delay(function() {
      cancelTimer = true;
      this.state.initialized = true;
      this.lastPosition = _.pluck(this.getPointOnLinePosition());
    }.bind(this), this.verticalLineAnimation.duration);

    d3.timer(function() {

      var bisect = d3.bisector(function(d) { return d[0]; });

      var p = this.getPointOnLinePosition();

      //this.renderPanLines({x:p.x,y:p.y});

      var month = bisect.left(this.meanLineData, this.xScaleLine.invert(p.x));
      this.monthSelected = month;

      var amount = this.yScale.invert(p.y) | 0;
      this.amountSelected = amount;
      this.updateAmount();
      return cancelTimer;
    }.bind(this));

    this.lastPosition = this.lastPosition || [0,0];
    this.dragDirection = 0; // 0 none, -1 up/down, 1 left/right
  };

  ProjectionGraph.prototype.setMonthlyAmount = function(monthlyAmount) {
    var bisect = d3.bisector(function(d) {
      return d;
    }).left;

    var monthly = this.monthlyValues[bisect(this.monthlyValues, monthlyAmount)];
    this.savingsRate = monthly;
    this.onSavingsRatePan(monthly);
    this.renderMonthlyLine();
    this.renderAxes();
  };

  /**
   * setMonthlyLineData
   */
  ProjectionGraph.prototype.setMonthlyLineData = function(data) {
    this.monthlyLineData = data;
  };

  /**
   * setDisablePan
   */
  ProjectionGraph.prototype.setDisablePan = function(shouldDisable) {
    this.disablePan = !!shouldDisable;
  };

  /**
   * init
   */
  ProjectionGraph.prototype.init = function() {
    var _this = this;

    this.update();
    this.renderGradientDefs();

    function onMouseMove(ev) {
      if (_this.disablePan) {
        return;
      }

      var pointer = ev.pointers[0];

      var svgOffset = $(_this.dragContainer[0][0]).offset();
      var svgClientRect = _this.dragContainer[0][0].getBoundingClientRect();

      var p = [(pointer.offsetX || pointer.layerX || pointer.clientX - (svgClientRect.left || svgOffset.left)) - _this.margin.left, pointer.offsetY || pointer.layerY || pointer.clientY - (svgClientRect.top || svgOffset.top)];
      var mX = _this.getPointOnLinePosition().x;
      var dragPadding = 50;

      if (Math.abs(p[1] - _this.lastPosition[1]) === 0) {
        // only allow side pan if close to line on mobile
        var allowForSwipe = $(window).width() <= 800 ? (mX <= p[0] + dragPadding && mX >= p[0] - dragPadding) : true;
        if (_this.dragDirection === 0) {
          _this.dragDirection = 1;
          $(_this.shadowDOM.querySelector(_this.el)).addClass('x');
        }
      }
      if (Math.abs(p[0] - _this.lastPosition[0]) === 0) {
        if (_this.dragDirection === 0) {
          _this.dragDirection = -1;
          $(_this.shadowDOM.querySelector(_this.el)).addClass('y');
        }
      }

      if (_this.dragDirection === 1) {
        _this.onGraphPan.call(_this, _.extend({x:p[0]}, ev, {panDirection: _this.dragDirection}));
      }
      if (_this.dragDirection === -1) {
        _this.onGraphPan.call(_this, _.extend({y:p[1]},  ev, {panDirection: _this.dragDirection}));
      }

      _this.lastPosition = p;
    }

    _this.canPan = false;

    var hammerEl = _this.dragContainer[0][0];

    var mc = new Hammer.Manager(hammerEl);
    mc.add( new Hammer.Pan({ event: 'pan'}) );
    mc.add( new Hammer.Swipe({ event: 'swipe'}) );
    mc.add( new Hammer.Press({ event: 'press', time: 1}) ); // mobile: 90
    mc.get('swipe').recognizeWith('pan');
    mc.get('pan').recognizeWith('press');
    mc.get('pan').requireFailure('swipe');
    mc.get('swipe').requireFailure('press');
    mc.on('pan press pressup', function(ev) {
      if (ev.type === 'swipe' && !_this.canPan) {
        // TODO: mobile only
        //_this.onSwipe(ev);
      }
      if (ev.type === 'press') {
        _this.canPan = true;
        _this.onMonthPanStart.call(_this);
      }
      if (ev.type === 'pressup') {
        _this.onMonthPanEnd.call(_this);
      }
      if (ev.type === 'pan' && _this.canPan) {
        var isWithinBoundingRect = ev.pointers[0].clientX >= _this.svg[0][0].getBoundingClientRect().left;
        if (isWithinBoundingRect) {
          onMouseMove(ev);
        }
      }
    });

    $(document).mouseup(_this.onMonthPanEnd.bind(_this));

    var resizeCallback = _.debounce(function() {
      _this.isResizing = false;
    }, 500);

    $(window).on('resize', function() {
      this.resize();
      resizeCallback();
    }.bind(this));

    $(this.shadowDOM.querySelectorAll(this.el)[0]).on('mousemove', _.throttle(function(ev) {
      _this.lastMousePosition = ev;
    }, 100));
  };

  ProjectionGraph.prototype.panToAge = function(age) {
    var cancelTimer = false;

    this.onGraphPan.call(this, {x: this.xScale(age) + 5, y: this.monthlyValueToPointScale(this.savingsRate)});

    _.delay(function() {
      cancelTimer = true;
      this.state.initialized = true;
      this.lastPosition = _.pluck(this.getPointOnLinePosition());
    }.bind(this), this.verticalLineAnimation.duration);

    d3.timer(function() {
      var p = this.getPointOnLinePosition();
      var age = this.xScale.invert(p.x);
      this.monthSelected = age;
      var amount = this.yScale.invert(p.y) | 0;
      this.amountSelected = amount;
      this.updateAmount();
      return cancelTimer;
    }.bind(this));

    this.lastPosition = this.lastPosition || [0,0];
    this.dragDirection = 0; // 0 none, -1 up/down, 1 left/right
  };

  /**
   * onMonthPanStart
   */
  ProjectionGraph.prototype.onMonthPanStart = function(p) {
      $(this.shadowDOM.querySelectorAll(this.el)[0]).addClass('grabbing');
      $('body').css('position', 'relative'); // prevent page from scrolling


      // Hide helper indicator
      if(!this.indicatorShown) {
        this.fadeOutDragIndicator();
        this.indicatorShown = true;
      }
  };

  /**
   * onMonthPanEnd
   */
  ProjectionGraph.prototype.onMonthPanEnd = function() {
    $(this.shadowDOM.querySelectorAll(this.el)[0]).removeClass('grabbing x y');
    this.dragDirection = 0;
    this.canPan = false;
    this.lastDeltaY = 0;
    $('body').css('position', 'relative');
  };

  /**
   * update
   */
  ProjectionGraph.prototype.update = function(opts) {
    if (opts) _.extend(this, opts);

    this.updateSize();
    this.renderContainer();
    this.renderScales();
    this.renderAxes();
    this.renderMeanLine();
    this.renderBoundLineAreas();
    this.renderPanLineContainer();
    this.renderBoundVerticalLines();
    this.renderMarker();
    this.renderOriginMarker();
    this.renderMarkerDragIndicator();
    this.renderVerticalLine();
    this.renderMonthlyLine();

    var x = this.xScale(this.monthSelected);
    var y = this.monthlyValueToPointScale(this.savingsRate);

    this.panTo([x,y]);

    this.renderPanLines({x:x,y:y});

    this.onUpdate();
  };

  /**
   * resize
   */
  ProjectionGraph.prototype.resize = function() {
    this.isResizing = true;
    this.update();
    this.onResize();
  };

  /**
   * renderGradientDefs
   */
  ProjectionGraph.prototype.renderGradientDefs = function() {
    var gradient1 = this.svg.append('svg:defs')
      .append('svg:linearGradient')
      .attr('id', 'graph-projection-grad1')
      .attr('x1', '0%')
      .attr('y1', '0%')
      .attr('x2', '100%')
      .attr('y2', '0%');

    gradient1
      .append('svg:stop')
      .attr('offset', '0')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 1);

    gradient1
      .append('svg:stop')
      .attr('offset', '50%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 1);

    gradient1
      .append('svg:stop')
      .attr('offset', '100%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0);

    var gradient2 = this.svg.append('svg:defs')
      .append('svg:linearGradient')
      .attr('id', 'graph-projection-grad2')
      .attr('x1', '0%')
      .attr('y1', '0%')
      .attr('x2', '100%')
      .attr('y2', '0%');

    gradient2
      .append('svg:stop')
      .attr('offset', '0')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 1);

    gradient2
      .append('svg:stop')
      .attr('offset', '50%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0.3);

    gradient2
      .append('svg:stop')
      .attr('offset', '100%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0);

    var gradient3 = this.svg.append('svg:defs')
      .append('svg:linearGradient')
      .attr('id', 'graph-projection-grad3')
      .attr('x1', '0%')
      .attr('y1', '0%')
      .attr('x2', '100%')
      .attr('y2', '0%');

    gradient3
      .append('svg:stop')
      .attr('offset', '0')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0.2);

    gradient3
      .append('svg:stop')
      .attr('offset', '50%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0.2);

    gradient3
      .append('svg:stop')
      .attr('offset', '90%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0);

    var gradient4 = this.svg.append('svg:defs')
      .append('svg:linearGradient')
      .attr('id', 'graph-projection-grad4')
      .attr('x1', '0%')
      .attr('y1', '0%')
      .attr('x2', '100%')
      .attr('y2', '0%');

    gradient4
      .append('svg:stop')
      .attr('offset', '0')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0.2);

    gradient4
      .append('svg:stop')
      .attr('offset', '50%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0.2);

    gradient4
      .append('svg:stop')
      .attr('offset', '70%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0);

    var gradient5 = this.svg.append('svg:defs')
      .append('svg:linearGradient')
      .attr('id', 'graph-projection-grad5')
      .attr('x1', '0%')
      .attr('y1', '0%')
      .attr('x2', '100%')
      .attr('y2', '0%');

    gradient5
      .append('svg:stop')
      .attr('offset', '0')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0.2);

    gradient5
      .append('svg:stop')
      .attr('offset', '50%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0.2);

    gradient5
      .append('svg:stop')
      .attr('offset', '80%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0);

    var gradient6 = this.svg.append('svg:defs')
      .append('svg:linearGradient')
      .attr('id', 'graph-projection-grad6')
      .attr('x1', '0%')
      .attr('y1', '0%')
      .attr('x2', '100%')
      .attr('y2', '100%')
      .attr('gradientUnits', 'userSpaceOnUse')
      .attr('spreadMethod', 'pad')
      .attr('gradientTransform', 'rotate(90)');

    gradient6
      .append('svg:stop')
      .attr('offset', '0')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0);

    gradient6
      .append('svg:stop')
      .attr('offset', '50%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 1);

    gradient6
      .append('svg:stop')
      .attr('offset', '100%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0);

    var gradient7 = this.svg.append('svg:defs')
      .append('svg:linearGradient')
      .attr('id', 'graph-projection-grad7')
      .attr('x1', '0%')
      .attr('y1', '0%')
      .attr('x2', '100%')
      .attr('y2', '0%');

    gradient7
      .append('svg:stop')
      .attr('offset', '0')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 1);

    gradient7
      .append('svg:stop')
      .attr('offset', '90%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 1);

    gradient7
      .append('svg:stop')
      .attr('offset', '95%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0);

    var gradient8 = this.svg.append('svg:defs')
      .append('svg:linearGradient')
      .attr('id', 'graph-projection-grad8')
      .attr('x1', '0%')
      .attr('y1', '0%')
      .attr('x2', '100%')
      .attr('y2', '100%')
      .attr('gradientUnits', 'userSpaceOnUse')
      .attr('spreadMethod', 'pad');

    gradient8
      .append('svg:stop')
      .attr('offset', '0')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0.25);

    gradient8
      .append('svg:stop')
      .attr('offset', '40%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0.15);

    gradient8
      .append('svg:stop')
      .attr('offset', '100%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0);

    var gradient9 = this.svg.append('svg:defs')
      .append('svg:linearGradient')
      .attr('id', 'graph-projection-grad9')
      .attr('gradientTransform', 'rotate(90)');

    gradient9
      .append('svg:stop')
      .attr('offset', '0')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0);

    gradient9
      .append('svg:stop')
      .attr('offset', '90%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 1);

    gradient9
      .append('svg:stop')
      .attr('offset', '100%')
      .attr('stop-color', 'rgb(255,255,255)')
      .attr('stop-opacity', 0);
  };

  ProjectionGraph.prototype.destroy = function() {
    $(window).off('resize.projection-graph');
  };

  return ProjectionGraph;
})();