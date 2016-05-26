/// <reference path="../typings/knockout/knockout.d.ts" />
/// <reference path="../typings/jquery/jquery.d.ts" />
var GrislyGrotto;
(function (GrislyGrotto) {
    var Search = (function () {
        function Search(resultsUrl) {
            this.freeText = ko.observable('');
            this.storiesOnly = ko.observable(false);
            this.author = ko.observable('');
            this.authorOptions = ['Christopher', 'Peter'];
            this.orderBy = ko.observable('Descending');
            this.orderByOptions = ['Descending', 'Ascending'];
            this.results = ko.observableArray([]);
            this.noResults = ko.observable(false);
            this.lastFilter = ko.observable('');
            this.page = ko.observable(1);
            this.moreAvailable = ko.observable(false);
            this.resultsUrl = resultsUrl;
        }
        Search.prototype.newSearch = function () {
            this.lastFilter("?freeText=" + (this.freeText() ? this.freeText() : '*')
                + "&storiesOnly=" + this.storiesOnly()
                + "&author=" + (this.author() ? this.author() : '')
                + "&orderBy=" + (this.orderBy() === 'Descending' ? 'date desc' : 'date asc'));
            this.moreAvailable(true);
            this.page(1);
            this.results([]);
            this.runSearch();
        };
        Search.prototype.getMore = function () {
            this.page(this.page() + 1);
            this.runSearch();
        };
        Search.prototype.runSearch = function () {
            var _this = this;
            var url = this.resultsUrl + this.lastFilter() + "&page=" + this.page();
            $.getJSON(url, null, function (results) {
                if (!results.length || results.length < 10)
                    _this.moreAvailable(false);
                else
                    for (var i in results)
                        _this.results.push(results[i]);
                _this.noResults(_this.results().length == 0);
            });
        };
        return Search;
    }());
    GrislyGrotto.Search = Search;
})(GrislyGrotto || (GrislyGrotto = {}));
//# sourceMappingURL=search.js.map