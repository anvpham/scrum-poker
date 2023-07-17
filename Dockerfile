FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env
WORKDIR /src
COPY backend/scrum-poker-server/*.csproj .
RUN dotnet restore
COPY backend/scrum-poker-server .
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
WORKDIR /publish
COPY --from=build-env /publish .
ENV ASPNETCORE_URLS http://0.0.0.0:5001
EXPOSE 5001

ENTRYPOINT ["dotnet", "scrum-poker-server.dll"]
