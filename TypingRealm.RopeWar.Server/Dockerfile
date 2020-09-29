FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["TypingRealm.RopeWar.Server/TypingRealm.RopeWar.Server.csproj", "TypingRealm.RopeWar.Server/"]
COPY ["TypingRealm.Communication/TypingRealm.Communication.csproj", "TypingRealm.Communication/"]
COPY ["TypingRealm.Messaging.Serialization.Core/TypingRealm.Messaging.Serialization.Core.csproj", "TypingRealm.Messaging.Serialization.Core/"]
COPY ["TypingRealm.Messaging.Core/TypingRealm.Messaging.Core.csproj", "TypingRealm.Messaging.Core/"]
COPY ["TypingRealm.Common/TypingRealm.Common.csproj", "TypingRealm.Common/"]
COPY ["TypingRealm.Messaging/TypingRealm.Messaging.csproj", "TypingRealm.Messaging/"]
COPY ["TypingRealm.Authentication/TypingRealm.Authentication.csproj", "TypingRealm.Authentication/"]
COPY ["TypingRealm.Profiles.Api.Client/TypingRealm.Profiles.Api.Client.csproj", "TypingRealm.Profiles.Api.Client/"]
COPY ["TypingRealm.RopeWar/TypingRealm.RopeWar.csproj", "TypingRealm.RopeWar/"]
COPY ["TypingRealm.SignalR/TypingRealm.SignalR.csproj", "TypingRealm.SignalR/"]
RUN dotnet restore "TypingRealm.RopeWar.Server/TypingRealm.RopeWar.Server.csproj"
COPY . .
WORKDIR "/src/TypingRealm.RopeWar.Server"
RUN dotnet build "TypingRealm.RopeWar.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TypingRealm.RopeWar.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TypingRealm.RopeWar.Server.dll"]