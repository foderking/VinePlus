
## Setup
- Set the connection string in the environment variable called `ConnectionStrings__comicvine_db`. eg (on windows`$env:ConnectionStrings__comicvine_db = "Server=..."`)
## Features
- Provides restful api for querying posts, threads, user profiles and more

## TODO
- [x] improve styling
- [x] responsive UI
- [ ] refactor internals
- [x] add polling worker
- [x] default error pages for web interface
- [x] error handling for deactivated profiles
- [ ] add footer linking to api on forums
- [ ] add ui icons for polls and stuff

### Posts
- [ ] fix youtube video in posts (http://localhost:5119/archives/thread/2308691#11)
- [x] add time stamp to posts
- [x] ui improvements on timestamp
- [x] change color of edited button
- [ ] add way to copy link to a specific post

### Threads 
- [ ] ability to sort threads by different criteria
- [ ] thread should have a link to the og post on cv
- [ ] shows full title of thread on hover
- [ ] forum should be able to link to the last post

### Profile
- [x] make images and blogs on user profile optional
- [x] refactors for thread and post view on user profile (and errors for deactivated accounts)

### Search
- [x] add search
- [x] search from user
- [ ] possibly filter thread search by board, type

### Stats
- [ ] add stats UI
- [ ] board stats

### Api
- [ ] better styling for api documentation
- [ ] more documentation for api
- [ ] add format for api errors

### Users archives
- [x] posts view in user profile should have link to the thread
- [x] there should be a way to link to a specific post in a thread

## Archives
- [x] sorting of threads by no of posts 
- [ ] sorting of posts by date created
- [x] highlight for deleted posts