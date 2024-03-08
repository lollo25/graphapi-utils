## Assumptions

- It is a CLI that starts, execute a command and dies; therefore:
  - CommandLine package is used to ease the usage of the CLI
  - A mechanism of auto refresh of the access token is not implemented
  - If the login fails it is logged but there is no retry since the user can simly re-run the CLI
