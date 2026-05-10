FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Resilient-AI-Gateway/Resilient-AI-Gateway.csproj", "Resilient-AI-Gateway/"]
RUN dotnet restore "Resilient-AI-Gateway/Resilient-AI-Gateway.csproj"
COPY . .
WORKDIR "/src/Resilient-AI-Gateway"
RUN dotnet build "./Resilient-AI-Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Resilient-AI-Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Resilient-AI-Gateway.dll"]
