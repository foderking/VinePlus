
## Setup
- Set the connection string in the environment variable called `ConnectionStrings__comicvine_db`. eg (on windows`$env:ConnectionStrings__comicvine_db = "Server=..."`)
## Features
- Provides restful api for querying posts, threads, user profiles and more

## TODO
-[ ] better styling
-[ ] responsive UI
-[ ] refactor internals
-[ ] add time stamp to posts
-[ ] add search
-[x] add polling worker
-[ ] add stats
-[ ] default error pages for both api and web interface
-[ ] better styling for api documentation
-[ ] more documentation for api
-[ ] refactors for thread and post view on user profile (and errors for deactivated accounts)
-[ ] ability to sort threads by different criteria
-[ ] added footer linking to api on forums
-[ ] make images and blogs on user profile optional

## Archives TODO
-[x] sorting of threads by no of posts 
-[ ] sorting of posts by date created
-[ ] thread should have a link to the og post on cv
-[ ] higlights for deleted posts
-[ ] posts view in user profile should have link to the thread
-[ ] there should be a way to link to a specific post in a thread
-[ ] clicking on post user's post view should link to the post in thread