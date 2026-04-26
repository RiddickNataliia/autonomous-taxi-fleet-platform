FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
RUN apk add --no-cache curl

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Add glibc compatibility for protoc
RUN apk add --no-cache libc6-compat

COPY ["src/NovaDrive.Api/NovaDrive.Api.csproj", "src/NovaDrive.Api/"]
COPY ["src/NovaDrive.Application/NovaDrive.Application.csproj", "src/NovaDrive.Application/"]
COPY ["src/NovaDrive.Infrastructure/NovaDrive.Infrastructure.csproj", "src/NovaDrive.Infrastructure/"]
COPY ["src/NovaDrive.Domain/NovaDrive.Domain.csproj", "src/NovaDrive.Domain/"]
RUN dotnet restore "src/NovaDrive.Api/NovaDrive.Api.csproj"
COPY . .
WORKDIR "/src/src/NovaDrive.Api"
RUN dotnet build "NovaDrive.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NovaDrive.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NovaDrive.Api.dll"]