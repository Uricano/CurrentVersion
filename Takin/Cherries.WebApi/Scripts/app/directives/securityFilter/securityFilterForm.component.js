(function (angular)
{
    angular.module('takinApp.common')
        .component('tfiSecurityFilterForm', {
            template:
                '<a href="#" class="security-filter-form__close" ng-click="$ctrl.close()">' +
                    '<img src="Content/themes/images/close_icon.png" />' +
                '</a>' +
                '<div class="security-filter-form__panel">' +
                    '<div class="security-filter-form__links">' +
                        '<a href="#" ng-click="$ctrl.apply()" class="l-font-bold">Apply and Close</a>' +
                        '<a href="#" ng-click="$ctrl.reset()">Reset</a>' +
                    '</div>' +
                '</div>' +
                '<div class="security-filter-form__panel">' +
                    '<div class="security-filter-form__title">Maximum Risk Level (Securities)</div>' +
                    '<tfi-risk-selector ng-model="$ctrl.riskLevel">' +
                    '</tfi-risk-selector>' +
                '</div>' +
                '<div class="security-filter-form__panel">' +
                    '<div class="security-filter-form__title">Exchanges</div>' +
                    '<div ng-repeat="exchange in $ctrl.dataSource.exchanges">' +
                        '<md-checkbox ng-model="exchange.selected">' +
                            '<span ng-bind="exchange.label" class="l-color-light l-font-base"></span>' +
                        '</md-checkbox>' +
                    '</div>' +
                '</div>' +
                '<div class="security-filter-form__panel">' +
                    '<div class="security-filter-form__title">Sectors</div>' +
                    '<div ng-repeat="sector in $ctrl.dataSource.sectors">' +
                        '<md-checkbox ng-model="sector.selected">' +
                            '<span ng-bind="sector.label" class="l-color-light l-font-base"></span>' +
                        '</md-checkbox>' +
                    '</div>' +
                '</div>',
            controller: SecurityFilterFormController,
            bindings: {
                dataSource: '<tfiDataSource',
                submit: '&tfiOnSubmit',
                cancel: '&tfiOnCancel'
            },
            bindToController: true
        });

    SecurityFilterFormController.$inject = ['$scope', '$element'];

    function SecurityFilterFormController($scope, $element)
    {
        this.apply = apply;
        this.reset = reset;
        this.close = close;

        this.$onInit = $onInit;
        this.$onChanges = $onChanges;
        this.$onDesrtoy = $onDesrtoy;
        
        var self = this;
        var selectedExchanges = [];
        var selectedSectors = [];

        function apply()
        {
            filterData = {
                exchanges: self.dataSource.exchanges.filter(function (exchange)
                {
                    return exchange.selected;
                }),
                sectors: self.dataSource.sectors.filter(function (sector)
                {
                    return sector.selected;
                }),
                riskLevel: self.riskLevel
            };
            self.submit({ filterData: filterData });
        }

        function reset()
        {
            self.cancel();
        }

        function close()
        {
            self.cancel();
        }

        function $onInit()
        {
            $element.addClass('security-filter-form');
            self.riskLevel = self.dataSource.riskLevel;
        }

        function $onChanges(changes) { }

        function $onDesrtoy() { }
    }
})(angular);