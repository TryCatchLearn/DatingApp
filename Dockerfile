# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file (if you have one)
COPY *.sln ./

# Copy project files first (enables Docker layer caching)
COPY API/API.csproj API/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source
COPY . .

# Publish API project
RUN dotnet publish API/API.csproj -c Release -o /app/publish

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# ASP.NET Core listens on 8080 by convention in containers
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]