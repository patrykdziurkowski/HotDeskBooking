# HotDeskBooking
## Setup
1. Set required variables:
    - Set the following environment variables in a `.env/` file at the project's root:
        - `SA_PASSWORD` - an environment variable required for running docker compose. This is the password of the MSSQL database admin.
        - `ASPNETCORE_ENVIRONMENT` - an environment variable used for running docker compose. This denotes the current environment. Consider setting this to "Development" by default.
        - `PRIVATE_KEY` - an environment variable used for running docker compose. This is used to generate and validate Jwt tokens used for Api authentication.
    - Set the following test User Secret variables for `App.csproj`:
        - `ConnectionString` - this connection string is used to connect to integration test database. When app is ran via docker compose this is overwritten by the connection string found inside docker compose.
        - `PrivateKey` - this is used to generate and validate Jwt tokens used for Api authentication.
    - Set the following test User Secret variables for `Tests.csproj`:
        - `SA_PASSWORD` - this is the MSSQL database admin password for the database container used for integration testing. Must match the password found in the connection string above.
2. Run `docker compose up` from within the project root.