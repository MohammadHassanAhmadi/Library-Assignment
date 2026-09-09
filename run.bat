start "Library Service" cmd /k "dotnet run --project src/Library.Service"
start "Library API" cmd /k "dotnet run --project src/Library.Api"

timeout /t 15 /nobreak >nul
start "" "http://localhost:5255/swagger"