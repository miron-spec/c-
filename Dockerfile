FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY MatchApp.Backend/MatchApp.Backend.csproj MatchApp.Backend/

RUN dotnet restore MatchApp.Backend/MatchApp.Backend.csproj

COPY . .

RUN dotnet publish MatchApp.Backend/MatchApp.Backend.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 10000

ENTRYPOINT ["sh", "-c", "dotnet MatchApp.Backend.dll --urls http://0.0.0.0:${PORT:-10000}"]
