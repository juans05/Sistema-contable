FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/SistemaContable.API/SistemaContable.API.csproj", "SistemaContable.API/"]
COPY ["src/SistemaContable.Application/SistemaContable.Application.csproj", "SistemaContable.Application/"]
COPY ["src/SistemaContable.Domain/SistemaContable.Domain.csproj", "SistemaContable.Domain/"]
COPY ["src/SistemaContable.Infrastructure/SistemaContable.Infrastructure.csproj", "SistemaContable.Infrastructure/"]
COPY ["src/SistemaContable.Common/SistemaContable.Common.csproj", "SistemaContable.Common/"]

RUN dotnet restore "SistemaContable.API/SistemaContable.API.csproj"
COPY src/ .
WORKDIR "/src/SistemaContable.API"
RUN dotnet build "SistemaContable.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SistemaContable.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SistemaContable.API.dll"]
