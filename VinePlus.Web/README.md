# VinePlus.Web

Web frontend and RESTful api interface for VinePlus. 
Supports fetching content from either a local postgres database or directly from comicvine servers.

## Features
- Provides restful api for querying posts, threads, user profiles and more
- Advanced search features (requires postgres database)
- Sorting of threads (requires postgres databases)

## How to set up the database

- Install postgresql
- Create a database with the name `comic_vine`
- Add the connection string with the name `comicvine_db` to the appsettings.json file or add as an environment variable in the form: `ConnectionStrings:comicvine_db=<connectionstring>`
- Hydrate the database

## How to hydrate the database

The database can be hydrated in two methods. You can either download all the data from comicvine's servers or restore the database from an backup.

### Method 1
- Change directory to this folder
- install dotnet ef `dotnet tool install --global dotnet-ef`
- push migrations to the database with `dotnet-ef database update --connection="connection string"`. Now the schema is on the db
- Build the [seed project](../VinePlus.Seed/README.md)
- Follow the instructions there to download the data in CSV files
- import the generated csv file into the database with pgadmin or any other method of your choice

### Method 2
- download a [database dump](https://mega.nz/file/KX4kCCzL#ue4ZPxWDqRYBjCQSeww_M_aOsTonAkKKwo2yWHIlcDQ)
- restore the dump to the database with `pg_restore -v -h <postgres-server-address> -U <username> -d <databasename> -j 2 comicvine_seed_v1`
- after a while, all the tables, and data should be restored

The first can be time consuming - especially on low internet speed, while the backup might not have the most recent data


## TODO
- [x] improve styling
- [x] responsive UI
- [ ] refactor internals
- [x] add polling worker
- [x] default error pages for web interface
- [x] error handling for deactivated profiles
- [x] add footer linking to api on forums
- [ ] ~~add ui icons for polls and stuff~~

### Posts
- [x] fix youtube video in posts (http://localhost:5119/archives/thread/2308691#11)
- [x] add time stamp to posts
- [x] ui improvements on timestamp
- [x] change color of edited button
- [x] add way to copy link to a specific post

### Threads 
- [ ] ~~ability to sort threads by different criteria~~
- [x] thread should have a link to the og post on cv
- [x] shows full title of thread on hover
- [x] forum should be able to link to the last post

### Profile
- [x] make images and blogs on user profile optional
- [x] refactors for thread and post view on user profile (and errors for deactivated accounts)
- [ ] more stuff on deactivated users
- [ ] fallbacks when profile not found
- [ ] different color for nav header
- [ ] add stats user

### Search
- [x] add search
- [x] search from user
- ~~[ ] possibly filter thread search by board, type~~
- [ ] "How to search" UI
- [ ] pagination for search

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
- [ ] an option of viewing all threads a user has posted in, and number of posts

## Archives
- [x] sorting of threads by no of posts 
- [x] sorting by various parameters
- [x] UI option to change sort type
- [ ] ~~sorting of posts by date created~~
- [x] highlight for deleted posts

## Tables
- [ ] add indexes on `posts.created`,`creator->>'Text'`.
- [ ] remove unnecessary columns in queries

## Features
- Viewing posts, threads, and blog made by a specific user
- Viewing deleted posts
- Sorting forums by number of posts, views, and date created
