(function (angular)
{
    angular.module('takinApp.common')
        .component('tfiRiskSelector', {
            template:
                '<div ng-repeat="(key, value) in $ctrl.levels" ' +
                    'class="risk-selector__item" ' +
                    'ng-class="{\'is-selected\': value.selected}" ' +
                    'ng-style="{\'width\':$ctrl.itemWidth}" ' +
                    'ng-click="$ctrl.selectRiskLevel(value)">' +
                    '<div class="risk-selector__item-color" ' +
                        'ng-style="{\'background-color\':value.color}"></div>' +
                    '<div ng-bind="value.label" class="risk-selector__item-label"></div>' +
                '</div>',
            controller: RiskSelectorController,
            bindToController: true,
            require: {
                ngModel: 'ngModel'
            }
        });

    RiskSelectorController.$inject = ['$element', '$attrs', 'riskLevel', 'underscore'];

    function RiskSelectorController($element, $attrs, riskLevel, underscore)
    {
        this.selectRiskLevel = selectRiskLevel;
        this.levels = angular.extend({}, riskLevel);
        this.itemWidth = '' + (100 / underscore.keys(riskLevel).length) + '%';
        this.isDisabled = false;

        this.$onInit = $onInit;
        this.$onChanges = $onChanges;
        this.$onDesrtoy = $onDesrtoy;

        var self = this;

        function $onInit()
        {
            $element.addClass('risk-selector');

            $attrs.$observe('disabled', function (disabled)
            {
                self.isDisabled = disabled;
                $element.toggleClass('is-disabled', disabled);
            });

            self.ngModel.$render = render;

            function render()
            {
                var currentRiskLevel = self.ngModel.$viewValue;
                angular.forEach(self.levels, function (level)
                {
                    level.selected = level.value == currentRiskLevel;
                });
            }
        }

        function $onChanges(changes)
        {

        }

        function $onDesrtoy()
        {

        }

        function selectRiskLevel(selectedLevel)
        {
            if (self.isDisabled)
            {
                return;
            }
            angular.forEach(self.levels, function (level)
            {
                level.selected = level.value == selectedLevel.value;
            });
            self.ngModel.$setViewValue(selectedLevel.value);
        }
    }
})(angular);