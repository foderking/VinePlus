# VinePlus.Seed

Scrapes all data from comicvine servers for use in the postgres database
As of the time of writing, there are about 840,000 threads and 22,000,000 posts on comicvine, all of which would take around 300MB, and 22GB of space respectively
After it is done, a bunch of csv files are created: `threads_full.csv` containing the data for all threads, and `posts_<x>.csv` for all the <x> posts

## Prerequisites
- Make sure you have [installed dotnet 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Make sure you have [installed redis](https://redis.io/docs/getting-started/installation/)
- Bash shell, jq

## Getting Started
- Make sure redis is installed and star the redis server with `redis-server`
- Build and run the project `dotnet run`
- This scrapes comicvine, and might take a while depending on your internet speed.
- After this is done, a `threads_full.json` file, and a bunch of files in the format `posts_*.json` ('*' is a number from 0+) should be created.
- Run `thread.sh threads_full` to convert the threads json file to csv
- Run `post.sh posts_*` ('*' is a number from 0+) to convert the posts json files to csv
- After the CSV files have been created, you can import them into postgres.

**Note**: You must have initialized the database with the steps described [here](../VinPlus.Database)
