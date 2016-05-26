/// <reference path="jquery.d.ts" />

module GrislyGrotto {

    export class Editor {

        isHtml = false;
        intervalForSaves = 10000;
        saveLabelHide = 1000;

        $editorDiv: JQuery;
        $htmlSwitch: JQuery;

        constructor($editorDiv: JQuery, $htmlSwitch: JQuery) {
            this.$editorDiv = $editorDiv;
            this.$htmlSwitch = $htmlSwitch;
        }

        setupContentEditing($editorHidden: JQuery) {
            this.$htmlSwitch.click(() => {
                this.isHtml = this.$htmlSwitch.is(":checked");

                if (this.isHtml)
                    this.$editorDiv.text(this.$editorDiv.html());
                else
                    this.$editorDiv.html(this.$editorDiv.text());
            });

            $("form").submit(() => {
                var html = this.isHtml ? this.$editorDiv.text() : this.$editorDiv.html();
                $editorHidden.val(html);
            });
        }

        setupIncrementalSaving($savingStatus: JQuery, saveEditorContentUrl: string) {
            const self = this;
            setInterval(() => {
                $savingStatus.text("Saving...");
                $.post(saveEditorContentUrl, {
                    content: this.isHtml ? this.$editorDiv.text() : this.$editorDiv.html()
                }, () => {
                    $savingStatus.text("Saved");
                    setTimeout(() => {
                        $savingStatus.text("");
                    }, self.saveLabelHide);
                });
            }, this.intervalForSaves);
        }

        setupImageUpload($fileControl: JQuery, $uploadLink: JQuery, $uploadingStatus: JQuery) {
            if (!FormData) {
                $fileControl.prop("disable", true);
                $uploadLink.prop("disable", true);
                return;
            }
                
            $uploadLink.click(() => {
                const fileControl = $fileControl[0];
                const file = (<HTMLInputElement>fileControl).files[0];

                if (!file)
                    return false;

                const extensionStart = file.name.lastIndexOf(".");
                const extension = file.name.substring(extensionStart + 1).toLowerCase();
                if (["jpg", "jpeg", "gif", "png"].indexOf(extension) < 0) {
                    $uploadingStatus.html('<span class="field-validation-error">Only jpg, jpeg, gif and png files are supported.</span>');
                    return false;
                }

                if (file.size > 500000) {
                    $uploadingStatus.html('<span class="field-validation-error">File size should be less than 500kb.</span>');
                    return false;
                }

                const form = new FormData();
                form.append("file", file);

                $uploadingStatus.text("Uploading...");

                const href = $uploadLink.attr("href");
                const self = this;
                $.ajax(href, {
                    type: "POST",
                    data: form,
                    processData: false, cache: false, contentType: false,
                    success(path) {
                        if (path === "file already exists") {
                            $uploadingStatus.html('<span class="field-validation-error">A file with this file name already exists. Rename or pick another.</span>');
                            return;
                        }

                        const imageHtml = '<p align="center"><img style="max-width: 600px" src="' + path + '" /></p>';
                        if (self.isHtml)
                            self.$editorDiv.text(self.$editorDiv.text() + imageHtml);
                        else
                            self.$editorDiv.append(imageHtml);

                        $uploadingStatus.text("");
                    },
                    error() {
                        $uploadingStatus.html('<span class="field-validation-error">Upload failed :(</span>');
                    }
                });
                return false;
            });
        }
    }
} 