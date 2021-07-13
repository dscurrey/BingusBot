FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["BingusBot2/BingusBot2.csproj", "BingusBot2/"]
RUN dotnet restore "BingusBot2/BingusBot2.csproj"
COPY . .
WORKDIR "/src/BingusBot2"
RUN dotnet build "BingusBot2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BingusBot2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BingusBot2.dll"]
