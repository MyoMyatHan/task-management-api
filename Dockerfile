# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (layer caching for restore)
COPY ["Task Management.sln", "./"]
COPY ["API/API.csproj", "API/"]
COPY ["BAL/BAL.csproj", "BAL/"]
COPY ["MODEL/MODEL.csproj", "MODEL/"]
COPY ["REPOSITORY/REPOSITORY.csproj", "REPOSITORY/"]

RUN dotnet restore "Task Management.sln"

# Copy all source and build
COPY . .
RUN dotnet publish "API/API.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create uploads directory
RUN mkdir -p /app/Uploads

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "API.dll"]
