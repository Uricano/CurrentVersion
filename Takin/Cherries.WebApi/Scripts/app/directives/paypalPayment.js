app.directive('paypalPayment', ["$rootScope", "utilitiesSvc", function ($rootScope, utilitiesSvc) {
    return {
        restrict: 'E',
        templateUrl: 'scripts/app/partials/paypalPayment.min.html',
        scope: {
            model: '=',
            disable: '=',
            total: '=',
            currency:'='
        },
        link: function (scope, element, attrs, ngModelCtrl) {

            var init = function () {

                paypal.Button.render({

                    // Set your environment

                    env: 'sandbox', // sandbox | production

                    // PayPal Client IDs - replace with your own
                    // Create a PayPal app: https://developer.paypal.com/developer/applications/create

                    client: {
                        sandbox: 'AZDxjDScFpQtjWTOUtWKbyN_bDt4OgqaF4eYXlewfBP4-8aqX3PiV8e1GWU6liB2CUXlkA59kJXE7M6R',
                        production: 'Aco85QiB9jk8Q3GdsidqKVCXuPAAVbnqm0agscHCL2-K2Lu2L6MxDU2AwTZa-ALMn_N0z-s2MXKJBxqJ'
                    },

                    // Wait for the PayPal button to be clicked

                    payment: function () {

                        // Make a client-side call to the REST api to create the payment
                        
                        return paypal.rest.payment.create(this.props.env, this.props.client, {
                            transactions: [
                                {
                                    amount: { total: scope.total, currency: scope.currency }
                                }
                            ]
                        });
                    },

                    // Wait for the payment to be authorized by the customer

                    onAuthorize: function (data, actions) {

                        // Execute the payment

                        return actions.payment.execute().then(function (res) {

                         //   document.querySelector('#paypal-button-container').innerText = 'Payment Complete!';
                            $rootScope.$broadcast('paypalPaymentComplited', res);

                        }, function (error) {
                            utilitiesSvc.showOKMessage('error', 'Payment failed', 'OK');
                        });
                    }

                }, '#paypal-button-container');

            }

            init();

        }
    }
}]);
