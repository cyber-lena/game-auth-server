FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Packages.props ./
COPY GameAuthServer.slnx ./
COPY src/ ./src/
RUN dotnet restore src/GameAuth.Core/GameAuth.Core.csproj
RUN dotnet publish src/GameAuth.Core/GameAuth.Core.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GameAuth.Core.dll"]
