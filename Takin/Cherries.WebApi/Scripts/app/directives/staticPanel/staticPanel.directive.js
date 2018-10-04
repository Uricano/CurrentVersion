(function (angular)
{
    angular.module('takinApp.common').directive('tfiStaticPanel', createDirective);

    createDirective.$inject = [];
    function createDirective()
    {
        return {
            restrict: 'EA',
            template: '<div class="static-panel__content" ng-transclude></div>',
            transclude: true,
            compile: compileDirective
        }

        function compileDirective(tElement)
        {
            tElement.addClass('static-panel');
            return linkDirective;
        }

        function linkDirective(scope, element, attrs)
        {
            var contentPanel = element.find('.static-panel__content');
        }
    }

})(angular);