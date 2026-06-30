FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/GreenNode.Domain/GreenNode.Domain.csproj", "src/GreenNode.Domain/"]
COPY ["src/GreenNode.Application/GreenNode.Application.csproj", "src/GreenNode.Application/"]
COPY ["src/GreenNode.Infrastructure/GreenNode.Infrastructure.csproj", "src/GreenNode.Infrastructure/"]
COPY ["src/GreenNode.WebAPI/GreenNode.WebAPI.csproj", "src/GreenNode.WebAPI/"]
RUN dotnet restore "src/GreenNode.WebAPI/GreenNode.WebAPI.csproj"

COPY . .
RUN dotnet publish "src/GreenNode.WebAPI/GreenNode.WebAPI.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GreenNode.WebAPI.dll"]
