# Cybertill SOAP Client 

This is a quick work-in-progress .NET 4.7 client for the Cybertill SOAP service.

## Set-up

To add your user secrets duplicate `appsettings.json.example` and name it `appsettings.json`.

## Usage

This solution provides two ways to interact with the Cybertill API:
- **Recommended**: A `CybertillBasicService` that provides a useful, limited interface and wraps a number of Cybertill API calls and returns sensible DTOs.
- A `CybertillClient` that provides direct access to the Cybertill API.

## Examples

The solution includes a **Cybertill.Console** app example project. Note that this app demonstrates both `CybertillBasicService` and calling the `CybertillClient` directly.

The console app includes two important example functions:
- `StockCheckExample` which retrieves all the stock levels updated in the last 7 days.
- `UpdateStockExample` which retrieves a single stock level and demonstrates reserving a single unit of that product option.
