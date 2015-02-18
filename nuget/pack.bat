set OUTPUT=bin
set RESOURCE=resource
IF not exist %OUTPUT% (mkdir %OUTPUT%)
nuget pack "..\src\NVisitor\NVisitor.csproj" -OutputDirectory %OUTPUT% -BasePath %RESOURCE% -Verbose -Symbols -Build