FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TypingRealm.Authentication.ConsoleClient/TypingRealm.Authentication.ConsoleClient.csproj", "TypingRealm.Authentication.ConsoleClient/"]
COPY ["TypingRealm.Authentication.Core/TypingRealm.Authentication.Core.csproj", "TypingRealm.Authentication.Core/"]
COPY ["TypingRealm.Authentication.Service/TypingRealm.Authentication.Service.csproj", "TypingRealm.Authentication.Service/"]
COPY ["TypingRealm.Authentication/TypingRealm.Authentication.csproj", "TypingRealm.Authentication/"]
COPY ["TypingRealm.Common/TypingRealm.Common.csproj", "TypingRealm.Common/"]
COPY ["TypingRealm.Communication.Redis/TypingRealm.Communication.Redis.csproj", "TypingRealm.Communication.Redis/"]
COPY ["TypingRealm.Communication/TypingRealm.Communication.csproj", "TypingRealm.Communication/"]
COPY ["TypingRealm.Configuration/TypingRealm.Configuration.csproj", "TypingRealm.Configuration/"]
COPY ["TypingRealm.Data.Api.Client/TypingRealm.Data.Api.Client.csproj", "TypingRealm.Data.Api.Client/"]
COPY ["TypingRealm.Data.Resources/TypingRealm.Data.Resources.csproj", "TypingRealm.Data.Resources/"]
COPY ["TypingRealm.Hosting.Service/TypingRealm.Hosting.Service.csproj", "TypingRealm.Hosting.Service/"]
COPY ["TypingRealm.Hosting/TypingRealm.Hosting.csproj", "TypingRealm.Hosting/"]
COPY ["TypingRealm.Logging/TypingRealm.Logging.csproj", "TypingRealm.Logging/"]
COPY ["TypingRealm.Messaging.Client/TypingRealm.Messaging.Client.csproj", "TypingRealm.Messaging.Client/"]
COPY ["TypingRealm.Messaging.Core/TypingRealm.Messaging.Core.csproj", "TypingRealm.Messaging.Core/"]
COPY ["TypingRealm.Messaging.Serialization.Core/TypingRealm.Messaging.Serialization.Core.csproj", "TypingRealm.Messaging.Serialization.Core/"]
COPY ["TypingRealm.Messaging.Serialization.Protobuf.Core/TypingRealm.Messaging.Serialization.Protobuf.Core.csproj", "TypingRealm.Messaging.Serialization.Protobuf.Core/"]
COPY ["TypingRealm.Messaging/TypingRealm.Messaging.csproj", "TypingRealm.Messaging/"]
COPY ["TypingRealm.Profiles.Api.Client/TypingRealm.Profiles.Api.Client.csproj", "TypingRealm.Profiles.Api.Client/"]
COPY ["TypingRealm.Profiles.Api.Resources/TypingRealm.Profiles.Api.Resources.csproj", "TypingRealm.Profiles.Api.Resources/"]
COPY ["TypingRealm.Profiles.Core/TypingRealm.Profiles.Core.csproj", "TypingRealm.Profiles.Core/"]
COPY ["TypingRealm.Serialization/TypingRealm.Serialization.csproj", "TypingRealm.Serialization/"]
COPY ["TypingRealm.SignalR.Client/TypingRealm.SignalR.Client.csproj", "TypingRealm.SignalR.Client/"]
COPY ["TypingRealm.SignalR/TypingRealm.SignalR.csproj", "TypingRealm.SignalR/"]
COPY ["TypingRealm.Tcp/TypingRealm.Tcp.csproj", "TypingRealm.Tcp/"]
COPY ["TypingRealm.TypingDuels.Api/TypingRealm.TypingDuels.Api.csproj", "TypingRealm.TypingDuels.Api/"]
COPY ["TypingRealm.TypingDuels.Core/TypingRealm.TypingDuels.Core.csproj", "TypingRealm.TypingDuels.Core/"]
COPY ["TypingRealm.TypingDuels.Infrastructure/TypingRealm.TypingDuels.Infrastructure.csproj", "TypingRealm.TypingDuels.Infrastructure/"]
COPY ["TypingRealm.TypingDuels/TypingRealm.TypingDuels.csproj", "TypingRealm.TypingDuels/"]
RUN dotnet restore "TypingRealm.TypingDuels.Api/TypingRealm.TypingDuels.Api.csproj"
COPY . .
WORKDIR "/src/TypingRealm.TypingDuels.Api"
RUN dotnet build "TypingRealm.TypingDuels.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TypingRealm.TypingDuels.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TypingRealm.TypingDuels.Api.dll"]