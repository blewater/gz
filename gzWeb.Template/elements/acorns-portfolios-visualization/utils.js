/**
* TODO: write better documentation.
*/

/**
* isExisty
* @desc Check if object is defined.
* @param {*} - object
* @return {boolean}
*/
function isExisty(x) {
  return x !== null || x !== undefined;
}

/**
* toRad
* @desc degrees to radians
* @param {number} - degrees
* @return {number} - radians
*/
function toRad(deg) {
  return deg * (Math.PI / 180);
}

/**
* toDeg
* @desc radians to degrees
* @param {number} - radians
* @return {number} - degrees
*/
function toDeg(rad) {
  return rad * (180 / Math.PI);
}

/**
* comparator
* @desc comparator function for sorting method
* @param {function} predicate
* @return {number} - -1 if pred true, 1 if pred false, 0 if pred equal
*
*/
function comparator(pred) {
  return function (x, y) {
    if (pred(x, y)) {
      return -1;
    } else if (pred(y, x)) {
      return 1;
    } else {
      return 0;
    }
  };
}

/**
* isDateLessthan
* @desc
* @return
*/
var isDateLessThan = comparator(function (x, y) {
  return moment(x.date) < moment(y.date);
});

/**
* addCommas
* @desc
* @return
*/
function commafy(n) {
  if (!n) return n;
  var parts = n.toString().split('.');
  parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ',');
  return parts.join('.');
}

/**
* toCurrency
* @desc
* @param opts {object}
* @param opts.decimal
* @param opts.symbol
* @param opts.sign
* @return
*/
function toCurrency(val, opts) {
  val = val || 0;
  opts = opts || {};
  var defaults = {
    symbol: true,
    decimal: true ,
    sign: false
  };
  opts = _.extend(defaults, opts);
  var t = [];
  if (opts.sign) {
    if (val !== 0) {
      t.push(val < 0 ? '-' : '+');
    }
    val = Math.abs(val);
  }
  if (opts.symbol) {
    t.push('$');
  }
  if (opts.decimal) {
    val = val.toFixed(2);
  } else {
    val = val|0;
  }
  t.push(commafy(val));
  return t.join('');
}

/**
* copy
* @desc
* @return
*/
function copy(a) {
  return _.map(a, _.clone);
}

/**
* randomDate
* @desc
* @return
*/
function randomDate(start, end) {
  return new Date(start.getTime() + Math.random() * (end.getTime() - start.getTime()));
}

/**
* getSlopeAngle
* @desc
* @return
*/
function slopeAngle(s1,s2) {
  return Math.atan((s2[1] - s1[1]) / (s2[0] - s1[0])) * 180/Math.PI;
}

/**
* percent
* @desc
* @return
*/
function percent(n,p) {
  return (n * (p/100));
}

/**
* minusPercent
* @desc
* @return
*/
function minusPercent(n,p) {
  return n - percent(n,p);
}

/**
* plusPercent
* @desc
* @return
*/
function plusPercent(n,p) {
  return n + percent(n,p);
}

/**
* slugify
* @desc
* @return
*/
function slugify(str) {
  str = str.replace(/^\s+|\s+$/g, ''); // trim
  str = str.toLowerCase();

  var from = "ãàáäâẽèéëêìíïîõòóöôùúüûñç·/_,:;";
  var to   = "aaaaaeeeeeiiiiooooouuuunc------";
  for (var i=0, l=from.length ; i<l ; i++) {
    str = str.replace(new RegExp(from.charAt(i), 'g'), to.charAt(i));
  }

  str = str.replace(/[^a-z0-9 -]/g, '')
  .replace(/\s+/g, '-')
  .replace(/-+/g, '-');

  return str;
}

/**
* result
* @desc
* @return
*/
function result(o) {
  return _.isFunction(o) ? o() : o;
}

/**
* detectIE
* @desc
* @return
*/
function detectIE() {
  var ua = window.navigator.userAgent;

  var msie = ua.indexOf('MSIE ');
  if (msie > 0) {
    // IE 10
    return parseInt(ua.substring(msie + 5, ua.indexOf('.', msie)), 10);
  }

  var trident = ua.indexOf('Trident/');
  if (trident > 0) {
    // IE 11
    var rv = ua.indexOf('rv:');
    return parseInt(ua.substring(rv + 3, ua.indexOf('.', rv)), 10);
  }

  var edge = ua.indexOf('Edge/');
  if (edge > 0) {
   // IE 12
   return parseInt(ua.substring(edge + 5, ua.indexOf('.', edge)), 10);
  }

  return false;
}
