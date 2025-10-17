# Blazor Server app Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/AiRag.Blazor/AiRag.Blazor.csproj", "AiRag.Blazor/"]
COPY ["src/AiRag.sln", "./"]
RUN dotnet restore "AiRag.Blazor/AiRag.Blazor.csproj"
COPY src/ .
WORKDIR "/src/AiRag.Blazor"
RUN dotnet publish "AiRag.Blazor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl ca-certificates && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish ./
EXPOSE 5250
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 CMD curl -f http://localhost:5250/ || exit 1
ENTRYPOINT ["dotnet", "AiRag.Blazor.dll"]
