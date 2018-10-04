app.factory('lookupSvc', ['$window', '$http', '$q', 'serviceHelperSvc', function ($window, $http, $q, serviceHelperSvc) {

    var lookups = serviceHelperSvc.Lookups;

    return {

        getLookup: function () {
            return lookups.get();
        }


    }

}]);