FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["HOB.Worker/HOB.Worker.csproj", "HOB.Worker/"]
COPY ["HOB.Data/HOB.Data.csproj", "HOB.Data/"]
COPY ["HOB.Common/HOB.Common.Library/HOB.Common.Library.csproj", "HOB.Common/HOB.Common.Library/"]
RUN dotnet restore "HOB.Worker/HOB.Worker.csproj"

COPY . .
WORKDIR "/src/HOB.Worker"
RUN dotnet build "HOB.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HOB.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create reports directory
RUN mkdir -p /reports && chmod 777 /reports

ENTRYPOINT ["dotnet", "HOB.Worker.dll"]
