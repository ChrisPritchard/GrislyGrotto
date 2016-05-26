/// <reference path="../typings/knockout/knockout.d.ts" />
/// <reference path="../typings/jquery/jquery.d.ts" />

module GrislyGrotto {

    interface IResult {
        title: string;
        author: string;
        date: string;
    }

    export class Search {

        freeText = ko.observable('');
        storiesOnly = ko.observable(false);
        author = ko.observable('');
        authorOptions = ['Christopher', 'Peter'];
        orderBy = ko.observable('Descending');
        orderByOptions = ['Descending', 'Ascending'];

        results: KnockoutObservableArray<IResult> = ko.observableArray([]);
        noResults = ko.observable(false);

        private lastFilter = ko.observable('');
        private page = ko.observable(1);
        private moreAvailable = ko.observable(false);

        private resultsUrl: string;

        constructor(resultsUrl: string) {
            this.resultsUrl = resultsUrl;
        }

        newSearch() {
            this.lastFilter("?freeText=" + (this.freeText() ? this.freeText() : '*')
                + "&storiesOnly=" + this.storiesOnly()
                + "&author=" + (this.author() ? this.author() : '')
                + "&orderBy=" + (this.orderBy() === 'Descending' ? 'date desc': 'date asc'));

            this.moreAvailable(true);
            this.page(1);
            this.results([]);
            this.runSearch();
        }

        getMore() {
            this.page(this.page() + 1);
            this.runSearch();
        }

        private runSearch() {
            var _this = this;
            var url = this.resultsUrl + this.lastFilter() + "&page=" + this.page();
            $.getJSON(url, null, function (results) {
                if(!results.length || results.length < 10)
                    _this.moreAvailable(false);
                else
                    for(var i in results)
                        _this.results.push(results[i]);
                _this.noResults(_this.results().length == 0);
            });
        }
    }
}