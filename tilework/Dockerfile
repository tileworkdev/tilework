FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5180

ENV ASPNETCORE_URLS=http://+:5180

RUN groupadd docker && usermod -aG docker app

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["tilework/tilework.csproj", "tilework/"]
RUN dotnet restore "tilework/tilework.csproj"
COPY . .
WORKDIR "/src/tilework"
RUN dotnet build "tilework.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "tilework.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["tilework/entrypoint.sh", "."]
USER root
ENTRYPOINT ["/bin/sh", "entrypoint.sh"]
