FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/ZuluIA_Back.Api/ZuluIA_Back.Api.csproj",                     "src/ZuluIA_Back.Api/"]
COPY ["src/ZuluIA_Back.Application/ZuluIA_Back.Application.csproj",     "src/ZuluIA_Back.Application/"]
COPY ["src/ZuluIA_Back.Domain/ZuluIA_Back.Domain.csproj",               "src/ZuluIA_Back.Domain/"]
COPY ["src/ZuluIA_Back.Infrastructure/ZuluIA_Back.Infrastructure.csproj","src/ZuluIA_Back.Infrastructure/"]

RUN dotnet restore "src/ZuluIA_Back.Api/ZuluIA_Back.Api.csproj"

COPY . .
WORKDIR "/src/src/ZuluIA_Back.Api"
RUN dotnet build "ZuluIA_Back.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "ZuluIA_Back.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZuluIA_Back.Api.dll"]
