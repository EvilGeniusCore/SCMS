FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY SCMS.Data/SCMS.Data.csproj SCMS.Data/
COPY SCMS/SCMS.csproj SCMS/
RUN dotnet restore SCMS/SCMS.csproj
COPY . .
RUN dotnet publish SCMS/SCMS.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
VOLUME /app/data
VOLUME /app/wwwroot/uploads
ENTRYPOINT ["dotnet", "SCMS.dll"]
