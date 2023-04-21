# Structure

main.rs sets up the webserver, adds resources like the template handler etc

model.rs contains the basic structs used by the solution. these are all serializable so they can be passed to the template renderer

data.rs retrieves data from the database

handlers.rs represents each path and method request to the site, e.g. get and post endpoints

## All paths, unauthenticated

- latest posts
- next 5 / prev 5

- single post
- next post / prev post

- add comment to post
- edit comment
- delete comment

- all months with post counts
- all posts in month

- all stories

- search
- search results

- about

## Paths authenticated 

- login
- login post

- new post
- edit post
- delete post

- upload file

- user details