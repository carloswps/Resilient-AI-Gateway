# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first (for layer caching)
COPY ["Resilient-AI-Gateway/Resilient-AI-Gateway.csproj", "Resilient-AI-Gateway/"]
COPY ["Resilient-AI-Gateway.Shared/Resilient-AI-Gateway.Shared.csproj", "Resilient-AI-Gateway.Shared/"]

# Restore dependencies
RUN dotnet restore "Resilient-AI-Gateway/Resilient-AI-Gateway.csproj"

# Copy everything else
COPY . .

# Publish
RUN dotnet publish "Resilient-AI-Gateway/Resilient-AI-Gateway.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# HF Spaces runs on port 7860
ENV ASPNETCORE_URLS=http://0.0.0.0:7860
EXPOSE 7860

# Copy published app
COPY --from=build /app/publish .

# Non-root user for security (HF Spaces requirement)
USER $APP_UID

ENTRYPOINT ["dotnet", "Resilient-AI-Gateway.dll"]