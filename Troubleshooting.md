# Troubleshooting
## Build errors
### docker-compose
#### Error
`docker-compose` might throw the following error when building or debugging the docker container:

    Cannot start service webhost: driver failed programming external connectivity on endpoint [...]
    Error starting userland proxy [...]

This error is caused because a previous instance of a docker image is blocking the port the new image wants to use.

#### Solution
Restart the `Docker for Windows`-VM.

### dotnet restore
#### Error
`dotnet restore` might throw the following error when building in **Release** configuration:

    Unable to load the service index for source https://api.nuget.org/v3/index.json.
    An error occurred while sending the request.
    Couldn't resolve host name

This error is caused by the Docker VM (specifically the networking), which prevents the VM from resolving the `api.nuget.org`-URL.

#### Solution
Restart the `Docker for Windows`-VM.