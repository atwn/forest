### Задание:
Создать минималистичный веб-сервис с использованием ASP.NET Core (.NET8.0), EF Core, SQLite, Docker.

Сервис должен содержать:
- древовидную иерархическую модель данных без ограничения уровня вложенности (структура каталогов).
- минимальные Rest API покрывающие CRUD операции над этой моделью.
- добавить поддержку транзакций при изменении иерархической структуры.
- запретить создание циклических ссылок в иерархии.
- аутентификация JWT.
- авторизация по ролям (только администратор может удалять и изменять узлы).
- возможность экспорта дерева в формате JSON.

Демонстрация работы приложения через Swagger UI.

Уделить внимание чистому стилю кода и общей завершенности приложения.

### Setup:
```bash
git clone https://github.com/atwn/forest.git

docker compose build
docker compose up

# Open http://localhost:8080/swagger in your web browser
```

### Endpoints:
- `POST /auth/login` - issue JWT token
- `GET  /api/nodes/{id}` - get single node details
- `POST /api/nodes/{id}` - create node (Admin only)
- `GET  /api/nodes/search?name={name}` - get nodes by name

### Checklist:
- [x] initialize SQLite database
- [x] add Dockerfile and docker-compose.yml
- [ ] add `/login` endpoint to issue JWT
- [ ] implement `/move` endpoint
- [ ] search Node by name -- add pagination
- [ ] split Node into Node + NodeEntity (hide EF-specifics behind a repository)
- [ ] implement deleting subtrees
- [ ] (optional) input validation on Node name

### Initialize:
```bash
# Set default .NET SDK version:
dotnet new globaljson --sdk-version 8.0.417 --roll-forward latestFeature

# Projects and Solution:
dotnet new sln -n Forest
dotnet new webapi -n Forest.Api -f net8.0 --no-https
dotnet sln add .\Forest.Api\Forest.Api.csproj

# Add dependencies and create a 'lock' file:
dotnet add .\Forest.Api\Forest.Api.csproj package Microsoft.EntityFrameworkCore --version 8.*
dotnet add .\Forest.Api\Forest.Api.csproj package Microsoft.EntityFrameworkCore.Sqlite --version 8.*
dotnet add .\Forest.Api\Forest.Api.csproj package Microsoft.EntityFrameworkCore.Design --version 8.*
dotnet add .\Forest.Api\Forest.Api.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.*
dotnet restore .\Forest.Api\Forest.Api.csproj --use-lock-file

# Install CLI tools:
winget install SQLite.SQLite
dotnet new tool-manifest
dotnet tool install --global dotnet-ef --version 8.*
dotnet tool restore

# Initialize SQLite DB:
dotnet ef migrations --project .\src\Forest.Api\Forest.Api.csproj add Initialize
dotnet ef database --project .\src\Forest.Api\Forest.Api.csproj update
```