app.factory('userSvc', ['$window', '$http', '$q', 'serviceHelperSvc', function ($window, $http, $q, serviceHelperSvc) {


    return {

        sendUserConfirmCode: function (paramerts) {

            return serviceHelperSvc.SendConfirmCode.save(paramerts);
        },
        verifyConfirmCode: function (params) {
            return serviceHelperSvc.VerifyConfirmCode.save(params);
        },
        verifyUsername: function(params) {
            return serviceHelperSvc.VerifyUsername.save(params);
        },
        createUser: function (parameters) {
            return serviceHelperSvc.CreateUser.save(parameters);
        },

        updateUesrDetails: function (paramaters) {
            return serviceHelperSvc.updateUserDetails.save(paramaters);
        }

    }


}]);