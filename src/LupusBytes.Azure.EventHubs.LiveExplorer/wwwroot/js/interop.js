window.clipboardInterop = {
    writeText: function (text) {
        return navigator.clipboard.writeText(text);
    }
};

window.fileInterop = {
    download: function (filename, contentType, content) {
        var blob = new Blob([content], { type: contentType });
        var url = URL.createObjectURL(blob);
        var a = document.createElement("a");
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }
};
