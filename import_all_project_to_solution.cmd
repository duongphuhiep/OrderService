del *.sln
dotnet new sln 
FOR /R %%i IN (*.csproj) DO dotnet sln add "%%i"