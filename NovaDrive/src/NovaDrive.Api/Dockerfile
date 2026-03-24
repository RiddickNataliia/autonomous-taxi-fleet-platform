FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY ["NovaDrive/src/NovaDrive.Api/NovaDrive.Api.csproj", "NovaDrive/src/NovaDrive.Api/"]
COPY ["NovaDrive/src/NovaDrive.Application/NovaDrive.Application.csproj", "NovaDrive/src/NovaDrive.Application/"]
COPY ["NovaDrive/src/NovaDrive.Infrastructure/NovaDrive.Infrastructure.csproj", "NovaDrive/src/NovaDrive.Infrastructure/"]
COPY ["NovaDrive/src/NovaDrive.Domain/NovaDrive.Domain.csproj", "NovaDrive/src/NovaDrive.Domain/"]
RUN dotnet restore "NovaDrive/src/NovaDrive.Api/NovaDrive.Api.csproj"
COPY . .
WORKDIR "/src/NovaDrive/src/NovaDrive.Api"
RUN dotnet build "NovaDrive.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NovaDrive.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NovaDrive.Api.dll"]