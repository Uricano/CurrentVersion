app.factory('exportDataSvc', function () {
    var download = function (data, getResponseHeaders, fileName) {
        var octetStreamMime = 'application/octet-stream';
        var success = false;

        // Get the headers
        // headers = getResponseHeaders();

        // Get the filename from the x-filename header or default to "download.bin"
        var filename = fileName;

        // Determine the content type from the header or default to "application/octet-stream"
        var contentType = octetStreamMime;

        var binary_string = window.atob(data);
        var len = binary_string.length;
        var bytes = new Uint8Array(len);
        for (var i = 0; i < len; i++) {
            bytes[i] = binary_string.charCodeAt(i);
        }
        data = bytes;
        try {
            // Try using msSaveBlob if supported
            var blob = new Blob([data], { type: contentType });
            if (navigator.msSaveBlob)
                navigator.msSaveBlob(blob, filename);
            else {
                // Try using other saveBlob implementations, if available
                var saveBlob = navigator.webkitSaveBlob || navigator.mozSaveBlob || navigator.saveBlob;
                if (saveBlob === undefined) throw "Not supported";
                saveBlob(blob, filename);
            }
            success = true;
        } catch (ex) {

        }

        if (!success) {
            // Get the blob url creator
            var iOS = /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;
            if (iOS){
                var reader = new FileReader();
                reader.onload = function (e) {
                    var bdata = btoa(reader.result);
                    var datauri = 'data:' + 'text/csv;charset=utf-8;' + ';base64,' + bdata;
                    newWindow = window.open(datauri);
                    newWindow.document.title = filename;
                };
                var blob = new Blob([data], { type: contentType });
                reader.readAsBinaryString(blob);
                return;
            }
            var urlCreator = window.URL || window.webkitURL || window.mozURL || window.msURL;
            if (urlCreator) {
                // Try to use a download link
                var link = document.createElement('a');
                try {
                    // Prepare a blob URL

                    var blob = new Blob([data], { type: contentType });
                    var url = urlCreator.createObjectURL(blob);
                    link.setAttribute('href', url);

                    // Set the download attribute (Supported in Chrome 14+ / Firefox 20+)
                    link.setAttribute("download", filename);

                    // Simulate clicking the download link
                    var event = document.createEvent('MouseEvents');
                    event.initMouseEvent('click', true, true, window, 1, 0, 0, 0, 0, false, false, false, false, 0, null);
                    link.dispatchEvent(event);

                    success = true;

                } catch (ex) {

                }

                if (!success) {
                    // Fallback to window.location method
                    try {
                        // Prepare a blob URL
                        // Use application/octet-stream when using window.location to force download

                        var blob = new Blob([data], { type: octetStreamMime });
                        var url = urlCreator.createObjectURL(blob);
                        window.location = url;
                        success = true;
                    } catch (ex) {

                    }
                }

            }
        }

        if (!success) {
            alert('היצוא נכשל');
        }
    }

    return {
        download: download
    }
});