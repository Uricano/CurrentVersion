angular.module('cgPrompt',['ui.bootstrap']);

angular.module('cgPrompt').factory('prompt',['$modal','$q',function($modal,$q){

    var prompt = function(options){

        var defaults = {
            title: '',
            message: '',
            input: false,
            label: '',
            value: '',
            values: false,
            closeAfter: null,
            withCaptcha: null,
            lockBack: null,
            buttons: [
                {label:'Cancel',cancel:true},
                {label:'OK',primary:true}
            ]
        };

        if (options === undefined){
            options = {};
        }

        for (var key in defaults) {
            if (options[key] === undefined) {
                options[key] = defaults[key];
            }
        }

        var defer = $q.defer();

        $modal.open({
            templateUrl:'angular-prompt.html',
            controller: 'cgPromptCtrl',
            backdrop: options.lockBack,
            resolve: {
                options:function(){ 
                    return options; 
                }
            }
        }).result.then(function(result){
            if (options.input){
                defer.resolve(result.input);
            } else {
                defer.resolve(result.button);
            }
        }, function(){
            defer.reject();
        });

        return defer.promise;
    };

    return prompt;
	}
]);

angular.module('cgPrompt').controller('cgPromptCtrl', ['$scope', 'options', '$timeout', '$rootScope', function ($scope, options, $timeout, $rootScope) {

    $scope.captchaStatus;

    $scope.input = {name:options.value};

    $scope.options = options;

    $scope.buttonClicked = function(button){

        if ($scope.options.withCaptcha && $scope.captchaStatus == null) {
                return;
        }
        if (button.cancel) {
            $scope.$dismiss();
            return;
        }
        if (options.input && angular.element(document.querySelector('#cgPromptForm')).scope().cgPromptForm.$invalid){
            $scope.changed = true;
            return;
        }
        if ($scope.options.closeAfter != null) {
            $scope.$dismiss();
        }
        else {
            $scope.$close({ button: button, input: $scope.input.name });
        }
    };

    $scope.submit = function(){
        var ok;
        angular.forEach($scope.options.buttons,function(button){
            if (button.primary){
                ok = button;
            }
        });
        if (ok){
            $scope.buttonClicked(ok);
        }
    };

    $timeout(function(){
        var elem = document.querySelector('#cgPromptInput');
        if (elem) {
            if (elem.select) {
                elem.select();
            }
            if (elem.focus) {
                elem.focus();
            }
        }
    },100);
    
    if ($scope.options.closeAfter) {
        $timeout(function () {
            $scope.$dismiss();
            return;
        }, $scope.options.closeAfter * 1000);
    }

    $rootScope.$on('submitCaptcha', function (event, captchaResult) {
        $scope.captchaStatus = captchaResult;
    })
}]);


angular.module('cgPrompt').run(['$templateCache', function($templateCache) {
  'use strict';

  $templateCache.put('angular-prompt.html',
    "<div>\n" +
    "    <div class=\"modal-header\">\n" +
    "        <button type=\"button\" class=\"close\" ng-class=\"{'pull-left':options.heformat}\" ng-click=\"$dismiss()\" aria-hidden=\"true\">×</button>\n" +
    "        <h4 class=\"modal-title\">{{options.title}}</h4>\n" +
    "    </div>\n" +
    "    <div class=\"modal-body\">\n" +
    "\n" +
    "       <p ng-if=\"options.message\">\n" +
    "           <div ng-bind-html='options.message'></div>\n" +
    "        </p>\n" +
    "\n" +
    "        <form id=\"cgPromptForm\" name=\"cgPromptForm\" ng-if=\"options.input\" ng-submit=\"submit()\">\n" +
    "            <div class=\"form-group\" ng-class=\"{'has-error':cgPromptForm.$invalid && changed}\">\n" +
    "                <label for=\"cgPromptInput\">{{options.label}}</label>\n" +
    "                <input id=\"cgPromptInput\" type=\"text\" class=\"form-control\"  placeholder=\"{{options.label}}\" ng-model=\"input.name\" required ng-change=\"changed=true\" ng-if=\"!options.values || options.values.length === 0\"/ autofocus=\"autofocus\">\n" +
    "                <div class=\"input-group\" ng-if=\"options.values\">\n" +
    "                    <input id=\"cgPromptInput\" type=\"text\" class=\"form-control\" placeholder=\"{{options.label}}\" ng-model=\"input.name\" required ng-change=\"changed=true\" autofocus=\"autofocus\"/>\n" +
    "\n" +
    "                    <div class=\"input-group-btn\" dropdown>\n" +
    "                        <button type=\"button\" class=\"btn btn-default dropdown-toggle\" dropdown-toggle data-toggle=\"dropdown\"><span class=\"caret\"></span></button>\n" +
    "                        <ul class=\"dropdown-menu pull-right\">\n" +
    "                            <li ng-repeat=\"value in options.values\"><a href=\"\" ng-click=\"input.name = value\">{{value}}</a></li>\n" +
    "                        </ul>\n" +
    "                    </div>\n" +
    "                </div>\n" +
    "            </div>\n" +
    "         </form>\n" +
    "\n" +
    "    </div>\n" +
    "<div ng-show='options.withCaptcha'><captcha></captcha></div>" +
    "    <div class=\"modal-footer\">\n" +
    "        <button ng-repeat=\"button in options.buttons track by button.label\" class=\"btn btn-default {{button.class}}\" ng-class=\"{'btn-primary':button.primary,'btn-green':button.primary,'btn-darkblue':!button.primary}\" focus focusme=\"{{button.isfocus}}\" ng-click=\"buttonClicked(button)\"  ng-disabled=\"options.withCaptcha && captchaStatus == null\">{{button.label}}</button>\n" +
    "    </div>\n" +
    "</div>"
  );

}]);