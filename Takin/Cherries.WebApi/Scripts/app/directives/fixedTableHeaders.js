app.directive('fixedTableHeaders', ['$timeout', function ($timeout, $window) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            $timeout(function () {
                container = element.parentsUntil(attrs.fixedTableHeaders);
                element.stickyTableHeaders({ scrollableArea: container, "fixedOffset": 2 });
            }, 0);
        }
    }
}]);