(function (angular)
{
    angular.module('takinApp.common')
        .component('tfiSecurityFilter', {
            template:
                '<div class="security-filter__icon">' +
                    '<tfi-popover tfi-popover-api="$ctrl.popoverApi" ng-disabled="$ctrl.isDisabled">' +
                        '<tfi-security-filter-form tfi-data-source="$ctrl.dataSouce" ' +
                            'tfi-on-submit="$ctrl.applyFilter(filterData)" ' +
                            'tfi-on-cancel="$ctrl.popoverApi.hide()">' +
                        '</tfi-security-filter-form>' +
                    '</tfi-popover>' +
                '</div>',
            controller: SecurityFilterController,
            bindings: {
                dataSouce: '<tfiDataSource',
                onFilter: '&tfiOnFilter'
            },
            bindToController: true
        });

    SecurityFilterController.$inject = ['$scope', '$element', '$attrs'];

    function SecurityFilterController($scope, $element, $attrs)
    {
        this.applyFilter = applyFilter;

        this.$onInit = $onInit;
        this.$onChanges = $onChanges;
        this.$onDesrtoy = $onDesrtoy;
        this.isDisabled = false;

        var self = this;

        function applyFilter(filterData)
        {
            self.onFilter({ filterData: filterData });
            self.popoverApi.hide()
        }

        function $onInit()
        {
            $element.addClass('security-filter');
            $attrs.$observe('disabled', function (disabled)
            {
                self.isDisabled = disabled;
                $element.toggleClass('is-disabled', disabled);
            });
        }

        function $onChanges(changes) { }

        function $onDesrtoy() { }
    }
})(angular);