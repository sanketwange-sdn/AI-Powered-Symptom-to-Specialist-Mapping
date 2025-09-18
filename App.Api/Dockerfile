# Use official ASP.NET runtime as base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore App.Api/App.Api.csproj
RUN dotnet publish App.Api/App.Api.csproj -c Release -o /App.Api/publish

# Final stage
FROM base AS final
WORKDIR /App.Api
COPY --from=build /App.Api/publish .
ENTRYPOINT ["dotnet", "App.Api.dll"]