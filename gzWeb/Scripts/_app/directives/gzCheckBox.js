(function () {
    'use strict';

    APP.directive('gzCheckBox', ['helpers', directiveFactory]);

    function directiveFactory(helpers) {
        return {
            restrict: 'E',
            require: '^form',
            scope: {
                gzModel: '=',
                gzValue: '@',
                gzDisabled: '&',
                gzCheck: '&',
                gzUncheck: '&',
            },
            replace: true,
            transclude: true,
            templateUrl: function () {
                return helpers.ui.getTemplate('partials/directives/gzCheckBox.html');
            },
            link: function (scope, element, attrs, form) {
                scope.form = form;
            },
            controller: ['$scope', '$element', '$attrs', function ($scope, $element, $attrs) {
                $scope.getValue = function () {
                    return helpers.reflection.hasValue($scope.gzModel)
                           ? helpers.reflection.hasValue($scope.gzValue) ? $scope.gzModel[$scope.gzValue] : $scope.gzModel
                           : false;
                };
                $scope.setValue = function (value) {
                    if (helpers.reflection.hasValue($scope.gzValue))
                        $scope.gzModel[$scope.gzValue] = value;
                    else
                        $scope.gzModel = value;
                    $scope.form.$setDirty();
                };
                $scope.onClick = function () {
                    var isDisabled = $scope.gzDisabled();
                    if (!helpers.reflection.hasValue(isDisabled) || !isDisabled) {
                        $scope.setValue(!$scope.getValue());
                        if (helpers.reflection.hasValue($scope.gzValue)) {
                            if (angular.isFunction($scope.gzCheck) && $scope.getValue())
                                $scope.gzCheck({ model: $scope.gzModel });
                            if (angular.isFunction($scope.gzUncheck) && !$scope.getValue())
                                $scope.gzUncheck({ model: $scope.gzModel });                            
                        }
                    }
                };
                $scope.model = $scope.getValue();
            }]
        };
    }
})();