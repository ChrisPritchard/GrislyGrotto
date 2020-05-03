# Tasks

Next steps, ideas, things to add etc.

- [x] Add an 'are you sure' when leaving the editor page if it is 'dirty'
- [x] Fix printing of 'nil'
- ~~[ ] Rewrite wandering triangles to be more intelligent and pure JS?~~
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

- [ ] also allow the user to set direction, e.g. down (default) out, in, up
- [x] return url for login page?
- [x] cookies should be set for site, not site + path

- [ ] keep alive of some sort on the editor screen
    - maybe a simple countdown and final copy prompt