app.factory('securitiesSvc', ['serviceHelperSvc', function (serviceHelperSvc) {

    var securities = serviceHelperSvc.Securities;
    var benchmarkSecurities = serviceHelperSvc.GetBenchmarkSecurities;

    return {

        getSecurities: function () {
            return securities.get();
        },
        getBenchmarkSecurities: function () {
            return benchmarkSecurities.get();
        },
        getAllSecurities : function (data) {
            return securities.getAll(data);
        }
    }

}]);