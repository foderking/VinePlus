# VinePlus.Seed

Scrapes all data from comicvine servers for use in the postgres database

As of the time of writing, there are about 800+ thousand threads and 20+ million posts on comicvine, which would take around 300MB, and 22GB of space respectively when downloaded

After it is completely downloaded, a bunch of csv files are created: `threads_full.csv` containing the data for all threads, and `posts_<x>.csv` for all the <x> posts

## Prerequisites
- [dotnet 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- [redis](https://redis.io/docs/getting-started/installation/)
- bash shell
- jq

## Getting Started
- Make sure redis is installed and start the redis server with `redis-server`
- Build and run this project `dotnet run`
- This scrapes comicvine, and might take a while depending on your internet speed.
- After this is done, a `threads_full.json` file, and a bunch of files in the format `posts_*.json` ('*' is a number from 0+) should be created.
- Run `thread.sh threads_full` to convert the threads json file to csv
- Run `post.sh posts_*` ('*' is a number from 0+) to convert the posts json files to csv
- After the CSV files have been created, you can import them into postgres.

**Note**: You must have initialized the database with the steps described [here](../VinPlus.Database)
