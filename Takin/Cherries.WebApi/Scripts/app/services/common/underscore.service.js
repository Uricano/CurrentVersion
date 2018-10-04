(function (angular, _)
{
    angular.module('takinApp.common').factory('underscore', createService);

    function createService()
    {
        return _.noConflict();
    }
})(angular, _);