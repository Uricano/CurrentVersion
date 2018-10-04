(function (angular)
{
    angular.module('takinApp.common').directive('tfiInputContainer', createDirective);

    createDirective.$inject = [];

    function createDirective()
    {
        return {
            restrict: 'E',
            template: '<ng-transclude></ng-transclude>',
            transclude: true,
            compile: compileDirective
        };

        function compileDirective(tElement)
        {
            tElement.addClass('input-container');
            return linkDirective;
        }

        function linkDirective(scope, element, attrs)
        {
            var prefix = attrs['tfiLeft'];

            if (prefix)
            {
                var prefixHolder = angular.element('<span>');
                prefixHolder.addClass('input-container__prefix');
                prefixHolder.text(prefix);
                element.prepend(prefixHolder);

                var width = prefixHolder.width() + 5;
                
                var inputElement = element.find('input');
                inputElement.css('padding-left', width);
            }
        }
    }

})(angular);