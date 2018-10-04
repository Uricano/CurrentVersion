(function (angular)
{
    angular.module('takinApp.common').service('tfiMedia', createService);

    createService.$inject = ['$window', 'tfiMediaConstant'];

    function createService($window, tfiMediaConstant)
    {
        this.watchMedia = watchMedia;
        this.isSmallScreen = isSmallScreen;

        var listeners = {
            sm: []
        };

        var mediaQueryListSm = $window.matchMedia('(max-width: ' + tfiMediaConstant.sm + 'px)');
        mediaQueryListSm.addListener(mediaChangedSm);

        function watchMedia(breakpoint, listener) {
            var listenersArray;

            switch (breakpoint) {
                case tfiMediaConstant.sm:
                    listenersArray = listeners.sm;
                    break;
            }

            if (listenersArray) {
                listenersArray.push(listener);
                return function() {
                    var index = listenersArray.indexOf(listener);
                    if (index >= 0) {
                        listenersArray.splice(index, 1);
                    }
                }
            } else {
                return angular.noop;
            }
        }

        function mediaChangedSm() {
            for (var i = 0; i < listeners.sm.length; i++) {
                listeners.sm[i]();
            }
        }

        function isSmallScreen() {
            return mediaQueryListSm.matches;
        }
    }
})(angular);