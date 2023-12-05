#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /source

# copy csproj and restore as distinct layers
COPY Console/*.csproj Console/
COPY Service/*.csproj Service/
RUN dotnet restore Console/Console.csproj

# copy and build app and libraries
COPY Console/ Console/
COPY Service/ Service/
RUN dotnet restore

RUN dotnet build --no-restore -c $BUILD_CONFIGURATION


FROM build AS publish
WORKDIR /source/Console
RUN dotnet publish --no-restore -c $BUILD_CONFIGURATION -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Console.dll"]