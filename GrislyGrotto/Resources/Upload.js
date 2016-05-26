var Upload = (function () {
    function Upload() { }
    Upload.getParameterByName = function getParameterByName(name) {
        var match = RegExp('[?&]' + name + '=([^&]*)').exec(window.location.search);
        return match && decodeURIComponent(match[1].replace(/\+/g, ' '));
    };
    Upload.initCheck = function initCheck() {
        if(Upload.getParameterByName('success') == 'true') {
            var key = Upload.getParameterByName('key');
            key = '/usercontent/' + key;
            $('.imageUrl').attr('href', key);
            $('.imageUrl').text(key);
            $('.successMessage').show();
        }
    };
    return Upload;
})();
$().ready(function () {
    Upload.initCheck();
});
