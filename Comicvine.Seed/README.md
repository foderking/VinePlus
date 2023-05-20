# Comicvine Seeder
Helps you download seed data from Comicvine. As of the time of writing, there are about 840,000 threads and 22,000,000 posts.

When the data is fully downloaded, the size of the `threads.json` is around 350Mb and `posts.json` is around 20Gb.

It would take a while to completely download all the data (it took like 2 hours to download all the threads, and <...> days to download all the posts)

You can also run the scripts to convert the json file to csv, to make it easier to insert into database

## Prerequisites
- Make sure you have [installed dotnet 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- Make sure you have [installed redis](https://redis.io/docs/getting-started/installation/)

## How to Run
- Clone the Main repository
- Navigate the path for this project `Comicvine.Polling`
- Run this command in terminal `dotnet run`

