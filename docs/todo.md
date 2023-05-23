# TODO

## To Start

- delete comments when deleting post
- maybe some command handling on the editor? e.g. ctrl+b for bold, a button to add a link etc: these could just insert the requisite markdown characters
    - could include easy emojis

## doing


## Done

- basic latest posts
- basic single post with comments
- adding comments
- error handling (server response and logging)
- comment validation javascript
- date formatting
- basic, functional style for dev
- menu
- latest next 5 / prev 5
- about
- all months with post counts
- all stories
- minimal embedded favicon
- all posts in month
- prev/next month
- search
- search results
- s3 image storage retrieval
- profile image links
- cookie functionality
- light mode / dark mode
- edit comment
- delete comment
- argon2 verification
- login
- login post
- logout
- better handler error handling
- argon2 hashing
- user details display
- update user details
- user details client side validations
- s3 image storage uploads
- new post
- edit post
- editor validation js
- editor dirty flag
- upload file for editor
- delete post
- csp, security headers
- docker file
- database path specifiable by param?
- fly.io deployment
- s3 issue
- docs
- dns update
- time zone fix
- secret key from env var
- full styling
- default click to open on image script code
- fix code rendering
- speed up docker image building

## Stretch / Possible ideas

- next post / prev post: can't see where this would be needed, even though the current site has it
- themes, like the current site has. was always a bit messy, not sure it added much even if fun
- rss feed
- share on other socs?
- social media links (linkedin, tryhackme, github etc) in more prominent locations
- categories?
- chatgpt integration lolol. could be used to get categories from a post
- last 10 post headings?
- cookie expiry check - new cookies last much longer, and its unclear how likely they are to expire during a new post
- comment edit author?
- automatic webp image conversion: https://github.com/image-rs/image/blob/master/README.md