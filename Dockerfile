FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY NuGet.Config ./
COPY AraseTraderOrderService.sln ./
COPY Api/Api.csproj Api/
COPY Application/Application.csproj Application/
COPY Contracts/Contracts.csproj Contracts/
COPY Domain/Domain.csproj Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/

RUN dotnet restore AraseTraderOrderService.sln

FROM restore AS build
ARG BUILD_CONFIGURATION=Release
COPY . .
RUN dotnet build Api/Api.csproj \
    --configuration $BUILD_CONFIGURATION \
    --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish Api/Api.csproj \
    --configuration $BUILD_CONFIGURATION \
    --no-build \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=publish /app/publish .

USER app
ENTRYPOINT ["dotnet", "Api.dll"]
