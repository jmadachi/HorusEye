ARG BUILD_DATE=unknown

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build
COPY Backends/WebApi ./
RUN dotnet restore HorusEye.Api/HorusEye.Api.csproj && \
    dotnet publish HorusEye.Api/HorusEye.Api.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
RUN apt-get update && apt-get install -y libkrb5-3 && rm -rf /var/lib/apt/lists/*
COPY --from=build /publish .
ENV BUILD_DATE=${BUILD_DATE}
ENTRYPOINT ["dotnet", "HorusEye.Api.dll"]
