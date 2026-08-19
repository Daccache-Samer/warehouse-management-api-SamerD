FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Notifications.Presentation/Notifications.Presentation.csproj", "Notifications.Presentation/"]
COPY ["Notifications.Application/Notifications.Application.csproj", "Notifications.Application/"]
COPY ["Notificatons.Infrastructure/Notificatons.Infrastructure.csproj", "Notificatons.Infrastructure/"]
COPY ["Notifications.Domain/Notifications.Domain.csproj", "Notifications.Domain/"]
RUN dotnet restore "Notifications.Presentation/Notifications.Presentation.csproj"
COPY . .
WORKDIR "/src/Notifications.Presentation"
RUN dotnet build "./Notifications.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Notifications.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notifications.Presentation.dll"]