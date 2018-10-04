(function (angular)
{
    angular.module('takinApp.common').directive('tfiFooter', createDirective);

    createDirective.$inject = [];

    function createDirective()
    {
        return {
            restrict: 'E',
            template: '' +
                '<div class="footer__links">' +
                    '<div class="footer__link">' +
                        '<a href="https://www.gocherries.com/terms-conditions" target="_blank">Terms & Conditions</a>' +
                    '</div>' +
                    '<div class="footer__separator"></div>' +
                    '<div class="footer__link">' +
                        '<a href="https://www.gocherries.com/privacy-policy" target="_blank">Privacy Policy</a>' +
                    '</div>' +
                    '<div class="footer__separator"></div>' +
                    '<div class="footer__link">' +
                        '<a href="https://support.gocherries.com/en/" target="_blank">Help Center</a>' +
                    '</div>' +
                '</div>' +
                '<div layout="row" layout-align="center center">' +
                    '<div class="footer__separator" show-gt-sm></div>' +
                    '<div class="l-inline-block">&copy;2018 by Cherries</div>' +
                '</div>',
            compile: function (tElement)
            {
                tElement.addClass('footer');
            }
        };
    }

})(angular);