FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["TypingRealm.RopeWar.TcpServer/TypingRealm.RopeWar.TcpServer.csproj", "TypingRealm.RopeWar.TcpServer/"]
COPY ["TypingRealm.Communication/TypingRealm.Communication.csproj", "TypingRealm.Communication/"]
COPY ["TypingRealm.Messaging.Core/TypingRealm.Messaging.Core.csproj", "TypingRealm.Messaging.Core/"]
COPY ["TypingRealm.Common/TypingRealm.Common.csproj", "TypingRealm.Common/"]
COPY ["TypingRealm.Messaging.Serialization.Protobuf.Core/TypingRealm.Messaging.Serialization.Protobuf.Core.csproj", "TypingRealm.Messaging.Serialization.Protobuf.Core/"]
COPY ["TypingRealm.Messaging.Serialization.Core/TypingRealm.Messaging.Serialization.Core.csproj", "TypingRealm.Messaging.Serialization.Core/"]
COPY ["TypingRealm.Messaging/TypingRealm.Messaging.csproj", "TypingRealm.Messaging/"]
COPY ["TypingRealm.Authentication/TypingRealm.Authentication.csproj", "TypingRealm.Authentication/"]
COPY ["TypingRealm.RopeWar/TypingRealm.RopeWar.csproj", "TypingRealm.RopeWar/"]
RUN dotnet restore "TypingRealm.RopeWar.TcpServer/TypingRealm.RopeWar.TcpServer.csproj"
COPY . .
WORKDIR "/src/TypingRealm.RopeWar.TcpServer"
RUN dotnet build "TypingRealm.RopeWar.TcpServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TypingRealm.RopeWar.TcpServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TypingRealm.RopeWar.TcpServer.dll"]