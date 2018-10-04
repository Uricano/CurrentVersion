(function ($) {
    $.extend($.datepicker, {

        // Reference the orignal function so we can override it and call it later
        _inlineDatepicker2: $.datepicker._inlineDatepicker,

        // Override the _inlineDatepicker method
        _inlineDatepicker: function (target, inst) {

            // Call the original
            this._inlineDatepicker2(target, inst);

            var beforeShow = $.datepicker._get(inst, 'beforeShow');

            if (beforeShow) {
                beforeShow.apply(target, [target, inst]);
            }
        }
    });

    $.datepicker._gotoToday = function (id) {
        var target = $(id);
        var date = new Date();
        if (target.attr('allowd-days') != null) {
            var days = target.attr('allowd-days').split(',');
            if (days.indexOf(date.getDay().toString() == -1)) return;
        }
        var inst = this._getInst(target[0]);
        if (this._get(inst, 'gotoCurrent') && inst.currentDay) {
            inst.selectedDay = inst.currentDay;
            inst.drawMonth = inst.selectedMonth = inst.currentMonth;
            inst.drawYear = inst.selectedYear = inst.currentYear;
        }
        else {

            inst.selectedDay = date.getDate();
            inst.drawMonth = inst.selectedMonth = date.getMonth();
            inst.drawYear = inst.selectedYear = date.getFullYear();
            // the below two lines are new
            this._setDateDatepicker(target, date);
            this._selectDate(id, this._getDateDatepicker(target));
        }
        this._notifyChange(inst);
        this._adjustDate(target);
    }
}(jQuery));


utilities.directive('cstDatepicker', ['$timeout', function ($timeout) {
    return {
        restrict: "A",
        require: 'ngModel',
        replace: true,
        compile: function (tElement)
        {
            tElement.addClass('cst-datepicker');
            return linkDirective;
        }
    };

    function linkDirective(scope, element, attrs, ngModelCtrl)
    {
        var formatDate = attrs.formatDate ? attrs.formatDate : "dd/mm/yy";
        var allowdDays = attrs.allowdDays ? attrs.allowdDays.split(',') : [0, 1, 2, 3, 4, 5, 6];
        $timeout(function ()
        {
            var startData = new Date();
            startData.setMonth(startData.getMonth() - 18);

            var dPicker = element.datepicker({
                showOn: "button",
                buttonImage: "Content/themes/images/Calandar.png",
                buttonImageOnly: true,
                buttonText: "Select date",
                changeMonth: true,
                changeYear: true,
                showButtonPanel: true,
                dateFormat: formatDate,
                minDate: startData,
                maxDate: new Date(),
                //yearRange: "2016:+0",
                onSelect: function (d) {
                    var date = $(this).datepicker('getDate');
                    var h = date.getHours();
                    date.setHours(h + 3);

                    ngModelCtrl.$setViewValue(date);
                    scope.$apply();
                    ngModelCtrl.$setValidity('invalid', true);
                },
                onClose: function (dateText, inst) {
                    //var month = $("#ui-datepicker-div .ui-datepicker-month :selected").val();
                    //var year = $("#ui-datepicker-div .ui-datepicker-year :selected").val();

                    //if (formatDate == "mm/yy" || formatDate == "yy/mm") {
                    //    dateText = new Date(year, month, 1);
                    //    $(this).datepicker('setDate', new Date(year, month, 1, 2));
                    //    ngModelCtrl.$setViewValue(dateText);
                    //    scope.$apply();

                    //}
                    ngModelCtrl.$setValidity('invalid', true);
                },
                onChangeMonthYear: function (year, month, inst) {
                    setTimeout(function () {
                        $($('.ui-datepicker-calendar').find('a')[0]).addClass('ui-state-active');
                    }, 100);
                },
                beforeShow: function () {

                    setTimeout(function () {
                        if ($('.modal').css('display') == 'block') {
                            $('.modal').css('display', '');
                            setTimeout(function () {
                                $('.modal').css('display', 'block');
                            }, 50);
                        }
                    }, 1)
                    //if (formatDate == "mm/yy" || formatDate == "yy/mm") {
                    //    if ($('#hide-days').length == 0)
                    //        $('head').append('<style id="hide-days">.ui-datepicker-calendar { display: none; } </style>');
                    //    if ((selDate = $(this).val()).length > 0) {
                    //        iYear = selDate.substring(selDate.length - 4, selDate.length);
                    //        iMonth = parseInt(selDate.substring(0, selDate.length - 5)) - 1;
                    //        $(this).datepicker('option', 'defaultDate', new Date(iYear, iMonth, 1));
                    //        $(this).datepicker('setDate', new Date(iYear, iMonth, 1));
                    //    }
                    //}
                    //else
                    //    $('#hide-days').remove();

                },
                beforeShowDay: function (date) {
                    var day = date.getDay();
                    return [(allowdDays.indexOf(day.toString()) > -1), ''];
                }
            });

            //element.on('keypress', function (e) {
            //    return false;
            //});

            ngModelCtrl.$render();
        });

        if (attrs.yearRange) $(element).datepicker('option', "yearRange", attrs.yearRange);
        if (attrs.monthRange) $(element).datepicker('option', "monthRange", attrs.monthRange);

        attrs.$observe('disabled', function (value) {
            if (value == undefined) return;
            $(element).datepicker("option", "disabled", value);
        });

        ngModelCtrl.$render = function () {
            var date = ngModelCtrl.$viewValue;
            if (date == "") date = null;
            else if (typeof (date) == "string") {
                date = new Date(date);
                if (date.getFullYear() == 1) {
                    date = null;
                }
            }
            if (angular.isDefined(date) && date !== null && !angular.isDate(date)) {
                throw new Error('ng-Model value must be a Date object - currently it is a ' + typeof date + ' - use ui-date-format to convert it from a string');
            }
            element.datepicker("setDate", date);
        };
        element.on('blur', function (e) {
            if (!$(this).datepicker("widget").is(":visible")) {
                var d;
                if (this.value == "") return;
                var dateParts = this.value.split('/');
                if (formatDate == "mm/yy")
                    d = new Date(dateParts[1], dateParts[0] - 1, 1);
                else if (formatDate == "yy/mm")
                    d = new Date(dateParts[0], dateParts[1] - 1, 1);
                else
                    try {
                        d = $.datepicker.parseDate(formatDate, this.value);
                    }
                    catch (e) {
                        d = "";
                    }
                if (!angular.isDate(d) || d.toString() == "Invalid Date")
                    ngModelCtrl.$setViewValue("");
                else {
                    d.setHours(2);
                    ngModelCtrl.$setViewValue(d);
                }
                ngModelCtrl.$render();
                scope.$apply();
            }
        })
    }
}]);