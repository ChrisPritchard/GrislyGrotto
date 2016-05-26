/// <reference path="../Scripts/knockout-2.2.1.debug.js" />

(function () {
    $().ready(function () {

        var initialCount = parseInt($('#Count').val());

        var viewModel = {
            isLoading: ko.observable(false),
            start: initialCount,
            count: initialCount,
            list: ko.observableArray(),

            dateFormatted: function (post) {
                if (!post.Created) {
                    return '';
                }
                var date = new Date(parseInt(post.Created.substr(6)));
                return date.toLocaleString();
            },

            loadMore: function () {
                viewModel.isLoading(true);
                window.location.hash = "#moreloaded";
                $.ajax({
                    url: '/home/fortaginrange?r=' + Math.random(),
                    data: {
                        tagName: $('#TagName').val(),
                        start: viewModel.start,
                        count: viewModel.count
                    },
                    dataType: 'json',
                    success: function (data) {
                        var newData = viewModel.list().concat(data);
                        viewModel.list(newData);
                        viewModel.isLoading(false);
                        viewModel.start += data.length;
                    }, error: function (e) { alert(e); }
                });
            }
        };
        viewModel.showLoadMore = ko.computed(function() {
            return !viewModel.isLoading();
        });

        ko.applyBindings(viewModel, $('.js_loadMore')[0]);
    });
})();