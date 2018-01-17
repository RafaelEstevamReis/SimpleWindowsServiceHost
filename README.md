# SimpleWindowsServiceHost
A simple configurable windows service host for test purposes written in C#

This program can be installed as windows service (open with -h parameter for help)
Then it read it's xml configuration file and launch the application path and parameters.

You can configure:
  Service Name
  Service Display Name
  Service Description
  Run Program Path
  Run Program Params
  Gracefully close sendind a command (with configurable timeout)
  Delete-me file to easy kill the process
