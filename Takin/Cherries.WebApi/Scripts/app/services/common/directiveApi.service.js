(function (angular)
{
    angular.module('takinApp.common').service('directiveApi', createService);

    createService.$inject = ['$parse'];
    function createService($parse)
    {
        return function (directiveName)
        {
            return {
                restrict: 'A',
                require: directiveName,
                link: function (scope, element, attrs, ctrl)
                {
                    var fieldName = attrs[directiveName + 'Api'];
                    if (angular.isUndefined(fieldName))
                    {
                        return;
                    }

                    var getter = $parse(fieldName);
                    getter.assign(scope, ctrl.publicApi);
                }
            };
        }
    }
})(angular, _);