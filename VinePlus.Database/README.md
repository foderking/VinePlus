# VinePlus.Database

A library for accessing the postgres database.

## How to set up the database

- Install postgresql
- Create a database with the name `comic_vine`
- Add the connection string with the name `comicvine_db` to the appsettings.json file or add as an environment variable in the form: `ConnectionStrings:comicvine_db=<connectionstring>`
- Hydrate the database

## How to hydrate the database

The database can be hydrated in two methods. You can either download all the data from comicvine's servers or restore the database from an backup.

### Method 1
- install dotnet ef `dotnet tool install --global dotnet-ef`
- Go to [web application](../VinePlus.Web)
- push migrations to the database with `dotnet-ef database update --connection="connection string"`. Now the schema is on the db
- Build the [seed project](../VinePlus.Seed/README.md)
- Follow the instructions there to download the data in CSV files
- import the generated csv file into the database with pgadmin or any other method of your choice

### Method 2
- download a [database dump](https://mega.nz/file/KX4kCCzL#ue4ZPxWDqRYBjCQSeww_M_aOsTonAkKKwo2yWHIlcDQ)
- restore the dump to the database with `pg_restore -v -h <postgres-server-address> -U <username> -d <databasename> -j 2 comicvine_seed_v1`
- after a while, all the tables, and data should be restored

The first can be time consuming - especially on low internet speed, while the backup might not have the most recent data
