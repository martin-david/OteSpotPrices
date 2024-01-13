#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

# copy csproj and restore as distinct layers
COPY src/Console/*.csproj Console/
COPY src/Service/*.csproj Service/
RUN dotnet restore Console/Console.csproj

# copy and build app and libraries
COPY src/Console/ Console/
COPY src/Service/ Service/
RUN dotnet build Console/Console.csproj -c $BUILD_CONFIGURATION

FROM build AS publish
RUN dotnet publish Console/Console.csproj -c $BUILD_CONFIGURATION -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Console.dll"]
CMD ["--startDate=2023-01-01", "--endDate=2023-12-31"]