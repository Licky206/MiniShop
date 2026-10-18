# ---------- faza 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Prvo samo projektni fajlovi -> restore sloj se kesira
# dok se NuGet paketi ne promene.
COPY MiniShop.Api/*.csproj MiniShop.Api/
COPY MiniShop.Application/*.csproj MiniShop.Application/
COPY MiniShop.Domain/*.csproj MiniShop.Domain/
COPY MiniShop.Data/*.csproj MiniShop.Data/

# Restore samo Api projekta i njegovih referenci.
RUN dotnet restore MiniShop.Api/MiniShop.Api.csproj

COPY . .

RUN dotnet publish MiniShop.Api/MiniShop.Api.csproj -c Release -o /app \
    --no-restore -p:UseAppHost=false


# ---------- faza 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app .

# LocalDiskStorage upisuje slike u ovaj direktorijum.
# Folder mora pripadati korisniku aplikacije.
RUN mkdir -p /app/wwwroot/images && \
    chown -R $APP_UID /app/wwwroot/images

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "MiniShop.Api.dll"]