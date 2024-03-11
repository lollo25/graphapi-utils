## Assumptions

- It is a CLI that starts, execute a command and dies; therefore:
  - CommandLine package is used to ease the usage of the CLI
  - A mechanism of auto refresh of the access token is not implemented
  - If the login fails it is logged but there is no retry since the user can simly re-run the CLI

## Limitations

- Groups are all kept in memory before the saving for the sake of simplicity, therefore this might cause high memory allocation in case of a large number of groups. If this case must be covered, the application should save each page as soon as it is retrieved.
