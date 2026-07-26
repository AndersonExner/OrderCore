FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY OrderCore.sln ./
COPY src/OrderCore.Contracts/OrderCore.Contracts.csproj src/OrderCore.Contracts/
COPY src/OrderCore.Domain/OrderCore.Domain.csproj src/OrderCore.Domain/
COPY src/OrderCore.Application/OrderCore.Application.csproj src/OrderCore.Application/
COPY src/OrderCore.Infrastructure/OrderCore.Infrastructure.csproj src/OrderCore.Infrastructure/
COPY src/OrderCore.Worker/OrderCore.Worker.csproj src/OrderCore.Worker/

RUN dotnet restore src/OrderCore.Worker/OrderCore.Worker.csproj

COPY src src

RUN dotnet publish src/OrderCore.Worker/OrderCore.Worker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderCore.Worker.dll"]
