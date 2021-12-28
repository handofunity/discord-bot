# Configuring app settings
## Development
### Initialization
**This topic is only included in the documentation for the sake of completeness, as the `WebHost` project has already been initialized by the project maintainers.**

Run the command `dotnet user-secrets init` in the `WebHost` project directory. This will add a `UserSecretsId` guid element to the csproj file (see [_Enable secret storage_](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows#enable-secret-storage)).

### Setting a secret
To add or change a secret for the `WebHost` project, have any command line open in the directory of the `WebHost` project, and type like the following to set individual secrets: `dotnet user-secrets set "key:with:hirachie" "valueOfKey"` (see [_Set a secret_](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows#set-a-secret)).

The following is a list of keys that **must** be set by each developer:

- `ConnectionStrings:HandOfUnityGuild`
- `ConnectionStrings:HangFire`
- `Seq:serverUrl`
- `Seq:apiKey`
- `Discord:botToken`

## Production
For the production environment, variables are set in the bash script that executes the `docker run` command.  
In that script, secrets are passed as environment variable (see [Environment variables](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#environment-variables)).  
Due to platform limitations, all `:` must be replaced by `__`.

Since the production bash script references the `settings.production.ini` file, values must be updated there, whereas the script must be extended to read the values from the `ini` file and setting the appropiate environment variable.

### Setting a secret
The following is a list of keys that **must** be set by an environment variable, each prefixed with `SETTINGS_OVERRIDE_` (e.g. `SETTINGS_OVERRIDE_ConnectionStrings__HandOfUnityGuild`):

- `ConnectionStrings__HandOfUnityGuild`
- `ConnectionStrings__HangFire`
- `Seq__serverUrl`
- `Seq__apiKey`
- `Discord__botToken`