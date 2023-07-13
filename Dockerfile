FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env

WORKDIR /src

COPY backend/scrum-poker-server .
RUN dotnet restore
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime

WORKDIR /publish
COPY --from=build-env /publish .
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "scrum-poker-server.dll"]