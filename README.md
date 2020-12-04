# Cybertill SOAP API Example

This is a quick work-in-progress example for using a .NET client to interact with the Cybertill SOAP service.

## Set-up

Override the app settings with user secrets. You can either right-click the project and use *Manage User Secrets* or use the command-line, for example:

```
dotnet user-secrets set Cybertill:EndpointUrl "https://ct1234xx.c-pos.co.uk/current/CybertillApi_v1_6.php"
```