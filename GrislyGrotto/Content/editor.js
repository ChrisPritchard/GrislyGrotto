/// <reference path="jquery.d.ts" />
var GrislyGrotto;
(function (GrislyGrotto) {
    var Editor = (function () {
        function Editor($editorDiv, $htmlSwitch) {
            this.isHtml = false;
            this.intervalForSaves = 10000;
            this.saveLabelHide = 1000;
            this.$editorDiv = $editorDiv;
            this.$htmlSwitch = $htmlSwitch;
        }
        Editor.prototype.setupContentEditing = function ($editorHidden) {
            var _this = this;
            this.$htmlSwitch.click(function () {
                _this.isHtml = _this.$htmlSwitch.is(":checked");
                if (_this.isHtml)
                    _this.$editorDiv.text(_this.$editorDiv.html());
                else
                    _this.$editorDiv.html(_this.$editorDiv.text());
            });
            $("form").submit(function () {
                var html = _this.isHtml ? _this.$editorDiv.text() : _this.$editorDiv.html();
                $editorHidden.val(html);
            });
        };
        Editor.prototype.setupIncrementalSaving = function ($savingStatus, saveEditorContentUrl) {
            var _this = this;
            var self = this;
            setInterval(function () {
                $savingStatus.text("Saving...");
                $.post(saveEditorContentUrl, {
                    content: _this.isHtml ? _this.$editorDiv.text() : _this.$editorDiv.html()
                }, function () {
                    $savingStatus.text("Saved");
                    setTimeout(function () {
                        $savingStatus.text("");
                    }, self.saveLabelHide);
                });
            }, this.intervalForSaves);
        };
        Editor.prototype.setupImageUpload = function ($fileControl, $uploadLink, $uploadingStatus) {
            var _this = this;
            if (!FormData) {
                $fileControl.prop("disable", true);
                $uploadLink.prop("disable", true);
                return;
            }
            $uploadLink.click(function () {
                var fileControl = $fileControl[0];
                var file = fileControl.files[0];
                if (!file)
                    return false;
                var extensionStart = file.name.lastIndexOf(".");
                var extension = file.name.substring(extensionStart + 1).toLowerCase();
                if (["jpg", "jpeg", "gif", "png"].indexOf(extension) < 0) {
                    $uploadingStatus.html('<span class="field-validation-error">Only jpg, jpeg, gif and png files are supported.</span>');
                    return false;
                }
                if (file.size > 500000) {
                    $uploadingStatus.html('<span class="field-validation-error">File size should be less than 500kb.</span>');
                    return false;
                }
                var form = new FormData();
                form.append("file", file);
                $uploadingStatus.text("Uploading...");
                var href = $uploadLink.attr("href");
                var self = _this;
                $.ajax(href, {
                    type: "POST",
                    data: form,
                    processData: false, cache: false, contentType: false,
                    success: function (path) {
                        if (path === "file already exists") {
                            $uploadingStatus.html('<span class="field-validation-error">A file with this file name already exists. Rename or pick another.</span>');
                            return;
                        }
                        var imageHtml = '<p align="center"><img style="max-width: 600px" src="' + path + '" /></p>';
                        if (self.isHtml)
                            self.$editorDiv.text(self.$editorDiv.text() + imageHtml);
                        else
                            self.$editorDiv.append(imageHtml);
                        $uploadingStatus.text("");
                    },
                    error: function () {
                        $uploadingStatus.html('<span class="field-validation-error">Upload failed :(</span>');
                    }
                });
                return false;
            });
        };
        return Editor;
    }());
    GrislyGrotto.Editor = Editor;
})(GrislyGrotto || (GrislyGrotto = {}));
