FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY HorusEye.slnx ./
COPY HorusEye.Api/HorusEye.Api.csproj HorusEye.Api/
COPY HorusEye.Core/HorusEye.Core.csproj HorusEye.Core/
COPY HorusEye.Infrastructure/HorusEye.Infrastructure.csproj HorusEye.Infrastructure/
RUN dotnet restore

COPY . .
RUN dotnet publish HorusEye.Api/HorusEye.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app .
ENTRYPOINT ["dotnet", "HorusEye.Api.dll"]
