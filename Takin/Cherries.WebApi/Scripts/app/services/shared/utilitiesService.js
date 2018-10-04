app.factory('utilitiesSvc', ['prompt', '$q', function (prompt, $q) {

    return {
        showOKMessage: function (header, msg, okBtnText, hebrewForamt, showNoSession) {
            if (!sessionStorage.getItem('user') && !showNoSession)
                return;
            if (okBtnText == '') okBtnText = 'Ok';
            var promptPromise = prompt({
                title: header,
                message: msg,
                heformat: hebrewForamt,
                buttons: [
                    {
                        "label": okBtnText,
                        "cancel": false,
                        "primary": true,
                        "isfocus": true
                    }
                ]
            });

            return promptPromise;
        },

        showOKMessageWithIndication: function (header, msg, okBtnText, hebrewForamt) {
            if (!okBtnText) okBtnText = 'Ok';
            var defer = $q.defer();
            prompt({
                title: header,
                message: msg,
                heformat: hebrewForamt,
                buttons: [
                    {
                        "label": okBtnText,
                        "cancel": false,
                        "primary": true,
                        "isfocus": true
                    }
                ]
            }).then(function (result) {
                defer.resolve();
            });
            return defer.promise;
        },

        showYesNoMessage: function (header, msg, yesBtnText, noBtnText) {
            var defer = $q.defer();
            prompt({
                title: header,
                message: msg,
                buttons: [
                    {
                        "label": yesBtnText,
                        "cancel": false,
                        "primary": true,
                        "class": 'btn-green'
                    },
                    {
                        "label": noBtnText,
                        "cancel": true,
                        "primary": false,
                        "class": 'bold',
                        "isfocus": true
                    }
                ]
            }).then(function (result) {
                defer.resolve();
            }, function () {
                defer.reject()
            });
            return defer.promise;
        },

        exportToPdf: function (elementId, fileName) {

            var deferred = $q.defer();

            var canvasToImage = function (canvas) {
                var img = new Image();
                var dataURL = canvas.toDataURL('image/png');
                img.src = dataURL;
                return img;
            };
            var canvasShiftImage = function (oldCanvas, shiftAmt) {
                shiftAmt = parseInt(shiftAmt) || 0;
                if (!shiftAmt) { return oldCanvas; }

                var newCanvas = document.createElement('canvas');
                newCanvas.height = oldCanvas.height - shiftAmt;
                newCanvas.width = oldCanvas.width;
                var ctx = newCanvas.getContext('2d');

                var img = canvasToImage(oldCanvas);
                ctx.drawImage(img, 0, shiftAmt, img.width, img.height, 0, 0, img.width, img.height);

                return newCanvas;
            };


            var canvasToImageSuccess = function (canvas) {
                var pdf = new jsPDF('l', 'px'),
                    pdfInternals = pdf.internal,
                    pdfPageSize = pdfInternals.pageSize,
                    pdfScaleFactor = pdfInternals.scaleFactor,
                    pdfPageWidth = pdfPageSize.width,
                    pdfPageHeight = pdfPageSize.height,
                    totalPdfHeight = 0,
                    htmlPageHeight = canvas.height,
                    htmlScaleFactor = canvas.width / (pdfPageWidth * pdfScaleFactor),
                    safetyNet = 0;

                while (totalPdfHeight < htmlPageHeight && safetyNet < 15) {
                    var newCanvas = canvasShiftImage(canvas, totalPdfHeight);
                    pdf.addImage(newCanvas, 'png', 0, 0, pdfPageWidth, 0, null, 'NONE');

                    totalPdfHeight += (pdfPageHeight * pdfScaleFactor * htmlScaleFactor);

                    if (totalPdfHeight < htmlPageHeight) {
                        pdf.addPage();
                    }
                    safetyNet++;
                }

                pdf.save(fileName + '.pdf');
                deferred.resolve();
            };
            $('body').css('overflow', 'auto');
            var h = $('.height-page-body').css('max-height');
            $('.height-page-body').css('max-height', 'none');
            html2canvas($('#' + elementId)[0], {

                onrendered: function (canvas) {
                    canvasToImageSuccess(canvas);
                    $('.height-page-body').css('max-height', h);
                    // $('body').css('overflow', 'hidden');
                }
            });

            return deferred.promise;
        }
    }
}]);