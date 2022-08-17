# TypingRealm

## Folder structure outline

```
/frontend
/backend

/backend/typingrealm - libraries
/backend/{service-name} - specific service

node - nodejs version of the project
net - .NET version of the project
db - database for the service, technology-agnostic (using dbmate migrations)

/backend/typingrealm/node
/backend/typingrealm/net

/backend/typing/node
/backend/typing/net
/backend/typing/db
```