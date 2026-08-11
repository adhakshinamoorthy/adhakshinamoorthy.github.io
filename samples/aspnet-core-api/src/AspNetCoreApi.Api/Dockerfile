FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/AspNetCoreApi.Api/AspNetCoreApi.Api.csproj", "src/AspNetCoreApi.Api/"]
RUN dotnet restore "src/AspNetCoreApi.Api/AspNetCoreApi.Api.csproj"
COPY . .
RUN dotnet publish "src/AspNetCoreApi.Api/AspNetCoreApi.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "AspNetCoreApi.Api.dll"]
