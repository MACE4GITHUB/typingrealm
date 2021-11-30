FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TypingRealm.Data.Api/TypingRealm.Data.Api.csproj", "TypingRealm.Data.Api/"]
COPY ["TypingRealm.Communication/TypingRealm.Communication.csproj", "TypingRealm.Communication/"]
COPY ["TypingRealm.Common/TypingRealm.Common.csproj", "TypingRealm.Common/"]
COPY ["TypingRealm.Hosting/TypingRealm.Hosting.csproj", "TypingRealm.Hosting/"]
COPY ["TypingRealm.Tcp/TypingRealm.Tcp.csproj", "TypingRealm.Tcp/"]
COPY ["TypingRealm.Messaging.Core/TypingRealm.Messaging.Core.csproj", "TypingRealm.Messaging.Core/"]
COPY ["TypingRealm.Messaging.Serialization.Protobuf.Core/TypingRealm.Messaging.Serialization.Protobuf.Core.csproj", "TypingRealm.Messaging.Serialization.Protobuf.Core/"]
COPY ["TypingRealm.Messaging.Serialization.Core/TypingRealm.Messaging.Serialization.Core.csproj", "TypingRealm.Messaging.Serialization.Core/"]
COPY ["TypingRealm.Messaging/TypingRealm.Messaging.csproj", "TypingRealm.Messaging/"]
COPY ["TypingRealm.Profiles.Core/TypingRealm.Profiles.Core.csproj", "TypingRealm.Profiles.Core/"]
COPY ["TypingRealm.Messaging.Client/TypingRealm.Messaging.Client.csproj", "TypingRealm.Messaging.Client/"]
COPY ["TypingRealm.Authentication/TypingRealm.Authentication.csproj", "TypingRealm.Authentication/"]
COPY ["TypingRealm.Profiles.Api.Client/TypingRealm.Profiles.Api.Client.csproj", "TypingRealm.Profiles.Api.Client/"]
COPY ["TypingRealm.Profiles.Api.Resources/TypingRealm.Profiles.Api.Resources.csproj", "TypingRealm.Profiles.Api.Resources/"]
COPY ["TypingRealm.Authentication.ConsoleClient/TypingRealm.Authentication.ConsoleClient.csproj", "TypingRealm.Authentication.ConsoleClient/"]
COPY ["TypingRealm.SignalR.Client/TypingRealm.SignalR.Client.csproj", "TypingRealm.SignalR.Client/"]
COPY ["TypingRealm.SignalR/TypingRealm.SignalR.csproj", "TypingRealm.SignalR/"]
COPY ["TypingRealm.Data.Api.Client/TypingRealm.Data.Api.Client.csproj", "TypingRealm.Data.Api.Client/"]
COPY ["TypingRealm.Data.Resources/TypingRealm.Data.Resources.csproj", "TypingRealm.Data.Resources/"]
COPY ["TypingRealm.Data.Infrastructure/TypingRealm.Data.Infrastructure.csproj", "TypingRealm.Data.Infrastructure/"]
COPY ["TypingRealm.Typing/TypingRealm.Typing.csproj", "TypingRealm.Typing/"]
RUN dotnet restore "TypingRealm.Data.Api/TypingRealm.Data.Api.csproj"
COPY . .
WORKDIR "/src/TypingRealm.Data.Api"
RUN dotnet build "TypingRealm.Data.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TypingRealm.Data.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TypingRealm.Data.Api.dll"]