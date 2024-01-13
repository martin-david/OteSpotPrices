# OteSpotPrices
App written in [.NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8) to display Czech electricity spot prices based on [OTE](https://www.ote-cr.cz/en/).
The price index is consumed via official OTE WCF Service and calculated from EUR/MWh to CZK/kWh.
Currency calculation is done via [Czech National Bank](https://www.cnb.cz/en/).
Data are persisted into [Azure Cosmos DB (NoSQL)](https://azure.microsoft.com/en-us/products/cosmos-db) via [EF Core](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app). Azure Cosmos DB connection string is stored in [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0) for development environment and for production is stored in [Azure Key Vault](https://azure.microsoft.com/en-us/products/key-vault).

## Docker
Create UserSecrets with following record:<br/>
{ "OteSpotPrices:AzureKeyVaultClientSecret": "Azure Key Vault Secret From App Registration / Certificates & secrets / Client Secrets" / Value }<br/>
%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json<br/>
https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows<br/>
For docker run params check [Dockerfile](src/Dockerfile)

![Workflow status badge](https://github.com/martin-david/OteSpotPrices/actions/workflows/build.yml/badge.svg)
[![codecov](https://codecov.io/github/martin-david/OteSpotPrices/graph/badge.svg?token=2K3534OXTC)](https://codecov.io/github/martin-david/OteSpotPrices)
![Docker Hub Push](https://github.com/martin-david/OteSpotPrices/actions/workflows/docker_ci.yml/badge.svg)
