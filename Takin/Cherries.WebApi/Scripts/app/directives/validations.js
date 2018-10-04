app.directive('phoneValidation', ["$timeout", "$compile", "$filter", function ($timeout, $compile, $filter) {
    return {
        require: 'ngModel',
        scope: {
        },
        link: function (scope, element, attr, modelCtrl) {
            modelCtrl.$parsers.push(function (inputValue) {

                if (inputValue == "") {
                    return "";
                }
                var transformedInput = inputValue;
                //remove chartes that not number or comma
                transformedInput = transformedInput.replace(/[^0-9]/g, '').substring(0, 7);

                if (transformedInput != inputValue) {
                    modelCtrl.$setViewValue(transformedInput);
                    modelCtrl.$render();
                }

                return transformedInput.replace(/,/g, '');
            });

            modelCtrl.$formatters.push(function (inputValue) {
                return inputValue;
            });
        }
    };
}]);

app.directive('maxLength', ['$compile', function ($compile) {

    return {
        require: 'ngModel',
        scope: {
            maxCharters: '=?',
        },
        link: function (scope, element, attr, modelCtrl) {
            modelCtrl.$parsers.push(function (inputValue) {

                if (inputValue == "") {
                    return "";
                }

                var transformedInput = inputValue;
            
                //limit text length

                transformedInput = transformedInput.substring(0, scope.maxCharters);

                if (transformedInput != inputValue) {
                    modelCtrl.$setViewValue(transformedInput);
                    modelCtrl.$render();
                }
                //set default number foramt
               
                return transformedInput;
            });

            modelCtrl.$formatters.push(function (inputValue) {
               
                return inputValue;
            });
        }
    };
}]);