(function (angular)
{
    angular.module('takinApp.common')
        .directive('tfiTablePaginator', createDirective)
        .controller('TablePaginatorController', TablePaginatorController);

    var POPOVER_STATE_HIDDEN = 0;
    var POPOVER_STATE_SHOWN = 1;
    var TOGGLE_POPOVER_INTERVAL = 1000;

    createDirective.$inject = [];
    function createDirective()
    {
        return {
            restrict: 'E',
            template: '' +
                '<div>' +
                    '<a tfi-link-button ng-click="ctrl.first()" ng-disabled="ctrl.isPrevDisabled()" class="table-paginator__link">' +
                        '<img src="Content/themes/images/first-page.png" class="table-paginator__icon" />' +
                    '</a>' +
                    '<a tfi-link-button ng-click="ctrl.prev()" ng-disabled="ctrl.isPrevDisabled()" class="table-paginator__link">' +
                        '<img src="Content/themes/images/prev-page.png" class="table-paginator__icon" />' +
                    '</a>' +
                '</div>' +
                '<div>' +
                    '<a tfi-link-button ng-click="ctrl.next()" ng-disabled="ctrl.isNextDisabled()" class="table-paginator__link">' +
                        '<img src="Content/themes/images/next-page.png" class="table-paginator__icon" />' +
                    '</a>' +
                    '<a tfi-link-button ng-click="ctrl.last()" ng-disabled="ctrl.isNextDisabled()" class="table-paginator__link">' +
                        '<img src="Content/themes/images/last-page.png" class="table-paginator__icon" />' +
                    '</a>' +
                '</div>',
            controller: 'TablePaginatorController',
            controllerAs: 'ctrl',
            bindToController: true,
            require: 'tfiPopover',
            scope: {
                params: '<tfiTableParams',
            }
        }
    }

    TablePaginatorController.$inject = ['$scope', '$element', '$attrs'];
    function TablePaginatorController($scope, $element, $attrs)
    {
        this.first = first;
        this.prev = prev;
        this.next = next;
        this.last = last;

        this.isPrevDisabled = isPrevDisabled;
        this.isNextDisabled = isNextDisabled;

        this.$onInit = $onInit;
        this.isDisabled = false;

        var self = this;

        function first()
        {
            self.params.page(1);
        }

        function prev()
        {
            self.params.page(self.params.page() - 1);
        }

        function next()
        {
            self.params.page(self.params.page() + 1);
        }

        function last()
        {
            var pagesCount = Math.ceil(self.params.total() / self.params.count());
            self.params.page(pagesCount);
        }

        function isPrevDisabled()
        {
            return self.params ? self.params.page() == 1 : true;
        }

        function isNextDisabled()
        {
            if (!self.params)
            {
                return true;
            }

            var pagesCount = Math.ceil(self.params.total() / self.params.count());
            return self.params.page() >= pagesCount;
        }

        function $onInit()
        {
            $element.addClass('table-paginator');
            $attrs.$observe('disabled', function (disabled)
            {
                self.isDisabled = disabled;
            });
        }
    }

})(angular);