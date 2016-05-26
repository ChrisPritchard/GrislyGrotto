/// <reference path="../Scripts/knockout-2.2.1.debug.js" />

(function () {
    $().ready(function () {

        var viewModel = {
            newComments: ko.observableArray(),
            commentAuthor: ko.observable(),
            commentContent: ko.observable(),
            allEnabled: ko.observable(true),

            dateFormatted: function (comment) {
                if (!comment.Created) {
                    return '';
                }
                var date = new Date(parseInt(comment.Created.substr(6)));
                return date.toLocaleString();
            },

            submitComment: function () {
                viewModel.allEnabled(false);
                $.ajax({
                    method: 'POST',
                    url: '/comments/newComment',
                    data: {
                        postID: $('.js_postID').val(),
                        author: viewModel.commentAuthor(),
                        content: viewModel.commentContent()
                    },
                    dataType: 'json',
                    success: function (data) {
                        var newData = viewModel.newComments().concat([ data ]);
                        viewModel.newComments(newData);
                        viewModel.commentAuthor('');
                        viewModel.commentContent('');
                        viewModel.allEnabled(true);
                        $('.js_commentsCount').text(parseInt($('.js_commentsCount').text()) + 1);
                        $('.js_noComment').hide();
                    }
                });
            }
        };
        viewModel.contentValid = ko.computed(function () {
            return viewModel.allEnabled() && viewModel.commentAuthor() && viewModel.commentContent();
        }, viewModel);

        ko.applyBindings(viewModel, $('.js_comments')[0]);
    });
})();