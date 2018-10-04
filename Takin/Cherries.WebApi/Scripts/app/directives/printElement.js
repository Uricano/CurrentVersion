
//  var mod = angular.module('ngPrint', []);
app.directive('ngPrint', ['$timeout', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {

            // if there is no printing section, create one
            var printSection = document.getElementById('printSection');

            if (!printSection) {
                printSection = document.createElement('div');
                printSection.id = 'printSection';
                document.body.appendChild(printSection);
            }

            element.on('click', function () {
                var elemToPrint = document.getElementById(attrs.printElementId);
                if (elemToPrint) {
                    printElement(elemToPrint);
                }
            });

            window.onafterprint = function () {
                // clean the print section before adding new content
                printSection.innerHTML = '';
            };

            var printElement = function (elem) {
                var domClone = elem.cloneNode(true);

                printSection.innerHTML = '';
                printSection.innerHTML = elem.innerHTML
                w = window.open();
                w.document.write(printSection.innerHTML);
                w.document.body.style.direction = 'rtl';
                $timeout(function () {
                    w.print();
                    $timeout(function () {
                        w.close();
                    }, 2000);
                }, 100);

            }

          
            
        }
    }
}]);

