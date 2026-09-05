# Imagen base en al cual basaremos nuestra imagen
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# Creación del directorio de trabajo
WORKDIR /app
# Creción de ruta para logs
RUN mkdir -p /var/log/app
# Copiar csproj y restauramos nuestra app
COPY ./*.csproj ./
RUN dotnet restore
# Copiamos todos los archivos y compilados o construidos de nuestra app
COPY . .
RUN dotnet publish -c Release -o publish
# Construir o instanciamos nuestro contenedor
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
# Establecer variable de entorno si lo deseas (puedes sobreescribirla luego)
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5013
# Exponemos el puerto 5278
EXPOSE 5013
# Indicar el archivo dll compilado (Nombre del proyecto)
ENTRYPOINT ["dotnet","ApiKalumAuth.dll"]