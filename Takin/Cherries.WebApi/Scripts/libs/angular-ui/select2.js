/**
 * Enhanced Select2 Dropmenus
 * When you define data source in ng-repeat you also need to define 2 attributes for id and description like this: <select ui-select2 code="ID" text="Name" />
 * you can make the select2 multi languae if you define the attribute lang to a variable that you control like this : <select ui-select2 lang="{{$parent.language}}" />
 * @AJAX Mode - When in this mode, your value will be an object (or array of objects) of the data used by Select2
 *     This change is so that you do not have to do an additional query yourself on top of Select2's own query
 * @params [options] {object} The configuration options passed to $.fn.select2(). Refer to the documentation
 */
//$.fn.select2.defaults.set('debug', true);
//$.fn.select2.defaults.set('amdLanguageBase', './i18n/');
$.fn.select2.defaults.set('language', 'en');
$.fn.select2.amd.define('select2/data/customAdapter', ['select2/data/array', 'select2/utils'],
    function (ArrayData, Utils) {
        function CustomDataAdapter($element, options) {
            CustomDataAdapter.__super__.constructor.call(this, $element, options);
        }

        Utils.Extend(CustomDataAdapter, ArrayData);

        CustomDataAdapter.prototype.current = function (callback) {
            var found = [],
                findValue = null,
     			initialValue = this.options.options.initialValue,
                selectedValue = this.$element.val(),
            	jsonData = this.options.options.jsonData,
            	jsonMap = this.options.options.jsonMap;

            if (initialValue !== null && initialValue != undefined) {
                findValue = initialValue;
                this.options.options.initialValue = null;  // <-- set null after initialized              
            }
            else if (selectedValue !== null) {
                findValue = selectedValue;
            }

            if (!this.$element.prop('multiple')) {
                findValue = [findValue];
                this.$element.html();     // <-- if I do this for multiple then it breaks
            }

            this.options.options.jsonData = [];

            for (var i = 0 ; i < this.$element[0].options.length ; i++) {
                this.options.options.jsonData.push({ "Code": this.$element[0].options[i].value, "Description": this.$element[0].options[i].text });
                for (var v = 0; v < findValue.length; v++) {
                    if (findValue[v] == this.$element[0].options[i].value)
                        found.push({ id: this.$element[0].options[i].value, text: this.$element[0].options[i].text });
                }
            }


            // Set found matches as selected
            this.$element.find("option").prop("selected", false).removeAttr("selected");
            for (var v = 0; v < found.length; v++) {
                this.$element.find("option[value='" + found[v].id + "']").prop("selected", true).attr("selected", "selected");
            }

            //// If nothing was found, then set to top option (for single select)
            //if (!found.length && !this.$element.prop('multiple')) {  // default to top option 
            //    found.push({ id: jsonData[0][jsonMap.id], text: jsonData[0][jsonMap.text] });
            //    this.$element.html(new Option(jsonData[0][jsonMap.text], jsonData[0][jsonMap.id], true, true));
            //}

            callback(found);
        };

        CustomDataAdapter.prototype.query = function (params, callback) {
            if (!("page" in params)) {
                params.page = 1;
            }

            var jsonData = this.options.options.jsonData,
                pageSize = this.options.options.pageSize,
                jsonMap = this.options.options.jsonMap;

            var results = $.map(jsonData, function (obj) {
                // Search
                if (new RegExp(params.term, "i").test(obj[jsonMap.text])) {
                    return {
                        id: obj.Code,
                        text: obj.Description
                    };
                }
            });

            callback({
                results: results.slice((params.page - 1) * pageSize, params.page * pageSize),
                pagination: {
                    more: results.length >= params.page * pageSize
                }
            });
        };

        return CustomDataAdapter;

    });

var jsonAdapter = $.fn.select2.amd.require('select2/data/customAdapter');


angular.module('ui.select2', []).value('uiSelect2Config', {}).directive('uiSelect2', ['uiSelect2Config', '$timeout', function (uiSelect2Config, $timeout) {
    var options = {};
    if (uiSelect2Config) {
        angular.extend(options, uiSelect2Config);
    }
    return {
        require: 'ngModel',
        priority: 1,
        compile: function (tElm, tAttrs) {
            var watch,
              repeatOption,
              repeatAttr,
              isSelect = tElm.is('select'),
              dataSource = [],
              pageSize = 20,
              select2Initialized = false;
            isMultiple = angular.isDefined(tAttrs.multiple);
            if (tAttrs.code == undefined) tAttrs.code = "Code";
            if (tAttrs.text == undefined) tAttrs.text = "Description";
            // Enable watching of the options dataset if in use
            if (tElm.is('select')) {
                repeatOption = tElm.find('optgroup[ng-repeat], optgroup[data-ng-repeat], option[ng-repeat], option[data-ng-repeat]');

                if (repeatOption.length) {
                    repeatAttr = repeatOption.attr('ng-repeat') || repeatOption.attr('data-ng-repeat');
                    watch = jQuery.trim(repeatAttr.split('|')[0]).split(' ').pop();
                }
                
            }

            return function (scope, elm, attrs, controller) {
                // instance-specific options
                if (attrs.code == undefined) attrs.code = "Code";
                if (attrs.text == undefined) attrs.text = "Description";
                var opts = initSelect2();
                $timeout(function () {
                    for (var i = 0; i < tElm[0].options.length ; i++) {
                        if (tElm[0].options[i].getAttribute('ng-repeat') == null && tElm[0].options[i].getAttribute('data-ng-repeat') == null) {
                            var obj = {};
                            obj[tAttrs["code"]] = tElm[0].options[i].value;
                            obj[tAttrs["text"]] = tElm[0].options[i].text;
                            dataSource.push(obj);
                        }
                    }
                    opts = initSelect2();

                    select2Initialized = true;
                    opts.initialValue = controller.$viewValue;
                    elm.select2(opts);
                    //setDataSource()
                    var isPristine = controller.$pristine;
                    controller.$pristine = true
                    controller.$setPristine();
                    elm.prev().toggleClass('ng-pristine', controller.$pristine);

                }, 1000);

                if (controller) {
                    // Watch the model for programmatic changes
                    scope.$watch(tAttrs.ngModel, function (current, old) {
                        //if (!select2Initialized) return;
                        opts.initialValue = current;
                        elm.select2(opts);
                    }, true);


                    // Watch the options dataset for changes
                    if (watch) {
                        scope.$watch(watch, function (newVal, oldVal, scope) {
                            if (newVal == undefined) return;
                            //if (angular.equals(newVal, oldVal)) {
                            //    return;
                            //}
                            opts = initSelect2();
                            dataSource = [];
                            for (var i = 0 ; i < newVal.length ; i++) {
                                dataSource.push(newVal[i]);
                            }
                            opts.jsonData = dataSource;
                            opts.initialValue = controller.$viewValue;

                            elm.select2(opts);
                            if (newVal && !oldVal && controller.$setPristine) {
                                controller.$setPristine(true);
                            }

                        });
                    }
                    
                    attrs.$observe('lang', function (value) {
                        if (value == undefined) return;
                        elm.select2();
                    });
                    // Update valid and dirty statuses
                    controller.$parsers.push(function (value) {
                        var div = elm.prev();
                        div
                          .toggleClass('ng-invalid', !controller.$valid)
                          .toggleClass('ng-valid', controller.$valid)
                          .toggleClass('ng-invalid-required', !controller.$valid)
                          .toggleClass('ng-valid-required', controller.$valid)
                          .toggleClass('ng-dirty', controller.$dirty)
                          .toggleClass('ng-pristine', controller.$pristine);
                        return value;
                    });


                }

                elm.bind("$destroy", function () {
                    elm.select2("destroy");
                });

                function initSelect2() {
                    var opts = angular.extend({}, options, scope.$eval(attrs.uiSelect2));
                    //opts.dir = "rtl";
                    opts.theme = "classic";
                    opts.language = "en";

                    if (opts.ajax == undefined) {
                        opts.ajax = {};
                        opts.jsonData = dataSource;
                        opts.jsonMap = { id: attrs.code, text: attrs.text };
                        opts.initialValue = 50;
                        opts.pageSize = 20;
                        opts.dataAdapter = jsonAdapter;
                    }
                    return opts;
                }


            };
        }
    };
}]);


