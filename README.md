# OteSpotPrices
App written in [.NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8) to display Czech electricity spot prices based on [OTE](https://www.ote-cr.cz/en/).
The price index is consumed via official OTE WCF Service and calculated from EUR/MWh to CZK/kWh.
Currency calculation is done via [Czech National Bank](https://www.cnb.cz/en/).
Data are persisted into [Azure Cosmos DB (NoSQL)](https://azure.microsoft.com/en-us/products/cosmos-db) via [EF Core](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app).

![Workflow status badge](https://github.com/martin-david/OteSpotPrices/actions/workflows/build.yml/badge.svg)
[![codecov](https://codecov.io/github/martin-david/OteSpotPrices/graph/badge.svg?token=2K3534OXTC)](https://codecov.io/github/martin-david/OteSpotPrices)
