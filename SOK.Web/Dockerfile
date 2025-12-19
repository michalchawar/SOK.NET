# Ten etap jest używany podczas uruchamiania z programu VS w trybie szybkim (domyślnie dla konfiguracji debugowania)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Ustawienia
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Etap budowania CSS z Tailwind
FROM node:20-alpine AS node-build
WORKDIR /app

# Kopiuj pliki package.json i package-lock.json
COPY ["SOK.Web/package*.json", "./"]
COPY ["SOK.Web/copy-libs.js", "./"]

# Instaluj zależności Node.js (łącznie z devDependencies dla buildu)
RUN npm ci

# Kopiuj pliki potrzebne do budowania CSS
COPY ["SOK.Web/tailwind.config.js", "./"]
COPY ["SOK.Web/tailwind.extension.json", "./"]
COPY ["SOK.Web/wwwroot/css/site.css", "./wwwroot/css/"]
COPY ["SOK.Web/Views/", "./Views/"]
COPY ["SOK.Web/wwwroot/", "./wwwroot/"]

# Buduj CSS
RUN npx @tailwindcss/cli -i wwwroot/css/site.css -o wwwroot/css/site.min.css -m


# Ten etap służy do kompilowania projektu usługi
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SOK.Web/SOK.Web.csproj", "SOK.Web/SOK.Web.csproj"]
RUN dotnet restore "./SOK.Web/SOK.Web.csproj"
COPY . .

# Skopiuj zbudowany CSS i biblioteki vendor z etapu node-build
COPY --from=node-build /app/wwwroot/css/site.min.css /src/SOK.Web/wwwroot/css/
COPY --from=node-build /app/wwwroot/lib/ /src/SOK.Web/wwwroot/lib/

WORKDIR /src/SOK.Web
RUN dotnet build "./SOK.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Ten etap służy do publikowania projektu usługi do skopiowania do etapu końcowego
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SOK.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Ten etap jest używany w środowisku produkcyjnym lub w przypadku uruchamiania z programu VS w trybie regularnym (domyślnie, gdy nie jest używana konfiguracja debugowania)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SOK.Web.dll"]