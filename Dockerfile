FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build
COPY Backends/WebApi ./
RUN dotnet restore HorusEye.Api/HorusEye.Api.csproj && \
    dotnet publish HorusEye.Api/HorusEye.Api.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "HorusEye.Api.dll"]
