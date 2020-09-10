FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["TypingRealm.Common/TypingRealm.Common.csproj", "TypingRealm.Common/"]
COPY ["TypingRealm.Profiles/TypingRealm.Profiles.csproj", "TypingRealm.Profiles/"]
COPY ["TypingRealm.Profiles.Api/TypingRealm.Profiles.Api.csproj", "TypingRealm.Profiles.Api/"]
COPY ["TypingRealm.Profiles.Api.Resources/TypingRealm.Profiles.Api.Resources.csproj", "TypingRealm.Profiles.Api.Resources/"]
COPY ["TypingRealm.Profiles.Infrastructure/TypingRealm.Profiles.Infrastructure.csproj", "TypingRealm.Profiles.Infrastructure/"]
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