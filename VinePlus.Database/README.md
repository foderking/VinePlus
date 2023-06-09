
A library for accessing the postgres database for vine

`
- install postgresql
- create a user with the name `foderking`
- create a database with the name `comic_vine`
- download the dump file

## Method 1
- download my [database dump](https://mega.nz/file/KX4kCCzL#ue4ZPxWDqRYBjCQSeww_M_aOsTonAkKKwo2yWHIlcDQ)
- restore the dump to the database with `pg_restore -v -h <postgres-server-address> -U <username> -d <databasename> -j 2 comicvine_seed_v1`
- after a while, all the tables, and data should be restored

## Method 2
- install dotnet ef `dotnet tool install --global dotnet-ef`
- Go to [web project](../ComicVine.API)
- run migrations to build the database schema
- Build the [seed project](../VinePlus.Seed/README.md)
- Run it
- import the generated csv file into the database with pgadmin or any other method of your choice
