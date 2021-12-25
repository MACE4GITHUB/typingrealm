#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

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
COPY ["TypingRealm.Hosting/TypingRealm.Hosting.csproj", "TypingRealm.Hosting/"]
COPY ["TypingRealm.Profiles.Core/TypingRealm.Profiles.Core.csproj", "TypingRealm.Profiles.Core/"]
COPY ["TypingRealm.Texts.Api.Client/TypingRealm.Texts.Api.Client.csproj", "TypingRealm.Texts.Api.Client/"]
COPY ["TypingRealm.Texts.Api/TypingRealm.Texts.Api.csproj", "TypingRealm.Texts.Api/"]
RUN dotnet restore "TypingRealm.Texts.Api/TypingRealm.Texts.Api.csproj"
COPY . .
WORKDIR "/src/TypingRealm.Texts.Api"
RUN dotnet build "TypingRealm.Texts.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TypingRealm.Texts.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TypingRealm.Texts.Api.dll"]