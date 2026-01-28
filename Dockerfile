FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FreedomDanceStudio/FreedomDanceStudio/FreedomDanceStudio.csproj", "FreedomDanceStudio/FreedomDanceStudio/"]
RUN dotnet restore "FreedomDanceStudio/FreedomDanceStudio/FreedomDanceStudio.csproj"
COPY . .
WORKDIR "/src/FreedomDanceStudio/FreedomDanceStudio"
RUN dotnet build "./FreedomDanceStudio.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FreedomDanceStudio.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV ASPNETCORE_ENVIRONMENT=Staging
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=10m --timeout=10s --start-period=10s \
CMD curl -f http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "FreedomDanceStudio.dll"]
