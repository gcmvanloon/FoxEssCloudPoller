# FoxESS Cloud Poller
Get your FoxESS solar inverter data and put it to good use!

This DotNet Core console application gets your FoxESS solar inverter data from
the FoxESS cloud API and posts it to [PVOutput.org](https://www.pvoutput.org)

## Configuration
You need to change the configuration in the appsettings.json to match your own FoxESS cloud username and password.
The `InverterId` can be found form the [FoxESS cloud website](https://foxesscloud.com). Choose
"Device" -> "Inverter" after you logged on and click the icon under "More Options" to get to the "Inverter Details" page.
There your can find your InverterId in the url of the page (`id=00000000-0000-0000-0000-000000000000`).

In order to access your PVOutput system you need an API key and a systemId from PVOutput. You can get both from the settings page.

### DotNet Core Configuration providers
If you're a developer and run the solution from Visual Studio you should be aware of the included launchSettings.json file that will
set an environment variable `NETCORE_ENVIRONMENT=Development`. With this environment variable set the `appsettings.{$NETCORE_ENVIRONMENT}.json` file
will take precedence over the settings in `appsettings.json`. This is currently being used to set the loglevel to debug for `Development`.

It is highly recommended to use the "User Secrets" configuration option to prevent you from commiting your personal account information to github.


### Example config
Here is an example of how things should look once everything is entered in the appsettings.json file:
```json
  "FoxEssCloud": {
    "User": "JohnnyB",
    "Password": "Welcome123",
    "InverterId": "9826eabd-79d5-4bbf-bd1f-0424da73e639"
  },
  "PVOutput": {
    "ApiKey": "4d74f723727c4c0857daaa464b0772b240d8a615",
    "SystemId": 12345
  }
```