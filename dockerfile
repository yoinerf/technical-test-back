FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["src/InvestmentFunds.API/InvestmentFunds.API.csproj", "InvestmentFunds.API/"]
COPY ["src/InvestmentFunds.Domain/InvestmentFunds.Domain.csproj", "InvestmentFunds.Domain/"]
COPY ["src/InvestmentFunds.Infrastructure/InvestmentFunds.Infrastructure.csproj", "InvestmentFunds.Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "InvestmentFunds.API/InvestmentFunds.API.csproj"

# Copiar todo el código fuente
COPY src/ .

# Build
WORKDIR "/src/InvestmentFunds.API"
RUN dotnet build "InvestmentFunds.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InvestmentFunds.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InvestmentFunds.API.dll"]
