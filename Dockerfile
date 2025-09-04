# Estágio 1: Build da Aplicação
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copia o .csproj e restaura
COPY AssetTrackingAPI/*.csproj ./AssetTrackingAPI/
RUN dotnet restore ./AssetTrackingAPI/AssetTrackingAPI.csproj

# Copia todo o resto
COPY . .

# Publica em modo Release
RUN dotnet publish ./AssetTrackingAPI/AssetTrackingAPI.csproj -c Release -o /app/publish

# Estágio 2: Imagem Final
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "AssetTrackingAPI.dll"]
