app.directive('backtesting', ["$location", "$modal", "utilitiesSvc", "backtestingSvc", function ($location, $modal, utilitiesSvc, backtestingSvc) {
    return {
        restrict: 'E',
        templateUrl: 'scripts/app/partials/backtesting.html',
        scope: {
            model: '=',
            refreshAll: '&'
        },
        compile: function (tElement) {
            tElement.addClass('backtesting');

            return function (scope, element, attrs, ngModelCtrl) {
                scope.$on('$destroy', function () {
                    for (var i in scope) {
                        if (i.indexOf('$') == 1)
                            scope[i] = null;
                    }
                })
                scope.openBacktesting = function (port) {
                    backtestingSvc.getbacktesingPortfoliesById(scope.model.Details.ID).$promise.then(function (res) {
                        var a = res;
                        var fullbenchMarks = [];
                        if (res.benchMarkResult)
                        {
                            for (var i = 0; i < res.benchMarkResult.length; i++) {
                                res.benchMarkResult[i].Item1 = res.benchMarkResult[i].m_Item1;
                                res.benchMarkResult[i].Item2 = res.benchMarkResult[i].m_Item2;
                                if (res.benchMarkResult[i].Item1.ID != '0000')
                                    fullbenchMarks.push({ id: res.benchMarkResult[i].Item1.ID, label: res.benchMarkResult[i].Item1.Name });
                            }
                        }

                        var params = {
                            Equity: res.Equity,
                            FullbenchMarks: fullbenchMarks,
                            Name: scope.model.Details.Name,
                            StartDate: new Date(scope.model.StartDate),
                            EndDate: new Date(scope.model.EndDate)
                        }
                        res.Equity = scope.model.Details.Equity;
                        scope.$parent.saveBacktestingParams(params);
                        scope.$parent.setBacktestingDetails(res);
                        $location.path('/backtesting');
                    });


                    // $location.path('/backtesting');

                }

                scope.deleteBacktesting = function ($event) {

                    $event.stopPropagation();
                    utilitiesSvc.showYesNoMessage("message", 'This backtesting will be deleted, continue?', "yes", "no").then(function (result) {
                        backtestingSvc.deleteBacktesting(scope.model.Details.ID).$promise.then(successDeleteBacktesting, failedDeleteBacktesting);
                    });

                };

                scope.toggleSectors = function ($event) {
                    $event.stopPropagation();
                    scope.showAll = !scope.showAll;
                }

                var successDeleteBacktesting = function (data) {
                    utilitiesSvc.showOKMessage('message', 'Backtesting: ' + scope.model.Details.Name + ' deleted.', 'OK');
                    scope.refreshAll({ backtest: scope.model });
                };

                var failedDeleteBacktesting = function (error) {
                    utilitiesSvc.showOKMessage('error', 'failed to delete backtesting: ' + scope.model.Details.Name, 'OK');
                };

                var setRiskColor = function () {

                    var value = scope.model.Details.CurrentStDev * 100;
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