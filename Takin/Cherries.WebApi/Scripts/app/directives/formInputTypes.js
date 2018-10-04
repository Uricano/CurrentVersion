app.directive('cstNumber', ['$compile', 'utilitiesSvc',
    function ($compile, utilitiesSvc) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {
                    pre: function (scope, element, attrs, modelCtrl) {
                        setElementNameAndModel(scope, modelCtrl, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        var errorDivMax = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.maxNumber',
                            "class": "error-message"
                        }).text('מספר גדול מ ' + element.attr("max"));
                        element.parent().append(errorDivMax);
                        $compile(errorDivMax)(scope);
                        var errorDivMin = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.minNumber',
                            "class": "error-message"
                        }).text('מספר קטן מ ' + element.attr("min"));
                        element.parent().append(errorDivMin);
                        $compile(errorDivMin)(scope);


                        modelCtrl.errorMessage = modelCtrl.errorMessage || {};
                        modelCtrl.errorMessage.minNumber = attrs.label + ": מספר קטן מ " + attrs.min;

                        //For DOM -> model validation
                        modelCtrl.$parsers.unshift(function (value) {
                            var valid = setValid(value, "min");
                            value = getNumber(value);
                            return valid ? value : undefined;
                        });

                        //For model -> DOM validation
                        modelCtrl.$formatters.unshift(function (value) {
                            setValid(value, "min");
                            value = getNumber(value);
                            return value;
                        });

                        modelCtrl.errorMessage = modelCtrl.errorMessage || {};
                        modelCtrl.errorMessage.maxNumber = attrs.label + ": מספר גדול מ " + attrs.max;

                        modelCtrl.$parsers.unshift(function (value) {
                            var valid = setValid(value, "max");
                            value = getNumber(value);
                            return valid ? value : undefined;
                        });

                        //For model -> DOM validation
                        modelCtrl.$formatters.unshift(function (value) {
                            setValid(value, "max");
                            value = getNumber(value);
                            return value;
                        });

                        function setValid(value, type) {
                            var valid = true;
                            if (attrs[type] && value != "" && !isNaN(value)) {
                                //modelCtrl.$modelValue = value;
                                if (type == "max")
                                    valid = value <= parseFloat(attrs[type]);
                                else
                                    valid = value >= parseFloat(attrs[type]);
                            }
                            modelCtrl.$setValidity(type + 'Number', valid);
                            return valid;
                        }
                        function getNumber(val) {
                            if (val)
                                if (val.toString().indexOf('.') > -1 && val.toString().indexOf('.') < val.toString().length - 2) val = parseFloat(val).toFixed(2);
                            return val;
                        }
                    },
                    post: function (scope, element, attrs, modelCtrl) {
                        element.bind("keypress", function () {
                            var digits = this.value.replace(/\d+\.?\d{0,1}/, "");
                            for (var i = this.value.length - 1 ; i > -1 ; i--) {
                                if (digits.indexOf(this.value.charAt(i)) > -1) {
                                    this.value = this.value.slice(0, i) + this.value.slice(i + 1);
                                    break;
                                }
                            }

                            //if (this.value.indexOf('.') > -1 && this.value.indexOf('.') == this.value.length - 3) this.value = parseFloat(this.value).toFixed(2);
                            modelCtrl.$setViewValue(this.value);
                            modelCtrl.$render();
                            scope.$apply();
                        });
                    }
                }
            }
        }
    }]);

app.directive('cstZehut', ['$compile',
    function ($compile) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {
                    pre: function (scope, element, attrs, modelCtrl) {
                        setElementNameAndModel(scope, modelCtrl, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        var errorDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.zehut',
                            "class": "error-message"
                        }).text('ספרת ביקורת שגויה');
                        element.parent().append(errorDiv);
                        $compile(errorDiv)(scope);

                        modelCtrl.errorMessage = modelCtrl.errorMessage || {};
                        modelCtrl.errorMessage.zehut = attrs.label + ": ספרת ביקורת שגויה ";

                    },
                    post: function (scope, element, attrs, modelCtrl) {
                        element.bind("keyup", function () {
                            var digits = this.value.split('').filter(function (s) { return (!isNaN(s) && s != ' '); }).join('');
                            modelCtrl.$viewValue = digits;
                            modelCtrl.$render();
                            scope.$apply();
                        });
                        element.on("blur", function () {
                            var digits = this.value;
                            if (digits != "") {
                                digits = digits.padLeft(9);
                                var counter = 0, incNum;
                                for (var i = 0 ; i < digits.length ; i++) {
                                    incNum = Number(digits.toString().charAt(i)) * ((i % 2) + 1);//multiply digit by 1 or 2
                                    counter += (incNum > 9) ? incNum - 9 : incNum;//sum the digits up and add to counter
                                }

                                modelCtrl.$setValidity('zehut', counter % 10 == 0);
                            }
                            else
                                modelCtrl.$setValidity('zehut', true);
                            modelCtrl.$setViewValue(digits);

                            modelCtrl.$render();
                            scope.$apply();
                        });
                    }
                }
            }
        }
    }]);

app.directive('cstBirthDateYear', ['$compile',
    function ($compile) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {
                    pre: function (scope, element, attrs, ngModel) {
                        setElementNameAndModel(scope, ngModel, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        var errorDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.birthDateYear',
                            "class": "error-message"
                        }).text(attrs.labelTxt);
                        element.parent().append(errorDiv);
                        $compile(errorDiv)(scope);
                    },
                    post: function (scope, element, attrs, ngModel) {
                        ngModel.errorMessage = ngModel.errorMessage || {};
                        ngModel.errorMessage.birthDateYear = attrs.label + attrs.labelMin;
                        var validate = function (viewValue) {
                            var comparisonModel = attrs.minYear;
                            var comparisonModelmax = attrs.maxYear;

                            if (!viewValue || viewValue == "") {
                                // It's valid because we have nothing to compare against
                                ngModel.$setValidity('birthDateYear', true);
                                return viewValue;
                            }

                            var birthDateYear = /^\d{4}$/.test(viewValue);
                            // It's valid if model is lower than the model we're comparing against
                            ngModel.$setValidity('birthDateYear', birthDateYear && (viewValue >= attrs.minYear || viewValue <= attrs.maxYear));
                            return viewValue;
                        };

                        ngModel.$parsers.unshift(validate);
                        ngModel.$formatters.push(validate);

                        attrs.$observe('dateMinModel', function (comparisonModel) {
                            // Whenever the comparison model changes we'll re-validate
                            return validate(ngModel.$viewValue);
                        });
                        attrs.$observe('dateMaxModel', function (comparisonModelmax) {
                            // Whenever the comparison model changes we'll re-validate
                            return validate(ngModel.$viewValue);
                        });
                    }
                }
            }

        };
    }]);

app.directive('cstHebrewText', ['$compile',
    function ($compile) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {

                    pre: function (scope, element, attrs, modelCtrl) {
                        setElementNameAndModel(scope, modelCtrl, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        if (scope.textLength == null) {
                            scope.textLength = [];
                        }
                        scope.textLength = attrs.maxLength;
                        var errorDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.hebrew',
                            "class": "error-message"
                        }).text('יש להכניס אותיות בעברית בלבד');
                        element.parent().append(errorDiv);
                        $compile(errorDiv)(scope);

                        modelCtrl.errorMessage = modelCtrl.errorMessage || {};
                        modelCtrl.errorMessage.hebrew = attrs.label + ": יש להכניס אותיות בעברית בלבד";

                        var errorLongTextDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.longtext',
                            "class": "error-message"
                        }).text("יש להכניס עד {{textLength}} תווים");
                        element.parent().append(errorLongTextDiv);
                        $compile(errorLongTextDiv)(scope);

                        modelCtrl.errorMessage = modelCtrl.errorMessage || {};
                        modelCtrl.errorMessage.longtext = attrs.label + ": יש להכניס עד {{textLength}} תווים";


                    },
                    post: function (scope, element, attrs, modelCtrl) {
                        element.bind("keyup", function () {

                        });
                        element.on("blur", function (attrs) {
                            var text = this.value;
                            scope.textLength = attrs.target.attributes.getNamedItem('max-length').nodeValue;
                            if (text != "" && text != null) {
                                var testHebrew = /^([ א-ת])*$/.test(text);
                                modelCtrl.$setValidity('hebrew', testHebrew);
                                if (testHebrew && scope.textLength) {
                                    var patt = new RegExp('^[ א-תA-Z]{1,' + scope.textLength + '}$');
                                    modelCtrl.$setValidity('longtext', (patt.test(text)));
                                }
                            }
                            else
                                modelCtrl.$setValidity('hebrew', true);
                            modelCtrl.$setViewValue(text);

                            modelCtrl.$render();
                            scope.$apply();
                        });
                    }
                }
            },
        }
    }]);

app.directive('cstEnCharactersText', ['$compile',
    function ($compile) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {

                    pre: function (scope, element, attrs, modelCtrl) {
                        setElementNameAndModel(scope, modelCtrl, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        if (scope.textLength == null) {
                            scope.textLength = [];
                        }
                        scope.textLength = attrs.maxLength;
                        var errorDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.encharacters',
                            "class": "error-message red-text"
                        }).text('Insert only english characters');
                        element.parent().append(errorDiv);
                        $compile(errorDiv)(scope);

                        modelCtrl.errorMessage = modelCtrl.errorMessage || {};
                        modelCtrl.errorMessage.hebrew = attrs.label + ": Insert only english characters";

                    },
                    post: function (scope, element, attrs, modelCtrl) {
                        element.bind("keyup", function () {

                        });
                        element.on("blur", function (attrs) {
                            var text = this.value;
                            if (text != "" && text != null) {
                                var testenCharacters = /^([ a-zA-Z])*$/.test(text);
                                modelCtrl.$setValidity('encharacters', testenCharacters);
                            }
                            else
                                modelCtrl.$setValidity('encharacters', true);
                            modelCtrl.$setViewValue(text);

                            modelCtrl.$render();
                            scope.$apply();
                        });
                    }
                }
            },
        }
    }]);


app.directive('cstSubmit', ['utilitiesSvc', function (utilitiesSvc) {
    return {
        restrict: "A",
        link: function (scope, element, attrs) {
            element.bind('click', function () {
                var formName = $(this).closest('form')[0].name;
                if (scope[formName].$invalid) {
                    scope[formName].invalidSubmitAttempt = true;
                    var msg = [];
                    for (var error in scope[formName].$error) {
                        for (var i = 0 ; i < scope[formName].$error[error].length ; i++) {
                            msg.push(scope[formName].$error[error][i].errorMessage[error]);
                        }
                    }

                    utilitiesSvc.showOKMessage(msg.join('\n'), 'אישור', 'שגיאה - נא לתקן את השדות הבאים');
                    scope.$apply();
                    return false;
                }
                scope[formName].invalidSubmitAttempt = false;
                scope.$eval(attrs.cstSubmit);
            });
        }
    };
}]);

app.directive('cstRequired', ['$compile',
    function ($compile) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {
                    pre: function (scope, element, attrs, ngModel) {
                        setElementNameAndModel(scope, ngModel, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        var elementName = element.closest('form')[0].name + "." + element[0].name;
                        var errorDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.required',
                            "class": "error-message"
                        }).text('חובה');
                        element.parent().append(errorDiv);
                        $compile(errorDiv)(scope);
                    },
                    post: function (scope, element, attrs, ngModel) {
                        ngModel.errorMessage = ngModel.errorMessage || {};
                        ngModel.errorMessage.required = attrs.label + ': חובה';
                        element.prop("required", true);
                        //For DOM -> model validation
                        ngModel.$parsers.unshift(function (value) {
                            var valid = value !== "" && value != undefined;
                            ngModel.$setValidity('required', valid);
                            return valid ? value : undefined;
                        });

                        //For model -> DOM validation
                        ngModel.$formatters.unshift(function (value) {
                            ngModel.$setValidity('required', value !== "" && value != undefined);
                            return value;
                        });

                        attrs.$observe('disabled', function (value) {
                            if (value == undefined) return;
                            ngModel.$setValidity('required', value);
                        });
                    }
                }
            }

        };
    }]);

app.directive('cstLowerThenDate', ['$compile',
    function ($compile) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {
                    pre: function (scope, element, attrs, ngModel) {
                        setElementNameAndModel(scope, ngModel, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        var elementName = element.closest('form')[0].name + "." + element[0].name;
                        var errorDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.lowerThenDate',
                            "class": "error-message"
                        }).text(attrs.labelMax);
                        element.parent().append(errorDiv);
                        $compile(errorDiv)(scope);
                    },
                    post: function (scope, element, attrs, ngModel) {
                        ngModel.errorMessage = ngModel.errorMessage || {};
                        ngModel.errorMessage.lowerThenDate = attrs.label + attrs.labelMax;
                        var validate = function (viewValue) {
                            var comparisonModel = attrs.dateMaxModel;

                            if (!viewValue || !comparisonModel || viewValue == "") {
                                // It's valid because we have nothing to compare against
                                ngModel.$setValidity('lowerThenDate', true);
                                return viewValue;
                            }

                            // It's valid if model is lower than the model we're comparing against
                            if (!angular.isDate(viewValue)) viewValue = new Date(viewValue);
                            ngModel.$setValidity('lowerThenDate', viewValue <= new Date(comparisonModel.replace(/"/g, "")));
                            return viewValue;
                        };

                        ngModel.$parsers.unshift(validate);
                        ngModel.$formatters.push(validate);

                        attrs.$observe('dateMaxModel', function (comparisonModel) {
                            // Whenever the comparison model changes we'll re-validate
                            return validate(ngModel.$viewValue);
                        });
                    }
                }
            }

        };
    }]);

app.directive('cstBiggerThenDate', ['$compile',
    function ($compile) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {
                    pre: function (scope, element, attrs, ngModel) {
                        setElementNameAndModel(scope, ngModel, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        var elementName = element.closest('form')[0].name + "." + element[0].name;
                        var errorDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.biggerThenDate',
                            "class": "error-message"
                        }).text(attrs.labelMin);
                        element.parent().append(errorDiv);
                        $compile(errorDiv)(scope);
                    },
                    post: function (scope, element, attrs, ngModel) {
                        ngModel.errorMessage = ngModel.errorMessage || {};
                        ngModel.errorMessage.biggerThenDate = attrs.label + attrs.labelMin;
                        var validate = function (viewValue) {
                            var comparisonModel = attrs.dateMinModel;

                            if (!viewValue || !comparisonModel || viewValue == "") {
                                // It's valid because we have nothing to compare against
                                ngModel.$setValidity('biggerThenDate', true);
                                return viewValue;
                            }
                            if (!angular.isDate(viewValue)) viewValue = new Date(viewValue);
                            // It's valid if model is lower than the model we're comparing against
                            ngModel.$setValidity('biggerThenDate', viewValue >= new Date(comparisonModel.replace(/"/g, "")));
                            return viewValue;
                        };

                        ngModel.$parsers.unshift(validate);
                        ngModel.$formatters.push(validate);

                        attrs.$observe('dateMinModel', function (comparisonModel) {
                            // Whenever the comparison model changes we'll re-validate
                            return validate(ngModel.$viewValue);
                        });
                    }
                }
            }

        };
    }]);

app.directive('cstEmail', ['$compile',
    function ($compile) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {
                    pre: function (scope, element, attrs, modelCtrl) {
                        setElementNameAndModel(scope, modelCtrl, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        var errorDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.email',
                            "class": "error-message"
                        }).text(attrs.labelTxt);
                        element.parent().append(errorDiv);
                        $compile(errorDiv)(scope);

                        modelCtrl.errorMessage = modelCtrl.errorMessage || {};
                        modelCtrl.errorMessage.email = attrs.label + ': כתובת דוא"ל שגויה ';

                    },
                    post: function (scope, element, attrs, modelCtrl) {

                        element.on("blur", function () {
                            var email = this.value;
                            if (email != "") {
                                var testMail = /^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$/.test(email);
                                modelCtrl.$setValidity('email', testMail);
                            }
                            else
                                modelCtrl.$setValidity('email', true);
                            modelCtrl.$setViewValue(email);

                            modelCtrl.$render();
                            scope.$apply();
                        });
                    }
                }
            }
        }
    }]);

function setElementNameAndModel(scope, modelCtrl, element) {
    if (scope.$index != undefined) {
        scope[element.closest('form')[0].name].$removeControl(modelCtrl);
        var name = element[0].name;
        name = name.replace(/\{\{\$index\}\}/g, scope.$index);
        if (scope[element.closest('form')[0].name][name] != undefined) name += "_" + scope.$index;
        element[0].name = name;
        //var modelCtrl = ctrls[0];
        modelCtrl.$name = name;
        scope[element.closest('form')[0].name].$addControl(modelCtrl);
        element.on('$destroy', function () {
            scope[element.closest('form')[0].name].$removeControl(modelCtrl);
        });
    }
}

app.directive('uiBlur', function () {

    return function (scope, elem, attrs) {
        elem.bind('blur', function () {
            scope.$apply(attrs.uiBlur);
        });
    };
});


app.directive('cstPadZero', function () {
    return {
        restrict: 'A',
        replace: true,
        link: function (scope, elem, attrs) {
            elem.bind('blur', function () {
                var n = attrs.cstPadZero;
                if (elem.context.value == "" || elem.context.value.length >= n)
                    return;

                var zeros = "";
                for (i = elem.context.value.length; i < n; i++)
                    zeros += "0";

                elem.context.value = zeros + elem.context.value;
            });
        }
    };
});