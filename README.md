# vh-maintenance-scripts-core

## Run the application

In order to run the application, change directory to the MaintenanceScriptsCore (which contains the Program.cs) and run the following command

```bash
dotnet run staffandclerk c:\\filepath
```

For first argument the group case needs to be specified, valid group cases are:

- staffandclerk
- listingofficer
- listingofficerwithkinly
- judge

NOTE: The second argument is the file directory containing the csv file. The csv file should have the first name, last name and the email.

In GraphApiService.cs, the AzureAd needs to be configured with the correct TenantId, ClientId and ClientSecret. Current implementation is using the vh-user-api-dev app registration.
