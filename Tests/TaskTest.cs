using System.Net;
using System.Text;
using System.Text.Json;

namespace TaskFlow.Tests;
public class TaskTest
{
    private string baseUrl;
    private string username;
    private string token;


    public TaskTest(string baseUrl, string username, string token)
    {
        this.baseUrl = baseUrl;
        this.username = username;
        this.token = token;
    }

    public async Task TestTaskApi()
    {
        Console.WriteLine("=== Test REST API dla TaskApi ===\n");

        try
        {
            ProjectTest projectTest = new ProjectTest(baseUrl, username, token);
            int projectId = await projectTest.TestCreateProject();

            await TestGetAllTasks(projectId);

            int newTaskId = await TestCreateTask(projectId);

            await TestGetTask(projectId, newTaskId);

            await TestUpdateTask(projectId, newTaskId);

            await TestGetTask(projectId, newTaskId);

            await TestDeleteTask(projectId, newTaskId);

            await TestGetTask(projectId, newTaskId);


            Console.WriteLine("\n=== Wszystkie testy zakończone ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas testowania: {ex.Message}");
        }

    }


    private async Task TestGetAllTasks(int projectId)
    {
        Console.WriteLine("Test GET - pobieranie wszystkich tasków dla konkretnego projektu...");

        try
        {
            var request = CreateRequest($"{baseUrl}/api/projects/{projectId}/tasks", "GET");
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

    public async Task<int> TestCreateTask(int projectId)
    {
        Console.WriteLine("Test POST - dodawanie nowego task...");

        try
        {

            var taskData = new
            {
                Title = "Test Task",
                Description = "Task utworzony przez API",
                AssigneeId = 1,
                Deadline = DateTime.Now.AddDays(7)
            };

            var request = CreateRequest($"{baseUrl}/api/projects/{projectId}/tasks", "POST");
            var json = JsonSerializer.Serialize(taskData);

            await WriteRequestBodyAsync(request, json);
            var response = await GetResponseAsync(request);

            Console.WriteLine("Task utworzony pomyślnie:");
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
                throw new Exception("Nie udało się wyciągnąć ID task z odpowiedzi");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }

        Console.WriteLine();
        return 0;
    }

    private async Task TestGetTask(int projectId, int taskId)
    {
        Console.WriteLine($"Test GET - pobieranie task o ID {taskId}...");

        try
        {
            var request = CreateRequest($"{baseUrl}/api/projects/{projectId}/tasks/{taskId}", "GET");
            var response = await GetResponseAsync(request);

            Console.WriteLine("Szczegóły task'a:");
            Console.WriteLine(FormatJson(response));
        }
        catch (WebException ex)
        {
            if (ex.Response is HttpWebResponse httpResponse && httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Task nie został znaleziony (404 Not Found)");
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

    private async Task TestUpdateTask(int projectId, int taskId)
    {
        Console.WriteLine($"Test PUT - modyfikacja task o ID {taskId}...");

        try
        {
            var updatedTaskData = new
            {
                Title = "Test Task - ZAKTUALIZOWANY",
                Description = "Task zaktualizowany przez test API",
                AssigneeId = 1,
                Deadline = DateTime.Now.AddDays(7)
            };

            var request = CreateRequest($"{baseUrl}/api/projects/{projectId}/tasks/{taskId}", "PUT");
            var json = JsonSerializer.Serialize(updatedTaskData);

            await WriteRequestBodyAsync(request, json);
            var response = await GetResponseAsync(request);

            Console.WriteLine("Task zaktualizowany pomyślnie");
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

    private async Task TestDeleteTask(int projectId, int taskId)
    {
        Console.WriteLine($"Test DELETE - usuwanie task o ID {taskId}...");

        try
        {
            var request = CreateRequest($"{baseUrl}/api/projects/{projectId}/tasks/{taskId}", "DELETE");
            var response = await GetResponseAsync(request);

            Console.WriteLine("Task usunięty pomyślnie");
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
                        Console.WriteLine("Task nie został znaleziony (404 Not Found)");
                        break;
                    case HttpStatusCode.Forbidden:
                        Console.WriteLine("Brak uprawnień do usunięcia task (403 Forbidden)");
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