String.prototype.padLeft = function (length, c) {
    if (!c) c = "0";
    var pad = "";
    for (var i = 0 ; i < length ; i++) {
        pad += c;
    }
    return (pad + this).slice(-length);
}

