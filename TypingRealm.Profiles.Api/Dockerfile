FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TypingRealm.Profiles.Api/TypingRealm.Profiles.Api.csproj", "TypingRealm.Profiles.Api/"]
COPY ["TypingRealm.Profiles.Infrastructure/TypingRealm.Profiles.Infrastructure.csproj", "TypingRealm.Profiles.Infrastructure/"]
COPY ["TypingRealm.Profiles.Core/TypingRealm.Profiles.Core.csproj", "TypingRealm.Profiles.Core/"]
COPY ["TypingRealm.Common/TypingRealm.Common.csproj", "TypingRealm.Common/"]
COPY ["TypingRealm.Profiles.Api.Resources/TypingRealm.Profiles.Api.Resources.csproj", "TypingRealm.Profiles.Api.Resources/"]
COPY ["TypingRealm.Profiles/TypingRealm.Profiles.csproj", "TypingRealm.Profiles/"]
COPY ["TypingRealm.Communication/TypingRealm.Communication.csproj", "TypingRealm.Communication/"]
COPY ["TypingRealm.Authentication.Core/TypingRealm.Authentication.Core.csproj", "TypingRealm.Authentication.Core/"]
COPY ["TypingRealm.Hosting/TypingRealm.Hosting.csproj", "TypingRealm.Hosting/"]
COPY ["TypingRealm.Tcp/TypingRealm.Tcp.csproj", "TypingRealm.Tcp/"]
COPY ["TypingRealm.Messaging.Core/TypingRealm.Messaging.Core.csproj", "TypingRealm.Messaging.Core/"]
COPY ["TypingRealm.Messaging.Serialization.Protobuf.Core/TypingRealm.Messaging.Serialization.Protobuf.Core.csproj", "TypingRealm.Messaging.Serialization.Protobuf.Core/"]
COPY ["TypingRealm.Messaging.Serialization.Core/TypingRealm.Messaging.Serialization.Core.csproj", "TypingRealm.Messaging.Serialization.Core/"]
COPY ["TypingRealm.Messaging/TypingRealm.Messaging.csproj", "TypingRealm.Messaging/"]
COPY ["TypingRealm.Messaging.Client/TypingRealm.Messaging.Client.csproj", "TypingRealm.Messaging.Client/"]
COPY ["TypingRealm.Authentication.Service/TypingRealm.Authentication.Service.csproj", "TypingRealm.Authentication.Service/"]
COPY ["TypingRealm.Authentication/TypingRealm.Authentication.csproj", "TypingRealm.Authentication/"]
COPY ["TypingRealm.Profiles.Api.Client/TypingRealm.Profiles.Api.Client.csproj", "TypingRealm.Profiles.Api.Client/"]
COPY ["TypingRealm.Authentication.ConsoleClient/TypingRealm.Authentication.ConsoleClient.csproj", "TypingRealm.Authentication.ConsoleClient/"]
COPY ["TypingRealm.SignalR.Client/TypingRealm.SignalR.Client.csproj", "TypingRealm.SignalR.Client/"]
COPY ["TypingRealm.SignalR/TypingRealm.SignalR.csproj", "TypingRealm.SignalR/"]
COPY ["TypingRealm.Data.Api.Client/TypingRealm.Data.Api.Client.csproj", "TypingRealm.Data.Api.Client/"]
COPY ["TypingRealm.Data.Resources/TypingRealm.Data.Resources.csproj", "TypingRealm.Data.Resources/"]
RUN dotnet restore "TypingRealm.Profiles.Api/TypingRealm.Profiles.Api.csproj"
COPY . .
WORKDIR "/src/TypingRealm.Profiles.Api"
RUN dotnet build "TypingRealm.Profiles.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TypingRealm.Profiles.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TypingRealm.Profiles.Api.dll"]