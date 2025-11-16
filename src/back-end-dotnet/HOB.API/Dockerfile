#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HOB.API/HOB.API.csproj", "HOB.API/"]
COPY ["HOB.Data/HOB.Data.csproj", "HOB.Data/"]
COPY ["HOB.Common/HOB.Common.Library/HOB.Common.Library.csproj", "HOB.Common/HOB.Common.Library/"]
RUN dotnet restore "./HOB.API/HOB.API.csproj"
COPY . .
WORKDIR "/src/HOB.API"
RUN dotnet build "./HOB.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HOB.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HOB.API.dll"]