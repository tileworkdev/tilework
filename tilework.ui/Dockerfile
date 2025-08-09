FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5180

ENV ASPNETCORE_URLS=http://+:5180

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["tilework.ui/tilework.ui.csproj", "tilework.ui/"]
RUN dotnet restore "tilework.ui/tilework.ui.csproj"
COPY . .
WORKDIR "/src/tilework.ui"
RUN dotnet build "tilework.ui.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "tilework.ui.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["tilework.ui/entrypoint.sh", "."]
USER root
ENTRYPOINT ["/bin/sh", "entrypoint.sh"]
