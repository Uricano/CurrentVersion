(function (angular)
{
    angular.module('takinApp.common').directive('tfiLinkButton', createDirective);

    createDirective.$inject = [];

    function createDirective()
    {
        return {
            restrict: 'A',
            compile: compileDirective
        };

        function compileDirective(tElement)
        {
            tElement.addClass('link-button');

            var children = tElement.children();
            if (children.length == 1)
            {
                if (children[0].tagName == 'IMG')
                {
                    children.addClass('icon');
                }
            }

            return linkDirective;
        }

        function linkDirective(scope, element, attrs)
        {
            var disabled = false;

            if (angular.isUndefined(attrs.href))
            {
                attrs.$set('href', '');
            }

            attrs.$observe('disabled', function (isDisabled)
            {
                disabled = isDisabled;
                if (disabled)
                {
                    element.addClass('is-disabled');
                }
                else
                {
                    element.removeClass('is-disabled');
                }
            });

            element.on('click', function (e)
            {
                if (disabled)
                {
                    e.preventDefault();
                    e.stopImmediatePropagation();
                }
            });
        }
    }

})(angular);