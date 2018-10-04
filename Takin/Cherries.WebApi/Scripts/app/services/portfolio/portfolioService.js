app.factory('portfolioSvc', ['$window', '$resource', '$q', 'serviceHelperSvc', function ($window, $resource, $q, serviceHelperSvc) {

    var portfolios = serviceHelperSvc.Portfolios;
    var updatePortfolios = serviceHelperSvc.updatePortfolios;
    var portfoliosId = serviceHelperSvc.PortfoliosId;

    return {

        getPortfolios: function (paramerts) {
            return portfolios.get({
                "pageSize": paramerts.pageSize, "pageNumber": paramerts.pageNumber, "sortField": paramerts.sortField
            });
        },
        createPortfolio: function (parameters) {
            return portfolios.save(parameters);
        },
        updatePortfolio: function (parameters) {

            return updatePortfolios.updatePort(parameters);
        },
        getSinglePortfolio: function (id) {
            return portfoliosId.get({ "id": id });
        },
        deletePortfolio: function (id) {
            return portfolios.delete({ "portId": id });
        },
        exportOptimize: function (paramerts) {
            return serviceHelperSvc.Optimization.save(paramerts);
        }
    }

}]);