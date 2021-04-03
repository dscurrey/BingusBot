FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app


COPY BingusBot/*.csproj ./
RUN dotnet restore


COPY . ./
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "BingusBot.dll"]