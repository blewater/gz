Polymer('acorns-portfolios-visualization', {
  loader: null,
  // Fires when an instance of the element is created
  created: function() {},
  // Fires when the element’s initial set of children and siblings are guaranteed to exist
  domReady: function() {},
  // Fires when the "<polymer-element>" has been fully prepared
  ready: function() {},
  selectedPortfolioIndex: 0,
  // Fires when the element was inserted into the document
  attached: function() {
    var self = this;
    /*
     * Hide the loader for this element
     */
    if (this.loader) {
      hideLoader();
    }

    function hideLoader() {
      self.$.graph.classList.add('fadeIn');
      var loaderElement = document.getElementById(self.loader);
      loaderElement.classList.add('fadeOut');
      setTimeout(function() {
        loaderElement.style.display = 'none';
      }, 350);
    }
    /*
     * Setup visualization
     */
    (function(root, opts) {
      'use strict';
      opts = opts || {};
      var $el = {
        leftLabel: $(self.shadowRoot.querySelector('.graph-header-left-label')),
        leftValue: $(self.shadowRoot.querySelector('.graph-header-left-value')),
        centerLabel: $(self.shadowRoot.querySelector('.graph-header-center-label')),
        centerValue: $(self.shadowRoot.querySelector('.graph-header-center-value')),
        centerBottomValue: $(self.shadowRoot.querySelector('.graph-header-center-bottom-value')),
        rightLabel: $(self.shadowRoot.querySelector('.graph-header-right-label')),
        rightValue: $(self.shadowRoot.querySelector('.graph-header-right-value')),
        navNext: $(self.shadowRoot.querySelector('.projection-graph-next')),
        navPrev: $(self.shadowRoot.querySelector('.projection-graph-prev')),
      };
      self.portfolios = _.map(['AGGRESSIVE', 'MODERATE', 'CONSERVATIVE'], function(p) {
        return {
          name: p,
          slug: slugify(p)
        };
      });


      function init(coeffData) {
        var portfolioThemeTimeout;

        var startYear = opts.age || 0;
        var startingValue = opts.startingValue || 0;
        var savingsRate = opts.savingsRate || 0;
        var portfolio = opts.portfolio || 'moderate';

        self.selectedPortfolioIndex = self.portfolios.indexOf(_.find(self.portfolios, {slug: portfolio}));

        function calcSaveRates(savingsRate, startingValue) {
          var dataSaveRates = [];
          dataSaveRates[0] = startingValue;
          for (var i = 1; i < 240; i++) {
            dataSaveRates[i] = dataSaveRates[i - 1] + savingsRate;
          }
          return dataSaveRates;
        }

        var dataSaveRates = calcSaveRates(savingsRate, startingValue);

        function getSaveRate(month) {
          return dataSaveRates[month];
        }

        function initialAxisData() {
          return {
            x: _.range(0, 240),
            y: _.range(0,70000, 10000)
          };
        }

        function valueAtMean(month) {
          return projectionLineData.mean[month][1];
        }

        var getPortfolioLinesFromPortfolio = Acorns.getPortfolioLinesFromPortfolio;

        function getLineData(savingsRate, startingValue, portfolioType) {
          var lines = getPortfolioLinesFromPortfolio(savingsRate, startingValue, portfolioType);

          return {
            // [index,,mean]
            mean: _.map(lines.Mean, function(val, i) {
              return [i, val];
            }),

            // [[index,68lb,68ub]x240,[index, 95lb, 95ub]x240,[index, 99lb, 99ub]x240]
            bound: _.map(_.range(3), function(i) {
              return _.map(_.range(240), function(j) {
                if (i === 0) {
                  return [j, lines.LB68[j], lines.UB68[j]];
                }
                if (i === 1) {
                  return [j, lines.LB95[j], lines.UB95[j]];
                }
                if (i === 2) {
                  return [j, lines.LB99[j], lines.UB99[j]];
                }
              });
            })
          };
        }

        var projectionLineData = getLineData(savingsRate, startingValue, portfolio);

        var monthlyRanges = [[0,1],_.range(1,100,1), _.range(100,1000,10), _.range(1000,10100,100)];
        var monthlyValues = _.reduce(monthlyRanges, function(a,r) {
          return a.concat(r);
        }, []);

        function monthlyLineData(savingsRate, startingValue) {
          var x = 240;
          var y = savingsRate * x;
          // [startingPoint, quantile point, end point]
          return [
            [0,startingValue],
            [minusPercent(x, 40), startingValue + minusPercent(y,40)],
            [x,y + startingValue]];
        }

        root.projectionGraph = new ProjectionGraph({
          shadowDOM: self.shadowRoot,
          el: '.projection-graph',
          axisData: initialAxisData(),
          meanLineData: projectionLineData.mean,
          boundLineData: projectionLineData.bound,
          monthlyValues: monthlyValues,
          monthlyLineData: monthlyLineData(savingsRate, startingValue),
          savingsRate: savingsRate,
          monthlyLineText: function(monthly) {
            return toCurrency(monthly, {decimal: false}) + ' Monthly Investment';
          },
          startYear: startYear,

          onInit: function(d) {
            $el.rightLabel.text('Monthly Investment');
            $el.leftLabel.text('Years');
            $el.centerLabel.text('Projected Value');
            $el.rightValue.text(toCurrency(savingsRate, {
              decimal: false
            }));
            $el.centerValue.text(toCurrency(0, {
              decimal: false
            }));
            $el.centerBottomValue.text(['(', toCurrency(0, {
              decimal: false,
              sign: true
            }), ')'].join(''));
            $el.leftValue.text(startYear);
          },

          onUpdate: function(d) {
            function updateTheme() {
              var duration = 500;
              var colorClasses = _.reduce(self.portfolios, function(acc, o, i) {
                return (acc.push(o.slug), acc);
              }, []).join(' ');
              var activeClass = self.portfolios[self.selectedPortfolioIndex].slug;
              self.shadowRoot.querySelectorAll('.portfolio-title')[0].innerHTML = self.portfolios[self.selectedPortfolioIndex].name;
              var $graphContainer = $('.portfolios');
              var $portfolioGraphBg = $('.portfolios-background');
              // Add correct portfolio class to container
              $graphContainer.removeClass(colorClasses).addClass(activeClass);
              portfolioThemeTimeout = setTimeout(function() {
                $portfolioGraphBg.removeClass(colorClasses).addClass(activeClass).fadeIn(0);
              }, duration);
              $portfolioGraphBg.fadeOut(duration);
            }
            updateTheme();
          },

          onMonthPan: function(month, projectedValue) {
            var meanValue = valueAtMean(month);
            var valueDiff = parseInt(meanValue - getSaveRate(month), 10);
            var newAge = startYear + (Math.round(month/12));

            $el.centerValue.text(toCurrency(meanValue, {decimal: false}));
            $el.leftValue.text(newAge);
            $el.centerBottomValue.text('(' + toCurrency(valueDiff, {decimal: false, sign: true}) + ')');
          },

          onSavingsRatePan: function(savings) {
            savingsRate = savings;
            dataSaveRates = calcSaveRates(savingsRate, startingValue);
            updateData();
            $el.rightValue.text(toCurrency(savingsRate, {decimal: false}));
          },

          onSwipe: function(ev) {
            if (ev.direction === 4) {
              navPrev();
            }
            if (ev.direction === 2) {
              navNext();
            }
          },

          onResize: function() {}
        });
        root.projectionGraph.init();

        var updateData = _.debounce(function() {
          projectionLineData = getLineData(savingsRate, startingValue, portfolio);
          self.projectionGraph.update({
            meanLineData: projectionLineData.mean,
            boundLineData: projectionLineData.bound,
            monthlyLineData: monthlyLineData(savingsRate, startingValue)
          });
        }, 100);
        /*
         * Loop through portfolios
         */
        var carouselInterval = setInterval(self.navNext, 4000);
        self.projectionGraphNav = new NavLabels({
          shadowDOM: self.shadowRoot,
          el: '.projection-graph-nav',
          names: self.portfolios,
          selected: self.selectedPortfolioIndex,
          onSelect: function(i) {
            updateSelection(i);
          },
          onMouseOver: function() {
            clearInterval(carouselInterval);
          }
        });

        function updateSelection(i) {
          self.selectedPortfolioIndex = i;
          portfolio = self.portfolios[self.selectedPortfolioIndex].slug;

          projectionLineData = getLineData(savingsRate, startingValue, portfolio);

          self.projectionGraph.update({
            meanLineData: projectionLineData.mean,
            boundLineData: projectionLineData.bound,
            monthlyLineData: monthlyLineData(savingsRate, startingValue)
          });
        }
      }
      var played = false;
      initOnVisible();

      function initOnVisible() {
        $('.portfolios .container acorns-portfolios-visualization').appear();
        $('.portfolios .container acorns-portfolios-visualization').on('appear', function(event, $all_appeared_elements) {
          if (!played) {
            /*
             * Animate pan to age on visible
             */
            root.projectionGraph.panToMonth(135);
            // Fade in drag indicator
            setTimeout(function () {
              root.projectionGraph.fadeInDragIndicator();
            }, 1500)
            played = true;
          }
        });
      }

      if (Acorns.coefficientData) {
        init();
      } else {
        window.addEventListener('coefficientDataLoaded', function (e) {
          init();
        }, false);
      }
    })(this, {
      age: 0,
      startingValue: 0,
      savingsRate: 30,
      portfolio: 'aggressive'
    });
  },
  prevPortfolio: function(event, detail, sender) {
    if (this.selectedPortfolioIndex === 0) {
      this.selectedPortfolioIndex = 4;
    } else {
      this.selectedPortfolioIndex -= 1;
    }
    this.projectionGraphNav.select(this.selectedPortfolioIndex);
  },
  nextPortfolio: function(event, detail, sender) {
    if (this.selectedPortfolioIndex >= _.size(this.portfolios) - 1) {
      this.selectedPortfolioIndex = 0;
    } else {
      this.selectedPortfolioIndex += 1;
    }
    this.projectionGraphNav.select(this.selectedPortfolioIndex);
  },
  // Fires when the element was removed from the document
  detached: function() {},
  // Fires when an attribute was added, removed, or updated
  attributeChanged: function(attr, oldVal, newVal) {}
});
