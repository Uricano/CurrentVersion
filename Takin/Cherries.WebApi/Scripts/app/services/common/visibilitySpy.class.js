// 
// Visibility spy
// 

(function (angular)
{
    angular.module('takinApp.common').factory('VisibilitySpy', createClass);

    createClass.$inject = [];

    function createClass()
    {
        function VisibilitySpy(element, visibilityChangeFn)
        {
            this.element = element;
            this.onVisibilityChange = visibilityChangeFn;
        }

        return VisibilitySpy;
    }
})(angular);