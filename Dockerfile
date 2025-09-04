# Estágio 1: Build da Aplicação
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
EXPOSE 5000

# Copia tudo de uma vez
COPY . ./

# Restaura e publica
RUN dotnet restore ./AssetTrackingAPI/AssetTrackingAPI.csproj
RUN dotnet publish ./AssetTrackingAPI/AssetTrackingAPI.csproj -c Release -o /app/publish

# Estágio 2: Imagem Final
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "AssetTrackingAPI.dll"]
