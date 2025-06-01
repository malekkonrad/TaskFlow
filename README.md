# TaskFlow

Aplikacja internetowa do zarządzania projektami i zadaniami zbudowana w technologii ASP.NET Core MVC z REST API.

## Opis

TaskFlow to system zarządzania projektami umożliwiający:
- Tworzenie i zarządzanie projektami
- Przypisywanie zadań użytkownikom
- Dodawanie komentarzy do zadań
- Zarządzanie statusami zadań
- System autoryzacji i uwierzytelniania

## Funkcjonalności

### Dla użytkowników:
- Rejestracja i logowanie do systemu
- Przeglądanie przypisanych zadań członkom projektu
- Tworzenie nowych projektów
- Dodawanie zadań do projektów
- Dodawanie członków do projektu
- Komentowanie zadań
- Zmiana statusów zadań

### Dla administratorów:
- Zarządzanie użytkownikami
- Dodawanie statusów


### REST API:
- Pełne API do zarządzania projektami (`/api/projects`)
- API do zarządzania zadaniami (`/api/projects/{id}/tasks`)
- API do komentarzy (`/api/projects/{id}/tasks/{id}/comments`)
- Autoryzacja poprzez username i token w nagłówkach

## Technologie

- **Backend**: ASP.NET Core MVC 7.0
- **Baza danych**: SQLite z Entity Framework Core
- **Uwierzytelnianie**: Session-based authentication
- **API**: REST API z Swagger/OpenAPI
- **Frontend**: Razor Views

Projekt wykorzystuje 4+ powiązanych tabel:
- `Users` - użytkownicy systemu
- `Projects` - projekty
- `UserTasks` - zadania w projektach
- `Comments` - komentarze do zadań
- `Statuses` - statusy zadań
- `ProjectMembers` - członkowie projektów

## Instalacja i uruchomienie

1. Sklonuj repozytorium
2. Przejdź do katalogu projektu
3. Zainstaluj zależności:
   ```bash
   dotnet restore
   ```
4. Uruchom migracje bazy danych
    ```bash
    dotnet ef database update
    ```
5. Uruchom aplikację:
    ```bash
    dotnet run
    ```
6. Otwórz przeglądarkę pod adresem https://localhost:5200


