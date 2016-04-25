(function () {
    'use strict';

    APP.directive('gzSelect', ['helpers', '$timeout', 'constants', directiveFactory]);

    function directiveFactory(helpers, $timeout, constants) {
        return {
            restrict: 'E',
            scope: {
                gzName: '@',
                gzModel: '=',
                gzCollection: '=',
                gzId: '@',
                gzDisplay: '@',
                gzPrompt: '@',
                gzChange: '&',
                gzDisabled: '&',
                gzLoading: '&',
                gzError: '&'
            },
            replace: true,
            templateUrl: function() {
                return helpers.ui.getTemplate('partials/directives/gzSelect.html');
            },
            controller: ['$scope', '$element', '$attrs', function ($scope, $element, $attrs) {
                $scope.spinnerOptions = { radius: 5, width: 2, length: 4, color: '#27A95C', position: 'absolute', top: '50%' };

                $scope.getId = function (item) {
                    return item && helpers.reflection.hasValue($scope.gzId) ? item[$scope.gzId] : item;
                };
                $scope.getDisplay = function (item) {
                    return item && helpers.reflection.hasValue($scope.gzDisplay) ? item[$scope.gzDisplay] : item;
                };
                $scope.onSelectionChange = function () {
                    $scope.gzChange({ id: helpers.reflection.hasValue($scope.gzModel) ? $scope.getId($scope.gzModel) : undefined });
                };
                $scope.isRequired = 'gzRequired' in $attrs;
                $scope.hasPrompt = !('gzNoPrompt' in $attrs);
                $scope.isDisabled = function () {
                    return $scope.gzDisabled() || $scope.gzLoading();
                };
                if (!$scope.gzPrompt)
                    $timeout(function () { $scope.gzPrompt = '---Please select---'; }, 0);

                if (!$scope.hasPrompt) {
                    var unregisterCollectionWatch = $scope.$watchCollection('gzCollection', function (newCollection, oldCollection) {
                        if (!$scope.gzModel && newCollection.length > 0) {
                            $scope.gzModel = newCollection[0];
                            unregisterCollectionWatch();
                        }
                    });
                }
            }]
        };
    }
})();