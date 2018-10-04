app.factory('priceSvc', ['serviceHelperSvc', function (serviceHelperSvc) {

    var Prices = serviceHelperSvc.Prices;

    return {

        getPrices: function (params) {
            return Prices.get(params);
        }

    }

}]);