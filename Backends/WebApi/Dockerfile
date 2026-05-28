FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY Backends/WebApi/HorusEye.slnx ./
COPY Backends/WebApi/HorusEye.Api/HorusEye.Api.csproj HorusEye.Api/
COPY Backends/WebApi/HorusEye.Core/HorusEye.Core.csproj HorusEye.Core/
COPY Backends/WebApi/HorusEye.Infrastructure/HorusEye.Infrastructure.csproj HorusEye.Infrastructure/
RUN dotnet restore

COPY Backends/WebApi/ .
RUN dotnet publish HorusEye.Api/HorusEye.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /out .
ENTRYPOINT ["dotnet", "HorusEye.Api.dll"]
