FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY simulator/ .
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "Simulator.dll"]