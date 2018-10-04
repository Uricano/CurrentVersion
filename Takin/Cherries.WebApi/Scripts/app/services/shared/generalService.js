app.factory('generalSvc', ['serviceHelperSvc', function (serviceHelperSvc) {
    var general = serviceHelperSvc.InvokerWithCache;
    var invok = serviceHelperSvc.Invoker;
    return {
        getGeneralObject: function (objectName) {
            return general.get({"method" : invok.createMethodName("offices")});
            //return general.get({"objectName": objectName });
           
        }

    }
}]);

app.filter('percentage', ['$filter', function ($filter) {
    return function (input, decimals) {
        return $filter('number')(input * 100, decimals) + '%';
    };
}]);

//alert('before filter');
app.filter('DistirictCountry', function () {
    return function (input, code, type) {
        var out = [];
        if (input != undefined) {
            for (var i = 0 ; i < input.length ; i++) {
                var value = input[i][type];
               
                if (value == code || value == -1) {
                    out.push(input[i])
                }
            }
        }
        return out;
    }
});