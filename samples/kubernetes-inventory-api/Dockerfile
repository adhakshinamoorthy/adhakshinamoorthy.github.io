FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props .
COPY src/KubernetesInventoryApi/KubernetesInventoryApi.csproj .
RUN dotnet restore KubernetesInventoryApi.csproj
COPY src/KubernetesInventoryApi/ .
RUN dotnet publish KubernetesInventoryApi.csproj -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "KubernetesInventoryApi.dll"]
