# Zobacz https://aka.ms/customizecontainer, aby dowiedzieć się, jak dostosować kontener debugowania i jak program Visual Studio używa tego pliku Dockerfile do kompilowania obrazów w celu szybszego debugowania.

# Ten etap jest używany podczas uruchamiania z programu VS w trybie szybkim (domyślnie dla konfiguracji debugowania)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Install node
#RUN apt-get update && \
#    apt-get upgrade -y && \
#    apt-get install -y curl && \
#    curl -fsSL https://deb.nodesource.com/setup_22.x -o nodesource_setup.sh && \
#    bash nodesource_setup.sh && \
#    rm -r nodesource_setup.sh && \
#    apt-get install -y nodejs

# Ustawienia
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# Ten etap służy do kompilowania projektu usługi
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SOK.Web/SOK.Web.csproj", "SOK.Web/SOK.Web.csproj"]
RUN dotnet restore "./SOK.Web/SOK.Web.csproj"
COPY . .
WORKDIR "/src/SOK.Web"
RUN dotnet build "./SOK.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Ten etap służy do publikowania projektu usługi do skopiowania do etapu końcowego
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SOK.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Ten etap jest używany w środowisku produkcyjnym lub w przypadku uruchamiania z programu VS w trybie regularnym (domyślnie, gdy nie jest używana konfiguracja debugowania)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "app.dll"]