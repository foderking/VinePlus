# VinePlus.Seed

scrapes comicvine to for all threads and posts
it is run as an executable independent from other projects
As of the time of writing, there are about 840,000 threads and 22,000,000 posts on comicvine, all of which would take around 300MB, and 22GB of space respectively

it took around 2.5 hours, and xxx days to download all threads and posts respectively

after it is done, a bunch of csv files are created: `threads_full.csv` containing the data for all threads, and `posts_<x>.csv` for all the <x> posts


## Prerequisites
- Make sure you have [installed dotnet 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- Make sure you have [installed redis](https://redis.io/docs/getting-started/installation/)

## How to Run
- Clone the Main repository
- Navigate the path for this project `cd /Comicvine.Polling`
- Run this command in terminal `dotnet run`

