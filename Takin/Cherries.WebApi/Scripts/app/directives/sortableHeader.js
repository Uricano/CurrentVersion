app.directive('sortableHeader', ['$timeout', function ($timeout, $window) {

    return {
        restrict: 'A',
        scope: {
            model: '=',
            sort: '&',
            readOnly:'='
        },
        link: function (scope, element, attrs) {
            $timeout(function () {
                var $sortable = element.find('.sortable');
                $sortable.on('click', function () {
                    if (scope.readOnly) return;
                    var asc = $(this).hasClass('asc');
                    var desc = $(this).hasClass('desc');
                    $sortable.removeClass('asc').removeClass('desc');
                    scope.model.field = $(this).attr('field');
                    if (desc || (!asc && !desc)) {
                        $(this).addClass('asc');
                        scope.model.direction = 'asc';
                    } else {
                        $(this).addClass('desc');
                        scope.model.direction = 'desc';
                    }
                    scope.sort();
                });
            }, 0);
        }
    }
}]);