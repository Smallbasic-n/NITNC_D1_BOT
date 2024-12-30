FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
RUN dotnet restore "ImagawaYoshimoto/ImagawaYoshimoto.csproj"

WORKDIR "/src"
RUN dotnet build "ImagawaYoshimoto/ImagawaYoshimoto.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ImagawaYoshimoto/ImagawaYoshimoto.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN dotnet dev-certs https -ep /app/publish/aspnetapp.pfx -p internalcred&&chmod 777 /app/publish/aspnetapp.pfx

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV "ASPNETCORE_Kestrel__Certificates__Default__Password" "internalcred"
ENV "ASPNETCORE_Kestrel__Certificates__Default__Path" "/app/aspnetapp.pfx"
ENTRYPOINT ["dotnet", "ImagawaYoshimoto.dll"]