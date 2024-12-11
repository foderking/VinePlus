# VinePlus

This project helps run the [comic-vine forum](https://comicvine.gamespot.com/forums/) locally.

`VinePlus.Web` contains the main web application. A postgres database `VinePlus.Database` is used for storage. A background worker in `VinePlus.Polling` is used to keeps the database up to date.

## Getting Started

- Clone the project, or download a binary [here](https://github.com/foderking/VinePlus/releases)
- follow the instruction [here](https://github.com/foderking/VinePlus/blob/master/VinePlus.Web/README.md) to set up the postgres database
- if you are running for the first time, follow the instruction [here](https://github.com/foderking/VinePlus/tree/master/VinePlus.Seed) to download all the content from comicvine. After this is done, a bunch of csv files are created which you import into postgres
- run the application at `Comicvine.API` to start the server
