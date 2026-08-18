# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/AiRag.Api/AiRag.Api.csproj", "AiRag.Api/"]
COPY ["src/AiRag.sln", "./"]
RUN dotnet restore "AiRag.Api/AiRag.Api.csproj"
COPY src/ .
WORKDIR "/src/AiRag.Api"
RUN dotnet build "AiRag.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AiRag.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl ca-certificates && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .
EXPOSE 8000
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 CMD curl -f http://localhost:8000/health || exit 1
ENTRYPOINT ["dotnet", "AiRag.Api.dll"]