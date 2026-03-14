# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Layer cache ottimizzato: copia tutti i .csproj prima del restore
COPY ["Shelly.McpServer/Shelly.McpServer.csproj", "Shelly.McpServer/"]
COPY ["Shelly.Models/Shelly.Models.csproj", "Shelly.Models/"]
COPY ["Shelly.Services/Shelly.Services.csproj", "Shelly.Services/"]
RUN dotnet restore "Shelly.McpServer/Shelly.McpServer.csproj"

# Copia il sorgente e pubblica
COPY . .
RUN dotnet publish "Shelly.McpServer/Shelly.McpServer.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Final stage — Alpine per immagine più piccola (~75MB vs ~190MB)
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS final
WORKDIR /app
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Shelly.McpServer.dll"]
