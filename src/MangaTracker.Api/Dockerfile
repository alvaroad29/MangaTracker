FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/MangaTracker.Api/MangaTracker.Api.csproj", "src/MangaTracker.Api/"]
COPY ["src/MangaTracker.Application/MangaTracker.Application.csproj", "src/MangaTracker.Application/"]
COPY ["src/MangaTracker.Domain/MangaTracker.Domain.csproj", "src/MangaTracker.Domain/"]
COPY ["src/MangaTracker.Infrastructure/MangaTracker.Infrastructure.csproj", "src/MangaTracker.Infrastructure/"]
RUN dotnet restore "src/MangaTracker.Api/MangaTracker.Api.csproj"

COPY . .
RUN dotnet publish "src/MangaTracker.Api/MangaTracker.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render asigna el puerto dinámicamente vía la variable $PORT — hay que escucharla, no un puerto fijo
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:$PORT dotnet MangaTracker.Api.dll"]