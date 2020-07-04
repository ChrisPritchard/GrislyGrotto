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

- [x] Rewrite wandering triangles to be more intelligent and pure JS?
- [-] also allow the user to set direction, e.g. down (default) out, in, up
    - not going to do this
- [-] perhaps some alternate vis's, like flames or stars
    - tracked as their own taks

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
- [-] background preserve state between loads? localstorage mebbe (DOESN'T WORK DUE TO BACKGROUND CANVAS)
    - [x] will require that triangle state is reduced to data rather than objects

- [x] embedded static content - single file + db website!
- [-] empty db for dev purposes?
    - nice in theory, but weight of data is useful for testing things like archives view

- [-] sample empty db for github
    - replaced with sql script in database instructions
- [x] instructions for db
- [x] user password hasher
    - now in same database instructions

- [x] remove old env-vars, switch to cmd line only
- [x] when in 'develop', direct link to views and statics
- [x] for wandering triangles, when off screen should jump

- [ ] experimental new visualisation, gradient fire effect
- [ ] second experiment: rippling triangle lattice

- [x] add a forced delay between comments? a few seconds from page load might ensure that spamming doesnt work while (if less than five or even ten seconds) not effecting regular users

- [x] rather than one big background, most of which is covered by the site container, split into two animations
    - [x] would require animations be runnable against more than one canvas, i.e. more functional without state
    - [x] would also require the amount of triangles be scalable by width

- [x] code reorganisation - breakup handlers

- [x] easy way to get images, script even into markdown doc.
    - [-] non standard html interpreter?
        - [x] goldmark supports enabling html unsafe mode, which allows direct html
    - [-] someway to use inline scripts - perhaps scripts can be embedded as a 'per blog' resource accessble via the static generator
        - would this just circumvent the csp? perhaps just disable csp for scripts, and improve blacklisting for comments?
        - for now csp is fine, good practice. the need for this bypass is so rare that weakening site security is probably not worth it.

- [x] switch to using the goldmark with unsafe html evaluation for all posts
    - [x] remove toggle control between html and markdown
    - [x] update existing posts to remove the 'markdown|' token
        - possibly via an update using sqlite3 command line tool
    