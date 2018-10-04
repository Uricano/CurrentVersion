(function (angular)
{
    angular.module('takinApp').constant('tfiCommonConstants', {

        // Event names
        updateAccountData: 'updateAccountData',
        isInProgress: 'isInProgress',
        wantToLogout: 'wantToLogout',
        menuItemChanged: 'menuItemChanged',
        objNameChanged: 'objNameChanged',
        isDataReady: 'isDataReady'
    });

})(angular);