FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY OrderCore.sln ./
COPY src/OrderCore.Api/OrderCore.Api.csproj src/OrderCore.Api/
COPY src/OrderCore.Application/OrderCore.Application.csproj src/OrderCore.Application/
COPY src/OrderCore.Contracts/OrderCore.Contracts.csproj src/OrderCore.Contracts/
COPY src/OrderCore.Domain/OrderCore.Domain.csproj src/OrderCore.Domain/
COPY src/OrderCore.Infrastructure/OrderCore.Infrastructure.csproj src/OrderCore.Infrastructure/

RUN dotnet restore src/OrderCore.Api/OrderCore.Api.csproj

COPY src/ src/
RUN dotnet publish src/OrderCore.Api/OrderCore.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "OrderCore.Api.dll"]
