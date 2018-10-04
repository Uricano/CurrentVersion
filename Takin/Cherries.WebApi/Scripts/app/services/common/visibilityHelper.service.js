(function (angular)
{
    angular.module('takinApp.common').service('visibilityHelper', createService);

    createService.$inject = ['$window', '$document', 'underscore'];

    function createService($window, $document, underscore)
    {
        this.register = register;

        var spies = [];
        var windowElement = angular.element($window);
        var unbindEvents = null;
        var body = $document[0].body;

        function register(spy)
        {
            if (spies.length == 0)
            {
                unbindEvents = bindEvents();
            }
            spies.push(spy);

            // Returns unregister function.
            return function ()
            {
                var spyIndex = spies.indexOf(spy);
                if (spyIndex >= 0)
                {
                    spies.splice(spyIndex, 1);
                    if (spies.length == 0 && unbindEvents)
                    {
                        unbindEvents();
                        unbindEvents = null;
                    }
                }
            }
        }

        function bindEvents()
        {
            var debouncedWindowResize = underscore.debounce(windowOnResize, 100);
            var debouncedWindowScroll = underscore.debounce(windowOnScroll, 100);

            windowElement
                .bind('resize', debouncedWindowResize)
                .bind('scroll', debouncedWindowScroll);

            return function ()
            {
                windowElement
                    .unbind('resize', debouncedWindowResize)
                    .unbind('scroll', debouncedWindowScroll);
            }
        }

        function windowOnResize()
        {
            recalculateElementsVisibility();
        }

        function windowOnScroll()
        {
            recalculateElementsVisibility();
        }

        function recalculateElementsVisibility()
        {
            var screenRect = body.getBoundingClientRect();
            var screenTop = screenRect.top;
            var screenBottom = screenRect.height;

            for (var i = 0; i < spies.length; i++)
            {
                var spy = spies[i];
                var elPos = getElementPosition(spy.element);
                var isVisible = !(elPos.top > screenBottom || (elPos.bottom < 0));
                spy.onVisibilityChange(isVisible);
            }
        }

        function getElementPosition(el)
        {
            var domElement = (el.nodeType === Node.ELEMENT_NODE) ? el : el[0];
            return domElement.getBoundingClientRect();
        }
    }
})(angular);