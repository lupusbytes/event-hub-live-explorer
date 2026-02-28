window.clipboardInterop = {
    writeText: function (text) {
        return navigator.clipboard.writeText(text);
    }
};
