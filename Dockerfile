FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Chronoscope.sln .
COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY src/Chronoscope.Domain/Chronoscope.Domain.csproj src/Chronoscope.Domain/
COPY src/Chronoscope.Application/Chronoscope.Application.csproj src/Chronoscope.Application/
COPY src/Chronoscope.Data/Chronoscope.Data.csproj src/Chronoscope.Data/
COPY src/Chronoscope.Infrastructure/Chronoscope.Infrastructure.csproj src/Chronoscope.Infrastructure/
COPY src/Chronoscope.Web/Chronoscope.Web.csproj src/Chronoscope.Web/
COPY src/Chronoscope.Host/Chronoscope.Host.csproj src/Chronoscope.Host/

RUN dotnet restore src/Chronoscope.Host/Chronoscope.Host.csproj

COPY src/ src/

RUN dotnet publish src/Chronoscope.Host/Chronoscope.Host.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Chronoscope.Host.dll"]
