# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props .
COPY src/DockerOrdersApi/DockerOrdersApi.csproj src/DockerOrdersApi/
RUN dotnet restore src/DockerOrdersApi/DockerOrdersApi.csproj
COPY src/DockerOrdersApi/ src/DockerOrdersApi/
RUN dotnet publish src/DockerOrdersApi/DockerOrdersApi.csproj -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "DockerOrdersApi.dll"]
