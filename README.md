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

docker compose up --build

# Open http://localhost:8080/swagger in your web browser
# Use credentials of a regular user ('user':'user') or an admin ('admin':'admin') to get a JWT token from the '/auth/login' endpoint
# Click on Authorize button and paste the JWT token
```

### Endpoints:
- `POST /auth/login` - issue JWT token
- `GET  /api/nodes/search?name={name}` - search all nodes by their names
- `GET  /api/nodes/{id}` - get single node details
- `POST /api/nodes/{id}` - create node (Admin only)

### Checklist:
- [x] initialize SQLite database
- [x] add Dockerfile and docker-compose.yml
- [x] split Node into Node + NodeEntity (hide EF-specifics behind a repository)
- [x] add `/login` endpoint to issue JWT
- [x] restrict access (implement authorization)
- [ ] implement `/move` endpoint
- [ ] implement deleting subtrees
- [ ] search Node by name -- add pagination
- [ ] use GitHub secrets to store JWT private key
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