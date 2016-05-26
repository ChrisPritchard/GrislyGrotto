/// <reference path="../Scripts/jquery-2.0.1.js" />
/// <reference path="../Scripts/knockout-2.2.1.debug.js" />

$().ready(function () {

    $('.js_runSearch').on('click', function () {
        var searchTerm = $('.js_searchTerm').val();
        if (!searchTerm)
            return;

        window.location.href = '/home/search/?searchTerm=' + escape(searchTerm);
    });

    $.ajax({
        url: '/shared/quote',
        success: function(data) {
            ko.applyBindings(data, $('.js_quote')[0]);
        }
    });

    var storiesViewModel = {
        showInitiator: ko.observable(true),
        showLoading: ko.observable(false),
        showHeading: ko.observable(false),
        stories: ko.observableArray(),
        loadStories: function() {
            storiesViewModel.showInitiator(false);
            storiesViewModel.showLoading(true);
            $.ajax({
                url: '/shared/stories?r=' + Math.random(),
                success: function (data) {
                    storiesViewModel.showLoading(false);
                    storiesViewModel.stories(data);
                    storiesViewModel.showHeading(true);
                }
            });
        },
        hideStories: function () {
            storiesViewModel.showInitiator(true);
            storiesViewModel.stories([]);
            storiesViewModel.showHeading(false);
        }
    };
    ko.applyBindings(storiesViewModel, $('.js_stories')[0]);
    $('.js_stories').show();

    var tagCountsViewModel = {
        showInitiator: ko.observable(true),
        showLoading: ko.observable(false),
        showHeading: ko.observable(false),
        tagCounts: ko.observableArray(),
        loadTagCounts: function () {
            tagCountsViewModel.showInitiator(false);
            tagCountsViewModel.showLoading(true);
            $.ajax({
                url: '/shared/tagcounts?r=' + Math.random(),
                success: function (data) {
                    tagCountsViewModel.showLoading(false);
                    tagCountsViewModel.tagCounts(data);
                    tagCountsViewModel.showHeading(true);
                }
            });
        },
        hideTagCounts: function () {
            tagCountsViewModel.showInitiator(true);
            tagCountsViewModel.tagCounts([]);
            tagCountsViewModel.showHeading(false);
        }
    };
    ko.applyBindings(tagCountsViewModel, $('.js_tagCounts')[0]);
    $('.js_tagCounts').show();
    
    var monthViewModel = {
        showInitiator: ko.observable(true),
        showLoading: ko.observable(false),
        showHeading: ko.observable(false),
        monthCounts: ko.observableArray(),
        loadMonthCounts: function () {
            monthViewModel.showInitiator(false);
            monthViewModel.showLoading(true);
            $.ajax({
                url: '/shared/monthcounts?r=' + Math.random(),
                success: function (data) {
                    monthViewModel.showLoading(false);
                    monthViewModel.monthCounts(data);
                    monthViewModel.showHeading(true);
                }
            });
        },
        hideMonthCounts: function () {
            monthViewModel.showInitiator(true);
            monthViewModel.monthCounts([]);
            monthViewModel.showHeading(false);
        }
    };
    ko.applyBindings(monthViewModel, $('.js_monthCounts')[0]);
    $('.js_monthCounts').show();
});