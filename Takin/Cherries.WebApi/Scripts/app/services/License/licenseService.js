app.factory('licenseSvc', ['serviceHelperSvc', function (serviceHelperSvc) {

    var license = serviceHelperSvc.CalculateLicense;

    return {

        calculateLicense: function (params) {
            return license.get(params);
        },
        updateUserLicence: function (params) {
            return serviceHelperSvc.License.save(params);
        }

    }

}]);