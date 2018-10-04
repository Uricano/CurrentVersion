app.factory('backtestingSvc', ['serviceHelperSvc', function (serviceHelperSvc) {

    var backtesting = serviceHelperSvc.Backtesting;
    var backtesingPortfolies = serviceHelperSvc.BacktestingPortfolios;
    var backtestingId = serviceHelperSvc.BacktestingId;
    var portfoliosId = serviceHelperSvc.PortfoliosId;

    return {

        createBacktesting: function (paramerts) {
            return backtesting.save(paramerts);
        },
        getbacktesingPortfolies: function (paramerts) {
            return backtesingPortfolies.get({ "pageSize": paramerts.pageSize, "pageNumber": paramerts.pageNumber, "sortField": paramerts.sortField });
        },
        getbacktesingPortfoliesById: function (id) {
            return backtestingId.get({ "id": id });
        },
        deleteBacktesting: function (portId) {
            return backtesingPortfolies.delete({ "portId": portId });
        },

    }

}]);