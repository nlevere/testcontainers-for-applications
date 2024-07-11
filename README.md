# Share Testcontainers with application

This solution is a POC on how to share Testcontainers between integration tests and application development. The goal of this solution is to share
Testcontainers environment between integration tests and the application with minimal impact and using existing .NET features.

The solution that is covered in this POC is using [Host startup hook](https://github.com/dotnet/runtime/blob/main/docs/design/features/host-startup-hook.md)
to "inject" the test environment on-demand when the application is started.

## Project Layout

This solution contains 4 projects. 2 of the projects are the application and integration tests and the other 2 are the host startup hook and the test environment (Testcontainers).

StartupHook
: This library is the code that is called when the application starts.

TestEnvironment
: This library is the main code for creating the Testcontainers environment. It is used by the `StartupHook` and `WeatherApp.Integration.Tests`

WeatherApp
: This ASP .NET app is the default Microsoft webapi application. The only changes made to this app was to add saving the weather forecasts to a reporting database.

WeatherApp.Integration.Tests
: This is the integration tests for the WeatherApp. It contains code that will create the Testcontainers environment along with a sample repository test.

## Running

### Setup

Before you are able to run the application, the `StartupHook` needs to be published and launchSettings.json will need to be updated with the location of the published `StartupHook`.

#### Publish StartupHook

Before the `StartupHook` can be used it will need to be published. To publish to library, run the following commands:

```sh
cd StartupHook
dotnet publish
```

Output of command should look like this:

```sh
  Determining projects to restore...
  All projects are up-to-date for restore.
  TestEnvironment -> /code/StartupHook/TestEnvironment/bin/Release/net8.0/TestEnvironment.dll
  StartupHook -> /Code/StartupHook/StartupHook/bin/Release/net8.0/StartupHook.dll
  StartupHook -> /Code/StartupHook/StartupHook/bin/Release/net8.0/publish/
```

This will build and publish `StartupHook` and `TestEnvironment` to `/Code/StartupHook/StartupHook/bin/Release/net8.0/publish`. Note that your path might be different based on the location of the solution folder.

If you need to debug the `StartupHook` or `TestEnvironment`, you can also run the `dotnet publish` command with a different configuration using the -c|configuration option:

```sh
cd StartupHook
dotnet publish -c Debug
```

This will produce similar output as above except Release will be replaced with Debug.

Remember when making changes to `StartupHook` or `TestEnvironment`, you will need to re-publish the `StartupHook` using the commands above.

#### Update launchSettings.json

Open `launchSettings.json` and change all of the `"DOTNET_STARTUP_HOOKS": "/Code/StartupHook/StartupHook/bin/Release/net8.0/publish/StartupHook.dll"` line to reflect the location from the output above. Once this is set, it will not have to be updated unless the location of the `StartupHook` changes.

After this change, there will be 5 launch profiles that can be used to launch the application:

* http
* https
* http With TestEnvironment
* https with TestEnvironment
* IIS Express

### Running integration tests

Using Testcontainers makes it easy to run your integration tests with out the need of creating and maintaining docker compose files. To run your tests is as simple as running the following commands:

```sh
dotnet test --filter="Category=Integration"
```

Because this solution does not currently have Unit or any other tests, you can also run the integration tests by just running `dotnet test`.

### Running the application

There are many ways to run the application. If you IDE supports setting the launch profile, you can run the application using the desired launch profile.

In the example code, the application is configured to use localhost:5432 as the default PostgreSql server as defined in `appSettings.json` when using the following launch profiles:

* http
* https
* IIS Express

When using the Testcontainers environment, the connection string is overridden to point to the server running in docker. Use the following launch profiles to enable the Testcontainers environment:

* http With TestEnvironment
* https with TestEnvironment

#### Running with dotnet cli

Use the following to run the application with or without the TestContainers environment using one of the profiles listed above. For example:

```sh
dotnet run --launch-profile "http With TestEnvironment" --project WeatherApp
```

This command will run the application with the Testcontainers environment on an unsecured connection (http).

Note: If you run `dotnet run` without using the --launchProfile option, the application will use the profile that is defined first. In the case of this application, it would use the `http` profile.
