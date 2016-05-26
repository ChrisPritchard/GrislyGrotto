/// <reference path="../Scripts/jquery-2.0.1.js" />
/// <reference path="../Scripts/knockout-2.2.1.js" />

this.grislyGrotto_addedTags = this.grislyGrotto_addedTags || []; // used on submit
$().ready(function () {

    var existingTags = $('.js_existingTags').val().split(',');
    var initialSelectedTags = $('.js_selectedTags').val().split(',');

    var tagsViewModel = {
        searchTerm: ko.observable(),
        addNewTag: function () {
            var searchTerm = tagsViewModel.searchTerm();
            addTagToViewModel(searchTerm);
        },
        addedTags: ko.observableArray()
    };

    function addTagModel(addedTags, tagName) {
        addedTags.push({
            name: tagName,
            displayName: tagName.replace(/_/g, ' '),
            removeThisTag: function () {
                var newAddedTags = tagsViewModel.addedTags();
                for (var i = 0; i < newAddedTags.length; i++)
                    if (newAddedTags[i].name === this.name) {
                        newAddedTags = newAddedTags.slice(0, i).concat(newAddedTags.slice(i + 1));
                        break;
                    }
                tagsViewModel.addedTags(newAddedTags);
                grislyGrotto_addedTags = newAddedTags;
            }
        });
    }

    function addMatchedTagModel(matchedTags, tagName) {
        matchedTags.push({
            name: tagName,
            addThisTag: function () {
                addTagToViewModel(this.name)
            }
        });
    }

    function addTagToViewModel(tagName) {
        if (!tagName)
            return;
        var safeTagName = tagName.toLowerCase();

        var addedTags = tagsViewModel.addedTags();
        for (var i = 0; i < addedTags.length; i++)
            if (addedTags[i].name.toLowerCase() === safeTagName) {
                tagsViewModel.searchTerm('');
                return;
            }

        addTagModel(addedTags, tagName);
        tagsViewModel.searchTerm('');
        tagsViewModel.addedTags(addedTags);
        grislyGrotto_addedTags = addedTags;
    } 

    tagsViewModel.foundTags = ko.computed(function () {
        var matchedTags = [];

        var searchTerm = tagsViewModel.searchTerm();
        if (!searchTerm)
            return;
        searchTerm = searchTerm.toLowerCase();

        for (var i = 0; i < existingTags.length; i++) {
            var tag = existingTags[i].toLowerCase();
            if (tag.indexOf(searchTerm) > -1) {
                var tagName = existingTags[i].replace(/_/g, ' ');
                addMatchedTagModel(matchedTags, tagName);
            }
        }

        return matchedTags;
    });

    for (var i = 0; i < initialSelectedTags.length; i++)
        addTagToViewModel(initialSelectedTags[i]);

    ko.applyBindings(tagsViewModel, $('.js_tags')[0]);
});