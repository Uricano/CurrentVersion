app.factory('errorHandler', ['prompt', function (prompt) {
    return {
        checkErrors: function (data) {
            var errorMessage = []
            if (data.Messages!= null && data.Messages.length > 0) {
                errorMessage = Enumerable.From(data.Messages)
                    .Where(function (x) { return x.LogLevel == Log.Level.Fatal || x.LogLevel == Log.Level.Error })
                    .Select(function (x) { return x.Text }).ToArray();
                
            }
            else if (data.ModelState != null) {
                for (var f in data.ModelState) {
                    if (f != '$id') {
                        for (var i = 0 ; i < data.ModelState[f].length ; i++)
                            errorMessage.push(data.ModelState[f][i]);
                    }
                }
            }
            if (errorMessage.length > 0) {
                var msg = errorMessage.join("<br>");
                prompt({
                    title: 'Error',
                    message: msg,
                    buttons: [
                            {
                                "label": "OK",
                                "cancel": false,
                                "primary": true
                            }
                    ]
                })
                return true;
            }
            return false;
        }
    }
}]);