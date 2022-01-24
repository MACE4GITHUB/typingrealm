FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TypingRealm.Authentication.Core/TypingRealm.Authentication.Core.csproj", "TypingRealm.Authentication.Core/"]
COPY ["TypingRealm.Authentication/TypingRealm.Authentication.csproj", "TypingRealm.Authentication/"]
COPY ["TypingRealm.Common/TypingRealm.Common.csproj", "TypingRealm.Common/"]
COPY ["TypingRealm.Communication.Redis/TypingRealm.Communication.Redis.csproj", "TypingRealm.Communication.Redis/"]
COPY ["TypingRealm.Communication/TypingRealm.Communication.csproj", "TypingRealm.Communication/"]
COPY ["TypingRealm.Configuration/TypingRealm.Configuration.csproj", "TypingRealm.Configuration/"]
COPY ["TypingRealm.Data.Resources/TypingRealm.Data.Resources.csproj", "TypingRealm.Data.Resources/"]
COPY ["TypingRealm.DataAccess.Postgres/TypingRealm.DataAccess.Postgres.csproj", "TypingRealm.DataAccess.Postgres/"]
COPY ["TypingRealm.Hosting/TypingRealm.Hosting.csproj", "TypingRealm.Hosting/"]
COPY ["TypingRealm.Logging/TypingRealm.Logging.csproj", "TypingRealm.Logging/"]
COPY ["TypingRealm.Profiles.Core/TypingRealm.Profiles.Core.csproj", "TypingRealm.Profiles.Core/"]
COPY ["TypingRealm.TextProcessing/TypingRealm.TextProcessing.csproj", "TypingRealm.TextProcessing/"]
COPY ["TypingRealm.Texts.Api.Client/TypingRealm.Texts.Api.Client.csproj", "TypingRealm.Texts.Api.Client/"]
COPY ["TypingRealm.Texts.Core/TypingRealm.Texts.Core.csproj", "TypingRealm.Texts.Core/"]
COPY ["TypingRealm.Typing.Api/TypingRealm.Typing.Api.csproj", "TypingRealm.Typing.Api/"]
COPY ["TypingRealm.Typing.Core/TypingRealm.Typing.Core.csproj", "TypingRealm.Typing.Core/"]
COPY ["TypingRealm.Typing.Infrastructure/TypingRealm.Typing.Infrastructure.csproj", "TypingRealm.Typing.Infrastructure/"]
COPY ["TypingRealm.Typing/TypingRealm.Typing.csproj", "TypingRealm.Typing/"]
RUN dotnet restore "TypingRealm.Typing.Api/TypingRealm.Typing.Api.csproj"
COPY . .
WORKDIR "/src/TypingRealm.Typing.Api"
RUN dotnet build "TypingRealm.Typing.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TypingRealm.Typing.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TypingRealm.Typing.Api.dll"]