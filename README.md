# FoxESS Cloud Poller
Get your FoxESS solar inverter data and put it to good use!

This DotNet Core console application gets your FoxESS solar inverter data from
the FoxESS cloud API and posts it to [PVOutput.org](https://www.pvoutput.org)

## Configuration
You need to change the configuration in `appsettings.json` to match your own FoxESS cloud ApiKey and DeviceSerialNr.
You can get an ApiKey from the FoxESS cloud website. The DeviceSerialNr is the serial number of your solar inverter.

In order to access your PVOutput system you need an API key and a systemId from PVOutput. You can get both from the [settings](https://pvoutput.org/account.jsp) page.

### DotNet Core Configuration providers
If you're a developer and run the solution from Visual Studio you should be aware of the included launchSettings.json file that will
set an environment variable `NETCORE_ENVIRONMENT=Development`. With this environment variable set, the `appsettings.{$NETCORE_ENVIRONMENT}.json` file
will take precedence over the settings in `appsettings.json`. This is currently being used to set the loglevel to debug for `Development`.

It is highly recommended to use the ["User Secrets"](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-7.0&tabs=windows)
configuration option to prevent you from commiting your personal account information to github.

### Example config
Here is an example of how things should look once everything is entered in the appsettings.json file:
```json
  "FoxEssCloud": {
    "ApiKey": "9826eabd-79d5-4bbf-bd1f-0424da73e639",
    "DeviceSerialNr": "435Z563957XY321"
  },
  "PVOutput": {
    "ApiKey": "4d74f723727c4c0857daaa464b0772b240d8a615",
    "SystemId": 12345
  }
```

> Note: the values in this sample configuration are completely fake. You should provide your own keys, serial number and system id.

## What does it do
FoxESS solar inverters log their data like generated power, temperature and voltage to the FoxESS cloud when equiped with a WiFi module.
A new data point is logged about every three minutes.
The application connects to the FoxESS OpenAPI to get the "realtime" data from your solar inverter at a fixed interval of 3 minutes.
(the same interval as the inverter logs the data to the cloud)
The data that is received from the FoxESS cloud API is then forwarded to PVOutput.org with the correct local timestamp and generated power
converted from kilo Watt to Watt.

## Docker
It is possible to run the FoxEssCloudPoller application in a container on a home server like a NAS.
There is a [pre-build container image available on docker hub](https://hub.docker.com/r/gcmvanloon/foxesscloud-poller).

### docker build
to build the container: `docker build . --tag foxesscloudpoller:latest`

### docker run
The configuration in appsettings.json can be overwritten using environment variables:
```
docker run --env FoxEssCloud__ApiKEy=9826eabd-79d5-4bbf-bd1f-0424da73e639 --env FoxEssCloud__DeviceSerialNr="435Z563957XY321" --env PVOutput__ApiKey=4d74f723727c4c0857daaa464b0772b240d8a615 --env PVOutput__SystemId=12345 -d foxesscloudpoller:dev
```


## What is next?
The PVOutput forwarder is a first implementation of the `IHandleNewInverterMeasurements` interface.
I plan for more handlers in the future, like writing to a CSV file or publishing the measurements to a [mosquitto](https://mosquitto.org/) message broker
for Home Assistent integration.
