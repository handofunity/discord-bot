# Troubleshooting
## Build errors with Docker
### docker-compose
#### Error
`docker-compose` might throw the following error when building or debugging the docker container:

    Cannot start service webhost: driver failed programming external connectivity on endpoint [...]
    Error starting userland proxy [...]

This error is caused because a previous instance of a docker image is blocking the port the new image wants to use.

#### Solution
Restart `Docker for Windows`.