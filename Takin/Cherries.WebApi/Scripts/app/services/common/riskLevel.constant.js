(function (angular)
{
    angular.module('takinApp.common').constant('riskLevel', {

        none: {
            value: 0,
            color: '#d7dbdc',
            label: 'None'
        },

        solid: {
            value: 9,
            color: '#2fd074',
            label: 'Solid'
        },

        low: {
            value: 14,
            color: '#f9ec23',
            label: 'Low'
        },

        mod: {
            value: 25,
            color: '#f9b50a',
            label: 'Mod'
        },

        high: {
            value: 40,
            color: '#ff8812',
            label: 'High'
        },

        veryHigh: {
            value: 100,
            color: '#fc3d5e',
            label: 'Very High'
        }
    });

})(angular);