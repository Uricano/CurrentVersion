(function (angular)
{
    angular.module('takinApp.common').directive('tfiResizableContainer', createDirective);

    createDirective.$inject = [];

    function createDirective()
    {
        return {
            restrict: 'A',
            require: ['tfiResizableContainer', '^^?tfiResizableContainer'],
            controller: ResizableContainerController,
            link: linkDirective
        }

        function linkDirective(scope, element, attrs, ctrls)
        {
            var currentCtrl = ctrls[0];
            var parentCtrl = ctrls[1];

            currentCtrl.init(parentCtrl);
        }
    }

    ResizableContainerController.$inject = ['$scope', '$window'];

    function ResizableContainerController($scope, $window)
    {
        this.init = init;
        this.addListener = addListener;
        this.notifyResize = notifyResize;

        var listeners = [];
        var registerParentResizeListener = null;
        var unregisterParentResizeListener = null;

        $scope.$on('$destroy', destroy);

        function init(parentContainer)
        {
            var listener = function ()
            {
                notifyResize();
            }

            if (parentContainer != null)
            {
                registerParentResizeListener = function ()
                {
                    return parentContainer.addListener(listener)
                }
            }
            else
            {
                registerParentResizeListener = function ()
                {
                    var windowElement = angular.element($window);
                    windowElement.on('resize', listener);
                    return function ()
                    {
                        windowElement.off('resize', listener);
                    }
                }
            }

            updateSubscriptions();
        }

        function addListener(listener)
        {
            listeners.push(listener);
            updateSubscriptions();
            var unregister = function ()
            {
                var index = listeners.indexOf(listener);
                if (index >= 0)
                {
                    listeners.splice(index, 1);
                    updateSubscriptions();
                }
            }
            return unregister;
        }

        function notifyResize()
        {
            for (var i = 0; i < listeners.length; i++)
            {
                listeners[i]();
            }
        }

        function updateSubscriptions()
        {
            if (registerParentResizeListener == null)
            {
                return;
            }

            if (unregisterParentResizeListener != null)
            {
                if (listeners.length == 0)
                {
                    unregisterParentResizeListener();
                    unregisterParentResizeListener = null;
                }
            }
            else if (listeners.length > 0)
            {
                unregisterParentResizeListener = registerParentResizeListener();
            }
        }

        function destroy()
        {
            if (unregisterParentResizeListener)
            {
                unregisterParentResizeListener();
            }
            listeners.length = 0;
        }
    }

})(angular);