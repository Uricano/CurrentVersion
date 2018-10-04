//
// Represents a filter to truncate number.
//

(function (angular)
{
    angular.module('takinApp.common').filter('truncate', createFilter);

    /*
    * @name truncate
    * @desc Represents a filter to truncate number.
    */
    createFilter.$inject = [];
    function createFilter()
    {
        return function (value)
        {
            if (!angular.isNumber(value))
            {
                return null;
            }

            return Math.floor(value);
        }
    }
})(angular);