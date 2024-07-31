FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

ENV ACCEPT_EULA Y
ARG password
RUN test -n "${password}" || (echo "password not set" && false)
ENV MSSQL_SA_PASSWORD ${password}

EXPOSE 1433

CMD /opt/mssql/bin/sqlservr

HEALTHCHECK --interval=5s --timeout=1s --start-period=10s --retries=10 \
  CMD /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P ${MSSQL_SA_PASSWORD} -Q "SELECT 1" -b -No