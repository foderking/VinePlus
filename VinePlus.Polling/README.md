# VinePlus.Polling

Worker service for updating VinePlus postgres database.
It is already added as a background service in the main project: `VinePlus.Web`. You can also run it directly

# How to run directly
- `dotnet run`
- make sure the database is initialized according to the instructions in `VinePlus.Web/README.md`
- you can set the connection string for the database in the appsettings.json or as an environment variable as described
