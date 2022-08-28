# TypingRealm

## Folder structure outline

```
/frontend
/backend

/backend/bots - bots for testing & for gameplay
/backend/framework - libraries
/backend/{service-name} - specific service

node - nodejs version of the project
dotnet - .NET version of the project
db - database for the service, technology-agnostic (using dbmate migrations)

/backend/framework/node
/backend/framework/dotnet

/backend/typing/node
/backend/typing/dotnet
/backend/typing/db

Component example: host in node, hosting in dotnet:

/backend/framework/node/host
/backend/framework/dotnet/TypingRealm.Hosting
```