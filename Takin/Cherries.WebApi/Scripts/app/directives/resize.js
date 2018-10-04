app.directive('resize', ['$window', '$rootScope', function ($window, $rootScope)
{
    return {
        restrict: 'A',
        link: function (scope, element)
        {
            var w = angular.element($window);
            scope.getWindowDimensions = function ()
            {
                return {
                    'h': w.height(),
                    'w': w.width()
                };
            };
            scope.$watch(scope.getWindowDimensions, function (newValue, oldValue)
            {
                scope.windowHeight = newValue.h;
                scope.windowWidth = newValue.w;
                setTimeout(function ()
                {
                    $rootScope.$broadcast('resizeWindow', newValue.w);
                }, 100);
            }, true);
            w.bind('resize', function ()
            {
                scope.$apply();
            });
        }
    };
}]);