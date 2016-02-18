(function(root, opts) {
  'use strict';

  root.Acorns = root.Acorns || {};
  opts = opts || {};
  var filePath = opts.filePath;

  /**
   * Fetch coefficients data
   */

  var request = new XMLHttpRequest();
  request.open('GET', filePath, true);

  request.onload = function() {
    if (request.status >= 200 && request.status < 400){
      Acorns.coefficientData = JSON.parse(request.responseText);
      var event;
      if (detectIE()) {
        event = document.createEvent('Event');
        event.initEvent('coefficientDataLoaded', true, false);
        window.dispatchEvent(event);
      } else {
        event = new Event('coefficientDataLoaded');
        window.dispatchEvent(event);
      }
    } else {
      console.error(request);
    }
  };

  request.onerror = function() {
    console.error(arguments);
  };

  window.onload = request.send();

  function getPortfolioKey(key) {
      var portfolio = [
       'conservative',
       'moderately-conservative',
       'moderate',
       'moderately-aggressive',
       'aggressive'
      ];

    return portfolio.indexOf(key) > -1 ? key : portfolio[2];
  }

  function getStartingValueKey(value) {
    value = parseFloat(value);
    if (value >= 1 && value < 100) {
      return 1;
    } else if (value >= 100 && value <= 10000) {
      return 100;
    } else if (value > 10000) {
      return 10000;
    } else {
      return 1;
    }
  }

  function getMonthlyInvestmentIndexKey(value) {
    value = parseFloat(value);
    if (value === 0) {
      return 0;
    } else if (value === 1) {
      return 1;
    } else if (value >= 2 && value <= 100) {
      return 2;
    } else if (value > 100) {
      return 3;
    } else {
      return 0;
    }
  }

  function getCoefficients(portfolioType, startingValue, savingsRate) {
    var data = Acorns.coefficientData;

    var base = [getPortfolioKey(portfolioType),
           getStartingValueKey(startingValue),
           getMonthlyInvestmentIndexKey(savingsRate),
           'coefficients'].join('-');

    return data[base];
  }

  function getLinesFromPortfolio(savingsRate, startingValue, portfolioType) {
    var multipleSeparator = ':';
    var projectionLines = [];

    var lines = getCoefficients(portfolioType, startingValue, savingsRate);

    for (var i = 0, lLen = lines.length; i < lLen; i++) {
      var row = lines[i]; // thisLine in Java app

      var blocks = [];

      for (var columnName in row) if (row.hasOwnProperty(columnName)) {
        var columnValue = row[columnName];

        var monthPower = 0;
        var savingsRatePower = 0;
        var startingValuePower = 0;

        // Skip non-coefficient columns
        if ((/index/gi.test(columnName) || /type/gi.test(columnName))) continue;

        // headerColumnsSections in Java app
        var multiples = columnName.split(multipleSeparator);

        for (var j = 0, mLen = multiples.length; j < mLen; j++) {
          var multiple = multiples[j];
          var power = getPowerForCoefficientName(multiple);
          var coeffName = getNameForCoefficients(multiple);

          if (coeffName === 'month') {
            monthPower = power;
          } else if (coeffName === 'savingsRate') {
            savingsRatePower = power;
          } else if (coeffName === 'startingValue') {
            startingValuePower = power;
          }
        }

        var coeffValue = parseFloat(columnValue);
        var coeffBlock = new CoefficientBlock(coeffValue, monthPower, startingValuePower, savingsRatePower);
        blocks.push(coeffBlock);
      }

      var intercept = row['(Intercept)'];
      var projectionType = row.type;
      projectionLines.push(new ProjectionLine(blocks, intercept, projectionType));
    }

    return projectionLines;
  }

  function ProjectionLine(blocks, intercept, projectionType) {
    this.coefficientBlocks = blocks;
    this.intercept = intercept;
    this.projectionType = projectionType;
  }

  ProjectionLine.prototype = {
    evaluateValueAtMonth: function(month, startingValue, savingsRate) {
      var self = this;
      var value = 0;
      for (var i = 0, len = self.coefficientBlocks.length; i < len; i++) {
        var block = self.coefficientBlocks[i];
        var d = block.evaluateCoefficientBlock(month, startingValue, savingsRate);
        value += d;
      }
      if (value < 0) return 0;
      return value;
    },
    getProjectionLineType: function() {
      return this.projectionType;
    }
  };

  function CoefficientBlock(coeffValue, monthPower, startingValuePower, savingsRatePower) {
    this.coeffValue = coeffValue;
    this.monthPower = monthPower;
    this.startingValuePower = startingValuePower;
    this.savingsRatePower = savingsRatePower;

    var self = this;
  }

  CoefficientBlock.prototype = {
    evaluateCoefficientBlock: function(month, startingValue, savingsRate) {
      var self = this;
      var value = parseFloat(
        self.coeffValue *
        Math.pow(month, self.monthPower) *
        Math.pow(startingValue, self.startingValuePower) *
        Math.pow(savingsRate, self.savingsRatePower)
      );
      return value;
    }
  };

  function getPowerForCoefficientName(s) {
    if (typeof s === 'undefined' || s === null) return 0;
    if (s === '') return 0;
    if (s.indexOf('^') === -1) return 1;
    return parseFloat(s.substring(s.indexOf('^') + 1));
  }

  function getNameForCoefficients(s) {
    if (typeof s === 'undefined' || s === null) return '';
    if (s === '') return '';
    if (s.indexOf('^') === -1) return s;
    return s.substring(0, s.indexOf('^'));
  }

  function getPortfolioLinesFromPortfolio(savingsRate, startingValue, portfolioType) {
    var projectionLines = Acorns.getLinesFromPorfolio(savingsRate, startingValue, portfolioType);

    var LB68, UB68, LB95, UB95, LB99, UB99, Mean;
    var nMonths = 240;

    for (var i = 0, pLen = projectionLines.length; i < pLen; i++) {
      var projectionLine = projectionLines[i];
      var floats = [];
      for (var j = 0; j < nMonths; j++) {
        floats[j] = projectionLine.evaluateValueAtMonth(j, startingValue, savingsRate);

        switch(projectionLine.getProjectionLineType()) {
          case '68lb': LB68 = floats; break;
          case '68ub': UB68 = floats; break;
          case '95lb': LB95 = floats; break;
          case '95ub': UB95 = floats; break;
          case '99lb': LB99 = floats; break;
          case '99ub': UB99 = floats; break;
          case 'mean': Mean = floats; break;
        }
      }
    }

    return {
      LB68: LB68,
      UB68: UB68,
      LB95: LB95,
      UB95: UB95,
      LB99: LB99,
      UB99: UB99,
      Mean: Mean
    };
  }

  Acorns.getLinesFromPorfolio = getLinesFromPortfolio;
  Acorns.getPortfolioLinesFromPortfolio = getPortfolioLinesFromPortfolio;

})(this, {
  filePath: 'elements/acorns-portfolios-visualization/assets/json/coefficients.json'
});
