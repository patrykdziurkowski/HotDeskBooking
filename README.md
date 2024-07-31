# HotDeskBooking
## Setup
1. Set `SA_PASSWORD` and `ASPNETCORE_ENVIRONMENT` environment variables (such as from within a `.env` file).
    - `SA_PASSWORD` is the password of the administrator account for the database.
    - `ASPNETCORE_ENVIRONMENT` denotes the current environment. Consider setting this to "Development" by default.
2. Run `docker compose up` from within the project root.