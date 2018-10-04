(function (angular)
{
    angular.module('takinApp.common')
        .directive('tfiPopover', createDirective)
        .directive('tfiPopoverApi', createDirectiveApi)
        .controller('PopoverController', PopoverController);

    var POPOVER_STATE_HIDDEN = 0;
    var POPOVER_STATE_SHOWN = 1;
    var POPOVER_STATE_ANIMATING = 2;
    var TOGGLE_POPOVER_INTERVAL = 1000;

    createDirective.$inject = [];
    function createDirective()
    {
        return {
            restrict: 'E',
            template: '<div class="tfi-popover__content"></div>',
            controller: 'PopoverController',
            controllerAs: 'ctrl',
            bindToController: true,
            require: 'tfiPopover',
            scope: {
                onSubmit: '&?tfiSubmit',
                onClose: '&?tfiClose'
            },
            transclude: true,
            link: linkDirective
        }

        function linkDirective(scope, element, attrs, ctrl, transcludeFn)
        {
            ctrl.transcludeFn = transcludeFn;
        }
    }

    PopoverController.$inject = ['$scope', '$element', '$attrs', '$$rAF', '$window', '$document', '$animate', '$mdUtil', 'underscore', 'keyCode'];
    function PopoverController($scope, $element, $attrs, $$rAF, $window, $document, $animate, $mdUtil, underscore, keyCode)
    {
        this.$onInit = $onInit;
        this.$postLink = $postLink;
        this.$onDesrtoy = $onDesrtoy;
        this.publicApi = {
            show: showPopover,
            hide: hidePopover
        };
        this.isDisabled = false;

        var self = this;
        var _isPopoverState;
        var _isDestroyed;
        var _popoverContentElement = null;
        var _popoverContentCleanupFn;
        var _triggerElement;
        var _windowElement;
        var _windowCleanupFn;
        var _elementCleanupFn;
        var _preventPopoverHiding;
        var _containerElement;
        var restoreScrolling;
        var _backdrop;
        var _triggerCleanupFn = null;

        function $onInit()
        {
            _isPopoverState = POPOVER_STATE_HIDDEN;
            _isDestroyed = false;

            $element.addClass('tfi-popover');
            // Make the element focusable
            $attrs.$set('tabindex', '-1');

            $attrs.$observe('disabled', function (disabled)
            {
                self.isDisabled = disabled;
            });
        }

        function $postLink()
        {
            // If the variable has a value then it is a function to cleanup the content of the popover
            _popoverContentElement = $element.children('.tfi-popover__content');
            _popoverContentCleanupFn = null;
            
            // Init trigger 
            _triggerElement = $element.parent();
            _triggerCleanupFn = initTrigger();

            // Init window
            _windowElement = angular.element($window);
            _windowCleanupFn = null;

            _elementCleanupFn = null
            _preventPopoverHiding = false;

            // Contains a container where the popover is attached
            _containerElement = null;

            restoreScrolling = null;
            // TODO:
            _backdrop = null;
            
            // Detach the element from the DOM tree
            $element.detach();

            function initTrigger()
            {
                var togglePopover = underscore.debounce(function ()
                {
                    $scope.$apply(triggerOnClick);
                }, TOGGLE_POPOVER_INTERVAL, true);

                _triggerElement.bind('click', function (event)
                {
                    togglePopover();

                    event.stopPropagation();
                    event.preventDefault();
                });

                _triggerElement.bind('$destroy', cleanup);

                return function cleanupTrigger()
                {
                    _triggerElement.unbind('click', triggerOnClick);
                    _triggerElement.unbind('$destroy', cleanup);
                }

                function triggerOnClick()
                {
                    if (_isDestroyed)
                    {
                        return;
                    }
                    if (_triggerElement.is('[disabled]'))
                    {
                        return;
                    }
                    if (_isPopoverState == POPOVER_STATE_SHOWN)
                    {
                        hidePopover();
                    }
                    else if (_isPopoverState == POPOVER_STATE_HIDDEN)
                    {
                        showPopover();
                    }
                }
            }
        }

        function showPopover()
        {
            if (_isPopoverState != POPOVER_STATE_HIDDEN)
            {
                return;
            }

            if (_containerElement == null)
            {
                _containerElement = $document.find('body');
            }

            _isPopoverState = POPOVER_STATE_ANIMATING;

            // Subscribe to the window events
            _windowCleanupFn = initWindowEvents();

            // Init popover content.
            _popoverContentCleanupFn = insertPopoverContent();

            // Init element events
            _elementCleanupFn = initElementEvents();

            restoreScrolling = disableBodyScrolling();

            // Create a backdrop
            _backdrop = $mdUtil.createBackdrop($scope, "md-backdrop--popover md-click-catcher");
            _containerElement.append(_backdrop);

            // Add the element to DOM, set its position and only then after rendering start
            // animation. Otherwise if position of the trigger element has changed since the time 
            // when the popover was displyed last time, changing popover position will be animated.
            _containerElement.append($element);
            setPopoverPosition();
            $element.focus();
            $animate
                .addClass($element, 'md-show')
                .finally(function () { _isPopoverState = POPOVER_STATE_SHOWN; });

            function insertPopoverContent()
            {
                var contentScope = null;
                var isContentCleanedUp = false;

                showResult();

                function showResult()
                {
                    self.transcludeFn(function (clonedElement, clonedScope)
                    {
                        contentScope = clonedScope;
                        _popoverContentElement.append(clonedElement);

                        // Popover height may be changed, recalucate the position.
                        $mdUtil.nextTick(function () { setPopoverPosition(); });
                    });
                }

                function ensureContentEmpty()
                {
                    if (contentScope != null)
                    {
                        contentScope.$destroy();
                        contentScope = null;
                    }
                    _popoverContentElement.empty();
                }

                return function cleanupContent()
                {
                    ensureContentEmpty();
                    isContentCleanedUp = true;
                }
            }

            function initElementEvents()
            {
                $element
                    .bind('click', elementOnClick)
                    .bind('keydown', elementOnKeyDown);

                function elementOnClick()
                {
                    _preventPopoverHiding = true;
                }

                function elementOnKeyDown(event)
                {
                    if (event.which === keyCode.ESCAPE)
                    {
                        $scope.$apply(hidePopover);
                        event.stopPropagation();
                        _triggerElement.focus();
                    }
                }

                return function clenupElementEvents()
                {
                    $element
                        .unbind('click', elementOnClick)
                        .unbind('keydown', elementOnKeyDown);
                };
            }

            function initWindowEvents()
            {
                var debouncedWindowResized = $$rAF.throttle(windowOnResize);
                _windowElement
                    .bind('click', windowOnClick)
                    .bind('resize', debouncedWindowResized)
                    .bind('scroll', windowOnScroll);

                return function cleanupWindow()
                {
                    _windowElement
                        .unbind('click', windowOnClick)
                        .unbind('resize', debouncedWindowResized)
                        .unbind('scroll', windowOnScroll);
                }

                function windowOnClick(event)
                {
                    if (event.target === _triggerElement[0] || event.target === $element[0] || _preventPopoverHiding)
                    {
                        _preventPopoverHiding = false;
                        return;
                    }
                    hidePopover();
                    $scope.$digest();
                }

                function windowOnResize()
                {
                    if (_isPopoverState)
                    {
                        setPopoverPosition();
                    }
                }

                function windowOnScroll()
                {
                    hidePopover();
                    $scope.$digest();
                }
            }

            function disableBodyScrolling()
            {
                var body = angular.element($document[0].body);
                var currentOverflow = body.css('overflow');
                body.css({ overflow: 'hidden' });

                return function ()
                {
                    body.css({ overflow: currentOverflow });
                }
            }
        }

        function hidePopover()
        {
            if (_isPopoverState != POPOVER_STATE_SHOWN)
            {
                return;
            }

            if (_backdrop)
            {
                _backdrop.remove();
                _backdrop = null;
            }

            if (restoreScrolling)
            {
                restoreScrolling();
                restoreScrolling = null;
            }

            $animate
                .removeClass($element, 'md-show')
                .then(function ()
                {
                    // Cleanup the content
                    if (_popoverContentCleanupFn != null)
                    {
                        _popoverContentCleanupFn();
                        _popoverContentCleanupFn = null;
                    }

                    // Unsubscribe from the window events
                    if (_windowCleanupFn != null)
                    {
                        _windowCleanupFn();
                        _windowCleanupFn = null;
                    }

                    // Unsubscribe element events
                    if (_elementCleanupFn != null)
                    {
                        _elementCleanupFn();
                        _elementCleanupFn = null;
                    }

                    // Remove the popover if it is one-time popover, otherwise just detach.
                    $element.detach();
                })
                .finally(function ()
                {
                    _isPopoverState = POPOVER_STATE_HIDDEN;
                });
        }

        function setPopoverPosition()
        {
            $element.position({
                my: 'left top',
                at: 'left top',
                of: _triggerElement,
                collision: 'fit'
            });
        }

        function $onDesrtoy()
        {
            cleanup();
        }

        function cleanup()
        {
            _isDestroyed = true;

            if (_backdrop)
            {
                _backdrop.remove();
                _backdrop = null;
            }
            if (restoreScrolling)
            {
                restoreScrolling();
                restoreScrolling = null;
            }
            _triggerCleanupFn();
            if (_windowCleanupFn != null)
            {
                _windowCleanupFn();
                _windowCleanupFn = null;
            }

            $element.remove();
        }
    }

    createDirectiveApi.$inject = ['directiveApi'];
    function createDirectiveApi(directiveApi)
    {
        return directiveApi('tfiPopover');
    }

})(angular);