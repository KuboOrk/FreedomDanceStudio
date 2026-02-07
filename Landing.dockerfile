FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FreedomDanceStudio/FREEDOM/FREEDOM.csproj", "FreedomDanceStudio/FREEDOM/"]
RUN dotnet restore "FreedomDanceStudio/FREEDOM/FREEDOM.csproj"
COPY . .
WORKDIR "/src/FreedomDanceStudio/FREEDOM"
RUN dotnet build "./FREEDOM.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FREEDOM.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=10m --timeout=10s --start-period=10s \
CMD curl -f http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "FREEDOM.dll"]
