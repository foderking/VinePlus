# VinePlus.Web

VinePlus is a different way to browse the ComicVine forum. It saves forum content in a PostgreSQL database, making it easier to add features like better search and filtering. The app also gets live updates directly from ComicVine servers, so users always have the latest content

## 1. Features
- Viewing posts, threads, and blog made by a specific user
- Viewing deleted posts
- Sorting threads by number of posts, views, and date created
- Viewing posts, and threads by deactivated user
- full text search of threads with filtering by user
- searching posts made by a specific user
- lots of stats for users, threads, and posts etc.

## 2. Prerequisites
- Make sure you have [.net 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) installed
- PostgreSQL
- dotnet-ef tool `dotnet tool install --global dotnet-ef`

## 3. Setting up the database
### 3.1. Setting up postgres
- Create a database with the name `comic_vine`
- Add the connection string named `comicvine_db` either to the appsettings.json file or as an environment variable in the format: `ConnectionStrings:comicvine_db=<connectionstring>`

### 3.2. Initializing schema
You can update the database to the latest schema through either of these ways:
- Navigate to the [database project](../VinePlus.Database) and run the `migrations.sql` script, or...
- Run `dotnet-ef database update` (make sure you've set up the connection strings)

### 3.3. Seeding the database

The database can be hydrated in two methods. You can either directly download all the data from comicvine's servers or restore the database from a backup.

#### 3.3.1 - Seeding database from backup
- Download the [latest backup](https://mega.nz/file/KX4kCCzL#ue4ZPxWDqRYBjCQSeww_M_aOsTonAkKKwo2yWHIlcDQ)
- Restore the dump to the database with `pg_restore -v -h <postgres-server-address> -U <username> -d <databasename> -j 2 comicvine_seed_v1`
- after a while, all the tables, and data should be restored
- **NOTE:** The backup might not have the most recent data

#### 3.3.2 - Seeding database directly
- Build the [seed project](../VinePlus.Seed/README.md)
- Follow the instructions there to download the data in CSV files
- import the generated csv file into the database with pgadmin or any other method of your choice
- **NOTE:** This can be time-consuming on low internet speed

### 3.4 Running VinePlus
- Make sure the database is set up.
- Run the command `dotnet run`. This will build the project, restore any necessary files, and then execute it.
- You can also download the latest release from the GitHub Releases section and run the prebuilt application using `dotnet VinePlus.Web.dll`

### TODO
- [ ] validation for forms
- [ ] "How to search" UI
- [ ] search data structure for threads, and posts