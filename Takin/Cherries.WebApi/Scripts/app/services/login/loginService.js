app.factory('loginSvc', ['$window', '$http', '$q', 'serviceHelperSvc', function ($window, $http, $q, serviceHelperSvc) {

    var applicant = serviceHelperSvc.Users;
    var logoff = serviceHelperSvc.logoff;
    var updateUserPassword = serviceHelperSvc.updateUserPassword;
    var sessionId;

    var setSessionHeader = function (id) {
        sessionId = id;
        $http.defaults.headers.common["Authorization"] = "Basic " + id;
    }

    $window.onbeforeunload = function () {
        if (sessionId)
            sessionStorage.setItem('sessionid', angular.toJson(sessionId));
    };



    var login = {
        get: function (serchParams) {
            var deferred = $q.defer();
            applicant.save(serchParams,
                function (data, getResponseHeaders) {
                    if (data.User != null) {
                        setSessionHeader(getResponseHeaders("sessionid"));
                    }

                    deferred.resolve(data);
                }, function (error) {
                    deferred.reject(error);
                });

            return deferred.promise;
        },
        logoff: function () {
            return logoff.get();
        },

        updatePasswrod: function (parameters) {

            return updateUserPassword.updatePassword(parameters);
        }

    }
    return login;
}]);