# VinePlus.Web

Web frontend and RESTful api interface for VinePlus. 
Supports fetching content from either a local postgres database or directly from comicvine servers.

## Features
- Viewing posts, threads, and blog made by a specific user
- Viewing deleted posts
- Sorting threads by number of posts, views, and date created
- Viewing posts, and threads by deactivated user
- full text search of threads with filtering by user
- searching posts made by a specific user

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

## Method 3
- navigate to `VinePlus.Database`
- run `migrations.sql`. this would initialize the schema
- then you can populate the data with either method 1 or 2

The first can be time consuming - especially on low internet speed, while the backup might not have the most recent data

### How to run
- make sure you have net9 (you can check installed sdks with `dotnet --list-sdks`)
- You can either run the project with `dotnet run` or download the latest release and run with `dotnet VinePlus.Web.dll`
- *make sure the database is initialized and the connection string is stored in `appsettings.json` or as the `ConnectionStrings:comicvine_db` environment variable as described earlier*

## TODO
- [x] improve styling
- [x] responsive UI
- [x] refactor internals
- [x] add polling worker
- [x] default error pages for web interface
- [x] error handling for deactivated profiles
- [x] add footer linking to api on forums
- [ ] ~~add ui icons for polls and stuff~~
- [x] better error pages
- [ ] validation for forms

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
- [ ] ?? more stuff on deactivated users ??
- [x] fallbacks when profile not found
- [ ] ~~different color for nav header~~
- [ ] ?? add stats user ??

### Search
- [x] add search
- [x] search from user
- [ ] ~~possibly filter thread search by board, type~~
- [ ] "How to search" UI
- [x] pagination for search
- [x] add ability to search posts
- [x] highlight search term

### Stats
- [ ] add stats UI
- [ ] board stats
- [x] viewing all threads a user has posted in, and number of posts
- [ ] stats for number of posts, and threads a user has made

### Api
- [ ] better styling for api documentation
- [ ] more documentation for api
- [ ] add format for api errors

### Users archives
- [x] posts view in user profile should have link to the thread
- [x] there should be a way to link to a specific post in a thread

## Archives
- [x] sorting of threads by no of posts 
- [x] sorting by various parameters
- [x] UI option to change sort type
- [ ] ~~sorting of posts by date created~~
- [x] highlight for deleted posts

## Tables
- [x] add indexes on `posts.created`,`creator->>'Text'`.
- [x] remove unnecessary columns in queries
- [ ] search data structure for threads, and posts
