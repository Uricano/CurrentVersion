// <summary>
//    Represents a service session storage.
// </summary>
(function (angular)
{
    angular.module('takinApp').service('session', createService);

    createService.$inject = [];

    function createService()
    {
        var userDetailsKey = 'userDetails';

        /*
        * @name getUserDetails
        * @desc Returns user details.
        * @return {Object} size in KBytes.
        */
        this.getUserDetails = getUserDetails;

        /*
        * @name setUserDetails
        * @desc Save user details to session storage.
        * @params {Object} user - user details.
        */
        this.setUserDetails = setUserDetails;

        /*
        * @name removeUserDetails
        * @desc Remove user details from session storage.
        */
        this.removeUserDetails = removeUserDetails;

        /*
        * @name clearSession
        * @desc Clear storage.
        */
        this.clearSession = clearSession;

        function getUserDetails()
        {
            return angular.fromJson(sessionStorage.getItem(userDetailsKey));
        }

        function setUserDetails(user)
        {
            sessionStorage.setItem(userDetailsKey, angular.toJson(user));
        }

        function removeUserDetails()
        {
            sessionStorage.removeItem(userDetailsKey);
        }

        function clearSession()
        {
            Object.keys(sessionStorage).forEach(function (k)
            {
                // TODO: "lookupData" ???
                if (k != "lookupData") sessionStorage.removeItem(k);
            });
        }
    }
})(angular);