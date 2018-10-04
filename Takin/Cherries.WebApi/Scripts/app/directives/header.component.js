(function (angular)
{
    angular.module('takinApp')
        .component('tfiHeader', {
            template:
                '<div class="tfi-header__nav-container tfi-header-lg">' +
                    '<a href="https://www.gocherries.com" target="_blank">' +
                        '<img src="content/themes/images/logo white clear background.png" class="tfi-header__logo" />' +
                    '</a>' +
                    '<div class="tfi-header__navbar">' +
                        '<div class="tfi-header__navbar-item">' +
                            '<a href="portfolios" ng-class="{\'is-selected\':$ctrl.selectedIndex == 1}">Portfolio Universe</a>' +
                        '</div>' +
                        '<div class="tfi-header__navbar-item">' +
                            '<a href="BuildNewPortfolio" ng-class="{\'is-selected\':$ctrl.selectedIndex == 2}">Build New Portfolio</a>' +
                        '</div>' +
                        '<div class="tfi-header__navbar-item">' +
                            '<a href="createbacktesting" ng-class="{\'is-selected\':$ctrl.selectedIndex == 3}">Backtesting</a>' +
                        '</div>' +
                    '</div>' +
                '</div>' +
                '<div class="regular-font tfi-header__links tfi-header-lg">' +
                    '<span ng-show="$ctrl.isDataReady">{{ $ctrl.objName }} Ready</span>' +
                    '<img src="content/themes/images/notification-outline.png" width="24" ' +
                        'ng-show="$ctrl.isDataReady" style="cursor:pointer" ng-click="$ctrl.showNewPortfolio()" ' +
                        'height="24" alt="notifications" />' +
                    '<span class="white-pipe-line"><span translate="Greeting"></span><span ng-bind="$ctrl.userName"></span></span>' +
                    '<a class="regular-font white-txt white-pipe-line" ng-click="$ctrl.showHideAccount($event)" ' +
                        'translate="Account">' +
                    '</a>' +
                    '<a translate="Logout" class="regular-font white-txt white-pipe-line" ng-click="$ctrl.logout()"></a>' +
                '</div>' +
                '<div class="tfi-header__account-info account-tp-padding tfi-header-lg" ' +
                    'ng-click="$ctrl.accountPnlClick($event)">' +
                    '<div class="tfi-header__account-info-container">' +
                        '<div class="no-padding">' +
                            '<div class="no-padding">' +
                                '<div class="tfi-header__account-info__label middle-label black-gray-color">License details</div>' +
                                '<div>' +
                                    '<span class="bold-text black-gray-color account-large-font no-padding">{{ $ctrl.licenseType }}</span>' +
                                '</div>' +
                                '<div class="tfi-header__account-info__label middle-label black-gray-color">Exchanges:</div>' +
                                '<div>' +
                                    '<span class="bold-text account-large-font black-gray-color tfi-header__account-info__exchange" ng-repeat="m in $ctrl.marketStock">' +
                                        '{{m.HebName}}' +
                                    '</span>' +
                                '</div>' +
                            '</div>' +
                        '</div>' +
                        '<div class="account-gray-splitter tfi-header__account-info__dates">' +
                            '<div class="tfi-header__account-info__label black-gray-color middle-label">Expiry Date</div>' +
                            '<div class="bold-text black-gray-color account-large-font">{{ $ctrl.expiryDate | date: \'dd.MM.yyyy\'}}</div>' +
                            '<div class="tfi-header__account-info__label black-gray-color middle-label">Days Left</div>' +
                            '<div class="bold-text black-gray-color account-large-font">{{ $ctrl.daysLeft }}</div>' +
                        '</div>' +
                    '</div>' +
                '</div>' +
                '<div class="tfi-header-sm">' +
                    '<a href="https://www.gocherries.com" target="_blank">' +
                        '<img src="content/themes/images/logo white clear background.png" class="tfi-header__logo-sm" />' +
                    '</a>' +
                    '<div class="tfi-header__expand-icon" ng-click="$ctrl.expandClick($event)">' +
                        '<img ng-show="$ctrl.menuExpanded" src="content/themes/images/close_icon.png" />' +
                        '<img ng-show="!$ctrl.menuExpanded" src="content/themes/images/hamburger_menu_icon.png" />' +
                    '</div>' +
                    '<div ng-show="$ctrl.menuExpanded" class="tfi-header__nav-container-sm">' +
                        '<div>Expand: {{$ctrl.menuExpanded}}</div>' +
                        '<div class="tfi-header__navbar-item">' +
                            '<a href="portfolios" ng-class="{\'is-selected\':$ctrl.selectedIndex == 1}" ng-click="$ctrl.menuClick()">Portfolio Universe</a>' +
                        '</div>' +
                        '<div class="tfi-header__navbar-item">' +
                            '<a href="BuildNewPortfolio" ng-class="{\'is-selected\':$ctrl.selectedIndex == 2}" ng-click="$ctrl.menuClick()">Build New Portfolio</a>' +
                        '</div>' +
                        '<div class="tfi-header__navbar-item">' +
                            '<a href="createbacktesting" ng-class="{\'is-selected\':$ctrl.selectedIndex == 3}" ng-click="$ctrl.menuClick()"">Backtesting</a>' +
                        '</div>' +
                        '<div class="tfi-header__navbar-separator"></div>' +
                        '<div class="tfi-header__navbar-item">' +
                            '<a translate="Logout" ng-click="$ctrl.logout()"></a>' +
                        '</div>' +                        
                    '</div>' +

                '</div>',
            controller: HeaderController,
            bindings: {
                userName: '<tfiUserName'
            },
            bindToController: true
        });

    HeaderController.$inject = ['$scope', '$element', '$rootScope', 'tfiCommonConstants', 'session', 'loginSvc', '$location', '$window', '$timeout'];

    function HeaderController($scope, $element, $rootScope, tfiCommonConstants, session, loginSvc, $location, $window, $timeout)
    {
        this.selectedIndex = 0;

        var self = this;
        var unsubscribes = [];
        var _window = angular.element($window);
        var accountInfoElement = $element.find('.tfi-header__account-info');
        var menuSmElement = $element.find('.tfi-header__nav-container-sm');

        this.$onInit = $onInit;
        this.$onDestroy = $onDestroy;
        this.showNewPortfolio = showNewPortfolio;
        this.showHideAccount = showHideAccount;
        this.logout = logout;
        this.accountPnlClick = accountPnlClick;
        this.menuClick = menuClick;
        this.expandClick = expandClick;
        this.objName = null;
        this.isDataReady = false;
        this.menuExpanded = false;
        
        function $onInit()
        {
            accountInfoElement.hide();
            //menuSmElement.hide();

            $element.addClass('tfi-header');

            _window.bind('click', windowOnClick);

            unsubscribes.push(function ()
            {
                _window.unbind('click', windowOnClick);
            });

            unsubscribes.push($scope.$on(tfiCommonConstants.updateAccountData, function (event, user)
            {
                saveAccountData(user);
            }));

            unsubscribes.push($scope.$on(tfiCommonConstants.objNameChanged, function (event, objName)
            {
                self.objName = objName;
            }));

            unsubscribes.push($scope.$on(tfiCommonConstants.isDataReady, function (event, isDataReady)
            {
                self.isDataReady = isDataReady;
            }));

            unsubscribes.push($rootScope.$on(tfiCommonConstants.menuItemChanged,
                function (event, menuItemIndex)
                {
                    self.selectedIndex = menuItemIndex;
                })
            );

            var user = session.getUserDetails();
            if (user)
            {
                saveAccountData(user);
            }
        }

        function $onDestroy()
        {
            for (var i = 0; i < unsubscribes.length; i++)
            {
                unsubscribes[i]();
            }
        }

        function showNewPortfolio() { }

        function showHideAccount(event)
        {
            event.stopPropagation();

            if (accountInfoElement.is(':visible'))
            {
                accountInfoElement.hide();
            }
            else
            {
                accountInfoElement.show();
            }
        }

        function windowOnClick(event)
        {
            accountInfoElement.hide();
           // menuSmElement.hide();

            self.menuExpanded = false;
        }

        function logout()
        {
            loginSvc.logoff().$promise.then(function (result)
            {
                session.clearSession();
                self.isDataReady = false;

                $rootScope.$broadcast(tfiCommonConstants.isInProgress, false);
                $rootScope.$broadcast(tfiCommonConstants.wantToLogout, true);
                scope.wantToLogout = true;
                $location.path("/");
            });}

        function accountPnlClick(event)
        {
            event.stopPropagation();
        }

        function menuClick()
        {
           // menuSmElement.hide();
            self.menuExpanded = false;
        }

        function expandClick(event)
        {
            //if (menuSmElement.is(':visible'))
            //{
            //    menuSmElement.hide();
            //}
            //else
            //{
            //    menuSmElement.show();
            //}
            event.stopPropagation();
            self.menuExpanded = !self.menuExpanded;
        }

        function saveAccountData(user)
        {
            self.marketStock = user.Licence.Stocks;
            for (var i = 0; i < self.marketStock.length; i++)
            {
                self.marketStock[i].Name = self.marketStock[i].Name.substring(0, self.marketStock[i].Name.indexOf('Stock E'));
            }
            self.expiryDate = user.Licence.ExpiryDate;
            self.daysLeft = getDaysBetween(new Date(self.expiryDate), new Date());
            self.licenseType = user.Licence.Service.StrServiceType
        }

        var getDaysBetween = function (date1, date2)
        {
            var ONE_DAY = 1000 * 60 * 60 * 24;

            // Convert both dates to milliseconds
            var date1_ms = date1.getTime();
            var date2_ms = date2.getTime();

            // Calculate the difference in milliseconds
            var difference_ms = date1_ms - date2_ms

            // Convert back to days and return
            return Math.round(difference_ms / ONE_DAY);
        }
    }
})(angular);