(function(root) {
  /**
  * NavLabels
  */
  function NavLabels(opts) {
    var defaults = {
      shadowDOM: null,
      el: null,
      names: [],
      selected: 0,
      onSelect: _.noop,
      onMouseOver: _.noop
    };

    _.defaults(this, opts, defaults);

    this.init();
  }

  /**
  * updateSize
  */
  NavLabels.prototype.updateSize = function() {
    this.width = $(this.shadowDOM.querySelectorAll(this.el)[0]).width();
    this.height = $(this.shadowDOM.querySelectorAll(this.el)[0]).height();
  };

  /**
  * updateContainer
  */
  NavLabels.prototype.updateContainer = function() {
    this.ul = this.ul || d3.select(this.shadowDOM.querySelectorAll(this.el)[0]).append('ul');

    this.ul.attr('class', 'labels-list');

  };

  /**
  * updateLabels
  */
  NavLabels.prototype.updateLabels = function() {
    var self = this;
    this.labels = this.labels || this.ul.selectAll('li')
    .data(_.range(this.names.length))
    .enter()
    .append('li')
    .attr('index', function(d,i) {
      return i;
    })
    .classed('active', function(d,i) {
      return this.selected == i;
    }.bind(this))
    .append('div')
    .text(function (index, text) {
      return self.names[index].name;
    });

  };

  /**
  * onMouseOver
  */
  // NavLabels.prototype.onMouseOver = function() {

  // };

  /**
  * onMouseOut
  */
  NavLabels.prototype.onMouseOut = function() {
    var c = d3.select(this);
    c
    .transition()
    .duration(200)
    .attr('r', c.attr('or'));
  };

  /**
  * onClick
  */
  NavLabels.prototype.onClick = function(el) {
    var c = d3.select(el);
    this.select(c.attr('index'));
  };

  /**
  * updateStyles
  */
  NavLabels.prototype.updateStyles = function() {
    var self = this;
    d3.selectAll(this.shadowDOM.querySelectorAll(this.el + ' li')).classed('active', function(d,i) {
      return self.selected == i;
    });
  };

  NavLabels.prototype.select = function(index) {
    this.selected = index;
    this.updateStyles();
    this.onSelect(index);
  };

  /**
  * init
  */
  NavLabels.prototype.init = function() {
    this.update();

    $(window).on('resize', this.resize.bind(this));

    var _this = this;

    d3.selectAll(_this.shadowDOM.querySelectorAll('li'))
      .on('mouseover', this.onMouseOver)
      .on('mouseout', this.onMouseOut)
      .on('click', function() {
        _this.onClick(this);
      });
  };

  /**
  * update
  */
  NavLabels.prototype.update = function() {
    this.updateSize();
    this.updateContainer();
    this.updateLabels();
  };

  /**
  * resize
  */
  NavLabels.prototype.resize = function() {
    this.update();
  };

  root.NavLabels = NavLabels;
})(this);
