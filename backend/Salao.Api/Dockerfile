# syntax=docker/dockerfile:1.7
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN --mount=type=secret,id=local_ca,required=false \
    if [ -s /run/secrets/local_ca ]; then cp /run/secrets/local_ca /usr/local/share/ca-certificates/local-build-ca.crt && update-ca-certificates; fi && dotnet publish Salao.Api.csproj -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Salao.Api.dll"]
