app.directive('focus',['$timeout', function ($timeout) {
    return function (scope, element, attrs) {
        $timeout(function () {
            if (attrs.focusme)
                element[0].focus();
        },500);
    }
}]);