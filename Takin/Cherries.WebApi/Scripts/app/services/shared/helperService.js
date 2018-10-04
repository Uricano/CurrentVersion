app.factory('serviceHelperSvc', ['$http', '$resource', function ($http, $resource) {
    var baseUrl = '';
    var buildUrl = function (resourceUrl) {
        return baseUrl + resourceUrl;
    };
    var Invoker = $resource(buildUrl('api/Invoker'));

    var InvokerWithCache = $resource(buildUrl('api/Invoker'), {}, {
        get: {
            method: "GET",
            cache: true
        }
    });

    Invoker.createMethodName = function (method) {
        return method + "?key=" + sessionStorage.getItem('sessionid');
    }

    Invoker.convertJsonToUri = function (model) {
        var params = "&";
        for (p in model) {
            params += p + "=" + model[p] + "&";
        }
        return params;
    }
    return {
        Invoker: Invoker,

        InvokerWithCache: InvokerWithCache,

        Users: $resource(buildUrl('api/User')),

        logoff: $resource(buildUrl('api/User/Logoff')),

        updateUserPassword: $resource(buildUrl('api/User'), null,
            {
                updatePassword: { method: 'put' }
            }),

        SendConfirmCode: $resource(buildUrl('api/User/SendConfirmCode')),

        VerifyConfirmCode:$resource(buildUrl('api/User/VerifyConfirmCode')),

        VerifyUsername: $resource(buildUrl('api/User/VerifyUsername')),

        CreateUser: $resource(buildUrl('api/User/Create')),

        updateUserDetails:$resource(buildUrl('api/User/Update')),

        Portfolios: $resource(buildUrl('api/Portfolios')),

        updatePortfolios: $resource(buildUrl('api/Portfolios'), null,
          {
              updatePort: { method: 'put' }
          }),

        PortfoliosId: $resource(buildUrl('api/Portfolios/:id'), { id: "@id" }),

        Securities: $resource(buildUrl('api/Securities'), null,
            {
                getAll: { method: 'get', url: buildUrl('api/Securities/GetAll') }
            }),

        Optimization: $resource(buildUrl('api/Optimization')),

        GetBenchmarkSecurities: $resource(buildUrl('api/Securities/GetBenchmarkSecurities')),

        Backtesting: $resource(buildUrl('api/Backtesting')),

        BacktestingId: $resource(buildUrl('api/Backtesting/:id'), { id: "@id" }),

        BacktestingPortfolios: $resource(buildUrl('api/BacktestingPortfolios')),

        BacktestingPortfoliosId: $resource(buildUrl('api/BacktestingPortfolios/:portId'), { portId: "@portId" }),

        Prices: $resource(buildUrl('api/Prices')),

        License: $resource(buildUrl('api/License')),
      
        CalculateLicense:  $resource(buildUrl('api/License/Calculate')),

        Lookups: $resource(buildUrl('api/Lookup')),

        Session: {
            removeSession: function () {
                var xhr = new XMLHttpRequest();
                xhr.open('post', buildUrl('api/Users/RemoveSession?key=' + sessionStorage.getItem('sessionid')), false);
                //xhr.setRequestHeader("Authorization", $http.defaults.headers.common["Authorization"]);
                xhr.send();
                return true;
            }
        },

        exportToExcel: function (methos, data, type) {
            return $http({
                url: buildUrl('api/Invoker'),
                method: "GET",
                params: {
                    method: methos + '?key=' + sessionStorage.getItem('sessionid'),
                    param: Invoker.convertJsonToUri(data)
                }
            }
            );
        },

        UserGroup: $resource(buildUrl('api/users/GetUserGroup'))
    }
}]);