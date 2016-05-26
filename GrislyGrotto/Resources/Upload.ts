/// <reference path="jquery.d.ts" />

class Upload {

    private static getParameterByName(name: string): string {
        var match = RegExp('[?&]' + name + '=([^&]*)').exec(window.location.search);
        return match && decodeURIComponent(match[1].replace(/\+/g, ' '));
    }

    public static initCheck(): void {
        if (getParameterByName('success') == 'true') {
            var key = getParameterByName('key');
            key = '/usercontent/' + key;
            $('.imageUrl').attr('href', key);
            $('.imageUrl').text(key);
            $('.successMessage').show();
        }
    }
}

$().ready(function () {
    Upload.initCheck();
});