# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar archivos de proyecto
COPY PlataformaCreditosWeb/*.csproj ./PlataformaCreditosWeb/
RUN dotnet restore ./PlataformaCreditosWeb/PlataformaCreditosWeb.csproj

# Copiar todo el código
COPY PlataformaCreditosWeb/. ./PlataformaCreditosWeb/

# Publicar aplicación
WORKDIR /app/PlataformaCreditosWeb
RUN dotnet publish -c Release -o /out

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copiar build final
COPY --from=build /out .

# Exponer puerto (Render usa PORT dinámico)
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Ejecutar aplicación
ENTRYPOINT ["dotnet", "PlataformaCreditosWeb.dll"]