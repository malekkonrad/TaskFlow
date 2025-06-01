using System.Net;
using System.Text;
using System.Text.Json;

namespace TaskFlow.Tests;
public class ProjectTest
{
    private string baseUrl;
    private string username;
    private string token;


    public ProjectTest(string baseUrl, string username, string token)
    {
        // Konstruktor może być użyty do inicjalizacji, jeśli potrzebne
        this.baseUrl = baseUrl;
        this.username = username;
        this.token = token;
    }

    public async Task TestProjectApi()
    {
        Console.WriteLine("=== Test REST API dla ProjectAPi ===\n");

        try
        {
            // Test autoryzacji
            await TestAuthorization();

            // Test GET - pobieranie wszystkich projektów
            await TestGetAllProjects();

            // Test POST - dodawanie nowego projektu
            int newProjectId = await TestCreateProject();

            await TestGetAllProjects();

            if (newProjectId > 0)
            {
                // Test GET - pobieranie konkretnego projektu
                await TestGetProject(newProjectId);

                // Test PUT - modyfikacja projektu
                await TestUpdateProject(newProjectId);

                // Test GET - pobieranie konkretnego projektu
                await TestGetProject(newProjectId);

                // Test DELETE - usuwanie projektu
                await TestDeleteProject(newProjectId);

                // Test GET - pobieranie konkretnego projektu
                await TestGetProject(newProjectId);
            }

            Console.WriteLine("\n=== Wszystkie testy zakończone ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas testowania: {ex.Message}");
        }

    }



    private async Task TestAuthorization()
    {
        Console.WriteLine("Test autoryzacji...");

        try
        {
            var request = CreateRequest($"{baseUrl}/api/projects", "GET");
            var response = await GetResponseAsync(request);

            if (response.Contains("Unauthorized"))
            {
                Console.WriteLine("Brak autoryzacji - sprawdź username i token");
            }
            else
            {
                Console.WriteLine("Autoryzacja przeszła pomyślnie");
            }
        }
        catch (WebException ex)
        {
            if (ex.Response is HttpWebResponse httpResponse && httpResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("Błąd autoryzacji (401 Unauthorized)");
            }
            else
            {
                Console.WriteLine($"Błąd połączenia: {ex.Message}");
            }
        }

        Console.WriteLine();
    }

    private async Task TestGetAllProjects()
    {
        Console.WriteLine("Test GET - pobieranie wszystkich projektów...");

        try
        {
            var request = CreateRequest($"{baseUrl}/api/projects", "GET");
            var response = await GetResponseAsync(request);

            Console.WriteLine("Odpowiedź serwera:");
            Console.WriteLine(FormatJson(response));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }

        Console.WriteLine();
    }

    public async Task<int> TestCreateProject()
    {
        Console.WriteLine("Test POST - dodawanie nowego projektu...");

        try
        {
            var projectData = new
            {
                Name = "Test Project API",
                Description = "Projekt utworzony przez test API",
                IsPublic = true
            };

            var request = CreateRequest($"{baseUrl}/api/projects", "POST");
            var json = JsonSerializer.Serialize(projectData);

            await WriteRequestBodyAsync(request, json);
            var response = await GetResponseAsync(request);

            Console.WriteLine("Projekt utworzony pomyślnie:");
            Console.WriteLine(FormatJson(response));

            try
            {
                var jsonDoc = JsonDocument.Parse(response);
                if (jsonDoc.RootElement.TryGetProperty("id", out var idElement))
                {
                    return idElement.GetInt32();
                }
            }
            catch
            {
                throw new Exception("Nie udało się wyciągnąć ID projektu z odpowiedzi");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }

        Console.WriteLine();
        return 0;
    }

    private async Task TestGetProject(int projectId)
    {
        Console.WriteLine($"Test GET - pobieranie projektu o ID {projectId}...");

        try
        {
            var request = CreateRequest($"{baseUrl}/api/projects/{projectId}", "GET");
            var response = await GetResponseAsync(request);

            Console.WriteLine("Szczegóły projektu:");
            Console.WriteLine(FormatJson(response));
        }
        catch (WebException ex)
        {
            if (ex.Response is HttpWebResponse httpResponse && httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Projekt nie został znaleziony (404 Not Found)");
            }
            else
            {
                Console.WriteLine($"Błąd: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }

        Console.WriteLine();
    }

    private async Task TestUpdateProject(int projectId)
    {
        Console.WriteLine($"Test PUT - modyfikacja projektu o ID {projectId}...");

        try
        {
            var updatedProjectData = new
            {
                Name = "Test Project API - ZAKTUALIZOWANY",
                Description = "Projekt zaktualizowany przez test API",
                IsPublic = false
            };

            var request = CreateRequest($"{baseUrl}/api/projects/{projectId}", "PUT");
            var json = JsonSerializer.Serialize(updatedProjectData);

            await WriteRequestBodyAsync(request, json);
            var response = await GetResponseAsync(request);

            Console.WriteLine("Projekt zaktualizowany pomyślnie");
            if (!string.IsNullOrEmpty(response))
            {
                Console.WriteLine($"Odpowiedź: {response}");
            }
        }
        catch (WebException ex)
        {
            if (ex.Response is HttpWebResponse httpResponse)
            {
                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        Console.WriteLine("Projekt nie został znaleziony (404 Not Found)");
                        break;
                    case HttpStatusCode.Forbidden:
                        Console.WriteLine("Brak uprawnień do modyfikacji projektu (403 Forbidden)");
                        break;
                    default:
                        Console.WriteLine($"Błąd HTTP: {httpResponse.StatusCode}");
                        break;
                }
            }
            else
            {
                Console.WriteLine($"Błąd: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }

        Console.WriteLine();
    }

    private async Task TestDeleteProject(int projectId)
    {
        Console.WriteLine($"Test DELETE - usuwanie projektu o ID {projectId}...");

        try
        {
            var request = CreateRequest($"{baseUrl}/api/projects/{projectId}", "DELETE");
            var response = await GetResponseAsync(request);

            Console.WriteLine("Projekt usunięty pomyślnie");
            if (!string.IsNullOrEmpty(response))
            {
                Console.WriteLine($"Odpowiedź: {response}");
            }
        }
        catch (WebException ex)
        {
            if (ex.Response is HttpWebResponse httpResponse)
            {
                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        Console.WriteLine("Projekt nie został znaleziony (404 Not Found)");
                        break;
                    case HttpStatusCode.Forbidden:
                        Console.WriteLine("Brak uprawnień do usunięcia projektu (403 Forbidden)");
                        break;
                    default:
                        Console.WriteLine($"Błąd HTTP: {httpResponse.StatusCode}");
                        break;
                }
            }
            else
            {
                Console.WriteLine($"Błąd: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }

        Console.WriteLine();
    }

    private HttpWebRequest CreateRequest(string url, string method)
    {
        var request = (HttpWebRequest)WebRequest.Create(url);
        request.ContentType = "application/json";
        request.Method = method;

        // Dodanie headerów autoryzacyjnych
        request.Headers.Add("username", username);
        request.Headers.Add("token", token);

        return request;
    }

    private async Task WriteRequestBodyAsync(HttpWebRequest request, string json)
    {
        using var streamWriter = new StreamWriter(await request.GetRequestStreamAsync());
        await streamWriter.WriteAsync(json);
    }

    private static async Task<string> GetResponseAsync(HttpWebRequest request)
    {
        var response = (HttpWebResponse)await request.GetResponseAsync();
        using var streamReader = new StreamReader(response.GetResponseStream());
        return await streamReader.ReadToEndAsync();
    }

    private static string FormatJson(string json)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch
        {
            return json;
        }
    }
}