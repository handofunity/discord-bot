# Using run configurations
These run configurations are used by JetBrains.

## Debug
The `WebHost Debug.run.xml` is used for debugging the application locally. Follow the steps below to start debugging:
1. Open a shell with the `.run` directory as your current working directory.
2. Run `git update-index --skip-worktree "WebHost Debug.run.xml"` to ignore local changes from change tracking.
    - You can run `git update-index --no-skip-worktree "WebHost Debug.run.xml"` to track changes again, in case you modify the configuration for good.
3. Replace the `%APPDATA%` placeholder with the value of your `APPDATA` environment variable.

## Publish
The `WebHost Publish.run.xml` can be left as-is. It is only used for publishing the Docker image to the Docker hub.