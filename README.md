# HotDeskBooking
## Setup
1. Set required environment and user secret variables:
    - `SA_PASSWORD` environment variable required for running docker compose. This is the password of the MSSQL database admin.
    - `ASPNETCORE_ENVIRONMENT` environment variable used for running docker compose. This denotes the current environment. Consider setting this to "Development" by default.
    - `SA_PASSWORD` in "Tests" assembly user secrets. This is the MSSQL database admin password for the database container used for integration testing. Must match the below ConnectionString's password.
    - `ConnectionString` in "App" assembly user secrets. This ConnectionString is used to connect to integration test database. When app is ran via docker compose this is overwritten by the compose connection string.
2. Run `docker compose up` from within the project root.