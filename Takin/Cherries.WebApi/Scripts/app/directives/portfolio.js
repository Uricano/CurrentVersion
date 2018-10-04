app.directive('portfolio', ["$location", "$modal", "utilitiesSvc", "portfolioSvc", function ($location, $modal, utilitiesSvc, portfolioSvc) {
    return {
        restrict: 'E',
        templateUrl: 'scripts/app/partials/portfolio.min.html',
        scope: {
            model: '=',
            refreshAll: '&'
        },
        compile: function(tElement)
        {
            tElement.addClass('portfolio');

            return function (scope, element, attrs, ngModelCtrl) {
            
                scope.openPortfolio = function (port) {
                    $location.path('/portfolios/' + scope.model.ID);
               
                }

                scope.deletePortfolio = function ($event) {
               
                    $event.stopPropagation();
                    utilitiesSvc.showYesNoMessage("message", 'This portfolio will be deleted, continue?', "yes", "no").then(function (result) {
                        portfolioSvc.deletePortfolio(scope.model.ID).$promise.then(successDeletePortfolio, failedDeletePortfolio);
                   
                    });

                };

                scope.toggleSectors = function ($event) {
                    $event.stopPropagation();
                    scope.showAll = !scope.showAll;
                }

                var successDeletePortfolio = function (data) {
                    utilitiesSvc.showOKMessage('message', 'Portfolio: ' + scope.model.Name + ' deleted.', 'OK');
                    scope.refreshAll({ port: scope.model });
                };

                var failedDeletePortfolio = function (error) {
                    utilitiesSvc.showOKMessage('error', 'failed to delete portfolio: ' + scope.model.Name, 'OK');
                };

                var setRiskColor = function () {

                    var value = scope.model.CurrentStDev * 100;
                    var riskName;

                    switch (true) {
                        case (value > 0 && value <= 9):
                            riskName = 'Solid'
                            break;
                        case (value > 9 && value <= 14):
                            riskName = 'Low';
                            break;
                        case (value > 14 && value <= 25):
                            riskName = 'Moderate';
                            break;
                        case (value > 25 && value <= 40):
                            riskName = 'High';
                            break;
                        case (value > 40 && value <= 100):
                            riskName = 'Very High';
                            break;
                    }

                    scope.riskColor = scope.riskColors[riskName];

                    if (!scope.riskColor)
                        scope.riskColor = '#ffffff';

                    scope.getCurrency = function getCurrency(currency) {
                        return scope.$parent.getLookupValues().Currencies.filter(
                            function (x) { return x.CurrencyId == currency })[0].CurrencySign;
                    }
                }

                var init = function () {

                    user = scope.$parent.getUser();

                    scope.currency = user.Currency.CurrencySign;

                    scope.riskColors = riskColors;

                    setRiskColor();
                }

                init();
            }
        }
    }
}]);