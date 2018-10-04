app.directive('numberOnly', ["$timeout", "$compile", "$filter", function ($timeout, $compile, $filter) {
    return {
        require: 'ngModel',
        scope: {
            negativeNumber: '=?',
            isNumberFormat: '=?',
            numberLength: '=?',
            limitDecimal: '=?'
        },
        link: function (scope, element, attr, modelCtrl) {
            modelCtrl.$parsers.push(function (inputValue) {

                if (inputValue == "") {
                    return "";
                }

                var transformedInput = inputValue.toString();


                //remove chartes that not number or comma
                if (scope.negativeNumber == true) {
                    transformedInput = transformedInput.replace(/[^-?0-9,\.]/g, '');
                }
                else if (scope.isNumberFormat == true) {
                    if (scope.limitDecimal == true)
                        transformedInput = transformedInput.replace(/[^0-9,]/g, '');
                    else
                        transformedInput = transformedInput.replace(/[^0-9,\.]/g, '');
                }
                else {
                    transformedInput = transformedInput.replace(/[^0-9]/g, '');
                }
                //limit number length
                if (scope.numberLength && transformedInput.replace(/[^0-9]/g, '').length > scope.numberLength) {
                    transformedInput = transformedInput.replace(/[^0-9]/g, '');
                    transformedInput = transformedInput.substring(0, scope.numberLength);

                }

                if (transformedInput != inputValue) {
                    modelCtrl.$setViewValue(transformedInput);
                    modelCtrl.$render();
                }
                //set default number foramt
                if (scope.isNumberFormat == true) {
                    var tr = transformedInput.replace(/,/g, '');

                    if (inputValue[inputValue.length - 1] != '-' && inputValue[inputValue.length - 1] != '.' && $filter('number')(tr) != transformedInput) {
                        if (tr != '') {
                            modelCtrl.$setViewValue($filter('number')(tr, 0));
                        }
                        else {
                            modelCtrl.$setViewValue(tr);
                        }
                        modelCtrl.$render();
                    }
                }
                return transformedInput.replace(/,/g, '');
            });

            modelCtrl.$formatters.push(function (inputValue) {
                if (scope.isNumberFormat == true) {
                    return $filter('number')(inputValue);
                }
                return inputValue;
            });
        }
    };
}]);