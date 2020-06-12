# Tasks

Next steps, ideas, things to add etc.

- [x] Add an 'are you sure' when leaving the editor page if it is 'dirty'
- [x] Fix printing of 'nil'
- [x] add a checkbox in the top right that will disable the visualisation
- [x] move this vis control panel when the screen gets small
- [x] extend the control panel - allow the user to set colours and 'apply' by setting a cookie

    - [x] setup the ui controls
    - [x] change animation colours
    - [x] set save / set cookie functionality
    - [x] allow this save to return to current page (return url)

    idea: change background to source its colours from html elements, then have theme change by reloading and applying a custom css file

    - [x] surface a theme css file via a handler
    - [x] set anim colours from theme handler
    - [x] switch cookie nonsense to use theme names, which control the dynamic theme applied
    
    - [x] apply changes to main theme, not just background

- [ ] Rewrite wandering triangles to be more intelligent and pure JS?
- [ ] also allow the user to set direction, e.g. down (default) out, in, up
- [ ] perhaps some alternate vis's, like flames or stars

- [x] return url for login page?
- [x] cookies should be set for site, not site + path

- [x] keep alive of some sort on the editor screen
    - [x] - maybe a simple countdown and final copy prompt
- [x] in memory brute force protection:
    - [x] delays based on account, and ip
    - [-] for comments, ensure that a user is on the page for a certain time before posting, maybe?
        - would affect regular users ("oh i just remembered to say...")
    - [x] and that for a given ip, they can only make one comment per five seconds?
    - [x] just the ip, and accounting for x-forwarded-for
    - [x] some way of capping this so it doesn't grow infinitely
- [x] the ability to make draft posts? visible only to the user?
    - probably via prefixing [DRAFT] to the title
- [x] when a draft is published, should its date be updated?
- [x] non-drafts cannot be made into drafts

- [x] new black/white colour scheme
- [x] fixed background canvas, with max size
- [x] change animation so canvas attributes are css (resize not checked)
- [ ] background preserve state between loads? localstorage mebbe