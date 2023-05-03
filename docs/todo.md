# TODO

## To Start

- new post
- edit post
- delete post

- upload file ui

- full styling
- csp, security headers

- docker file
- fly.io deployment
- dns update
- docs

## doing

- s3 image storage uploads
    - either via form or pure js

> change how account works? rest of site will use js to prevent submits that are invalid, and just bad request at the server
> difference here is that we need to return to this page on completion, and as there are three forms, indicate success
> possibly pass back query and print message?
> could also simplify updates... send each form to a different endpoint, which then redirects back

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

## Stretch / Possible ideas

- next post / prev post: can't see where this would be needed, even though the current site has it
- themes, like the current site has. was always a bit messy, not sure it added much even if fun
- rss feed
- click enlarge images?
- share on other socs?
- social media links (linkedin, tryhackme, github etc) in more prominent locations
- categories?
- chatgpt integration lolol. could be used to get categories from a post
- last 10 post headings?