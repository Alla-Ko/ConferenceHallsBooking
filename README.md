# ConferenceHalls API

ASP.NET Core Web API для оренди конференц-залів: каталог, бронювання, розрахунок ціни та звіти для адміністратора.

## Що потрібно

- .NET 10 SDK

## Запуск

```bash
dotnet build
dotnet run
```

При старті застосунок:

1. Автоматично застосовує всі EF Core міграції
2. Створює локальну SQLite-базу `Infrastructure/Persistence/conferencehalls.db` (файл не комітиться)
3. Заповнює ролі, дефолтні зали/послуги (якщо таблиці порожні) і дефолтного адміна

Swagger UI після запуску:

**http://localhost:5070/swagger/index.html**

(`http://localhost:5070/swagger` теж відкриває ту саму сторінку.)

OpenAPI-документ: `http://localhost:5070/openapi/v1.json`

## Дефолтний адмін

|          |                               |
| -------- | ----------------------------- |
| Email    | `admin@conferencehalls.local` |
| Password | `Admin123!`                   |

Логін, пароль адміна та JWT-ключ (`Jwt:Key`) **свідомо** лежать у `appsettings.json` — лише для локальної розробки. На продакшені так робити не можна.

На продакшені email та пароль дефолтного адміна, JWT key і рядок підключення треба винести в environment variables (або `.env` / secret store). Файли `.env` є в `.gitignore` і в репозиторій не потрапляють.

## Що в git, а що ні

У репозиторії: код, **міграції** і **сідер** (`DbSeeder.cs`).

Не в репозиторії: файли SQLite (`*.db`, `*.db-shm`, `*.db-wal`). База створиться локально після першого `dotnet run`.

## Документація

Правила (включно з тими, яких не було в ТЗ), контролери та user flow: `DOCUMENTATION.md`
