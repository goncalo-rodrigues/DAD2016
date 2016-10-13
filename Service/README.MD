1. Open Visual Studio Solution of ProcessCreationService
2. Build the solution (ctrl+shift+b)
3. Open Developer Command Prompt for VS2015 as Administrator
4. cd to wherever the repo is (example: cd C:\Users\username\Source\Repos\DAD2016\Service\ProcessCreationService\bin\Debug\)
5. installutil ProcessCreationService.exe
6. A permission dialog will show up. Fill it with your username and password. You should add .\ before your username to indicate that it is a local user. Example (Username: .\Goncalo Password: xxxxxx)
7. NET START ProcessCreationService /path_to_operator.exe
where path_to_operator.exe should be replaced with the path to the operator exe file. (example: NET START /C:\Users\username\Source\Repos\DAD2016\Operator\Operator\bin\Debug\Operator.exe)
