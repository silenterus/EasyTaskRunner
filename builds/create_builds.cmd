@echo Off
dotnet new console --name ProjectName.Win
cd ProjectName.Win
dotnet add package Bullseye
dotnet add package SimpleExec
cd..



dotnet new console --name ProjectName.Mac
cd ProjectName.Mac
dotnet add package Bullseye
dotnet add package SimpleExec
cd..

dotnet new console --name ProjectName.Ios
cd ProjectName.Ios
dotnet add package Bullseye
dotnet add package SimpleExec
cd..


dotnet new console --name ProjectName.Android
cd ProjectName.Android
dotnet add package Bullseye
dotnet add package SimpleExec
cd..


dotnet new console --name ProjectName.Linux
cd ProjectName.Linux
dotnet add package Bullseye
dotnet add package SimpleExec
cd..


dotnet new console --name ProjectName.Browser
cd ProjectName.Browser
dotnet add package Bullseye
dotnet add package SimpleExec
cd..
