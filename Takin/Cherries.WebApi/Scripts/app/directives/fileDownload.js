app.directive('fileBrowser', function () {
    return {
        restrict: 'A',
        replace: true,
        transclude: true,
        scope: false,
        template:
            '<div class="input-prepend extended-date-picker">' +
                '<input type="button" class="btn" value="טען מסמך" style="margin-left: 40px;">' +
                '<input type="text" readonly class="override" style="min-width: 400px;text-align: left;">' +
                '<div class="proxied-field-wrap" ng-transclude></div>' +
            '</div>',
        link: function ($scope, $element, $attrs, $controller) {
            var button, fileField, proxy;
            fileField = $element.find('[type="file"]').on('change', function () {
                proxy.val(angular.element(this).val());
                $scope.$emit('SetFileName', angular.element(this)[0])
            });
            proxy = $element.find('[type="text"]').on('click', function () {
                fileField.trigger('click');
            });
            button = $element.find('[type="button"]').on('click', function () {
                fileField.trigger('click');
            });
        }
    };
});