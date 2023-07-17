FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env
WORKDIR /src
COPY backend/scrum-poker-server/*.csproj .
RUN dotnet restore
COPY backend/scrum-poker-server .
RUN dotnet publish -c Release -o /publish
RUN dotnet dev-certs https

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
WORKDIR /publish
COPY --from=build-env /publish .
COPY --from=build-env /root/.dotnet/corefx/cryptography/x509stores/my/* /root/.dotnet/corefx/cryptography/x509stores/my/
EXPOSE 5001

ENTRYPOINT ["dotnet", "scrum-poker-server.dll"]
