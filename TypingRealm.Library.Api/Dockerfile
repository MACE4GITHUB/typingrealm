FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TypingRealm.Authentication.Core/TypingRealm.Authentication.Core.csproj", "TypingRealm.Authentication.Core/"]
COPY ["TypingRealm.Authentication/TypingRealm.Authentication.csproj", "TypingRealm.Authentication/"]
COPY ["TypingRealm.Common/TypingRealm.Common.csproj", "TypingRealm.Common/"]
COPY ["TypingRealm.Communication.Redis/TypingRealm.Communication.Redis.csproj", "TypingRealm.Communication.Redis/"]
COPY ["TypingRealm.Communication/TypingRealm.Communication.csproj", "TypingRealm.Communication/"]
COPY ["TypingRealm.DataAccess.Postgres/TypingRealm.DataAccess.Postgres.csproj", "TypingRealm.DataAccess.Postgres/"]
COPY ["TypingRealm.Hosting/TypingRealm.Hosting.csproj", "TypingRealm.Hosting/"]
COPY ["TypingRealm.Library.Api/TypingRealm.Library.Api.csproj", "TypingRealm.Library.Api/"]
COPY ["TypingRealm.Library.Core/TypingRealm.Library.Core.csproj", "TypingRealm.Library.Core/"]
COPY ["TypingRealm.Library.Infrastructure/TypingRealm.Library.Infrastructure.csproj", "TypingRealm.Library.Infrastructure/"]
COPY ["TypingRealm.Library/TypingRealm.Library.csproj", "TypingRealm.Library/"]
COPY ["TypingRealm.Profiles.Core/TypingRealm.Profiles.Core.csproj", "TypingRealm.Profiles.Core/"]
COPY ["TypingRealm.Texts.Core/TypingRealm.Texts.Core.csproj", "TypingRealm.Texts.Core/"]
RUN dotnet restore "TypingRealm.Library.Api/TypingRealm.Library.Api.csproj"
COPY . .
WORKDIR "/src/TypingRealm.Library.Api"
RUN dotnet build "TypingRealm.Library.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TypingRealm.Library.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TypingRealm.Library.Api.dll"]