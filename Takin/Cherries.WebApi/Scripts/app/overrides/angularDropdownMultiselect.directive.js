//
// Overrides for ngDropdownMultiselect directive
//

(function (angular)
{
    angular.module('takinApp.common').directive('ngDropdownMultiselect', createDirective);

    createDirective.$inject = ['$parse'];
    function createDirective($parse)
    {
        return {
            restrict: 'AE',
            link: linkDirective,
            priority: 10
        }

        function linkDirective(scope, element, attrs)
        {
            var onCloseFn;
            if (attrs.tfiOnClose)
            {
                onCloseFn = $parse(attrs.tfiOnClose)(scope);
            }

            var isolateScope = element.isolateScope();
            isolateScope.open = false;
            isolateScope.$watch('open', function (value)
            {
                if (value == false && onCloseFn)
                {
                    onCloseFn();
                }
            });
        }
    }

})(angular);