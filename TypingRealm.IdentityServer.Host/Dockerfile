FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TypingRealm.IdentityServer.Host/TypingRealm.IdentityServer.Host.csproj", "TypingRealm.IdentityServer.Host/"]
RUN dotnet restore "TypingRealm.IdentityServer.Host/TypingRealm.IdentityServer.Host.csproj"
COPY . .
WORKDIR "/src/TypingRealm.IdentityServer.Host"
RUN dotnet build "TypingRealm.IdentityServer.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TypingRealm.IdentityServer.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TypingRealm.IdentityServer.Host.dll"]