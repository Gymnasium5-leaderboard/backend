FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Leaderboard.Api/Leaderboard.Api.csproj", "Leaderboard.Api/"]
COPY ["Leaderboard.Application/Leaderboard.Application.csproj", "Leaderboard.Application/"]
COPY ["Leaderboard.Domain/Leaderboard.Domain.csproj", "Leaderboard.Domain/"]
COPY ["Leaderboard.BackgroundJobs/Leaderboard.BackgroundJobs.csproj", "Leaderboard.BackgroundJobs/"]
COPY ["Leaderboard.Cache/Leaderboard.Cache.csproj", "Leaderboard.Cache/"]
COPY ["Leaderboard.DAL/Leaderboard.DAL.csproj", "Leaderboard.DAL/"]
RUN dotnet restore "Leaderboard.Api/Leaderboard.Api.csproj"
COPY . .
WORKDIR "/src/Leaderboard.Api"
RUN dotnet build "./Leaderboard.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Leaderboard.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Leaderboard.Api.dll"]
