
## Setup
- Set the connection string in the environment variable called `ConnectionStrings__comicvine_db`. eg (on windows`$env:ConnectionStrings__comicvine_db = "Server=..."`)
## Features
- Provides restful api for querying posts, threads, user profiles and more

## TODO
-[ ] improve styling
-[ ] responsive UI
-[ ] refactor internals
-[ ] add time stamp to posts
-[x] add polling worker
-[ ] default error pages for web interface
-[ ] added footer linking to api on forums

### Threads 
-[ ] ability to sort threads by different criteria

### Profile
-[ ] make images and blogs on user profile optional
-[ ] refactors for thread and post view on user profile (and errors for deactivated accounts)

### Search
-[x] add search
-[x] search from user

### Stats
-[ ] add stats UI

### Api
-[ ] better styling for api documentation
-[ ] more documentation for api
-[ ] add format for api errors

## Archives TODO
-[x] sorting of threads by no of posts 
-[ ] sorting of posts by date created
-[ ] thread should have a link to the og post on cv
-[ ] higlights for deleted posts
-[ ] posts view in user profile should have link to the thread
-[ ] there should be a way to link to a specific post in a thread
-[ ] clicking on post user's post view should link to the post in thread