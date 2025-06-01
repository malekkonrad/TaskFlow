using System.Net;
using System.Text;
using System.Text.Json;

namespace TaskFlow.Tests;
public class CommentTest
{
    private string baseUrl;
    private string username;
    private string token;


    public CommentTest(string baseUrl, string username, string token)
    {
        // Konstruktor może być użyty do inicjalizacji, jeśli potrzebne
        this.baseUrl = baseUrl;
        this.username = username;
        this.token = token;
    }

    public async Task TestCommentApi()
    {
        Console.WriteLine("=== Test REST API dla CommentApi ===\n");

        try
        {

            ProjectTest projectTest = new ProjectTest(baseUrl, username, token);
            int projectId = await projectTest.TestCreateProject();

            TaskTest taskTest = new TaskTest(baseUrl, username, token);
            int taskId = await taskTest.TestCreateTask(projectId);

            // Test GET - pobieranie wszystkich projektów
            await TestGetAllComments(projectId, taskId);


            int newCommentId = await TestCreateComment(projectId, taskId);

            await TestGetComment(projectId, taskId, newCommentId);

            await TestUpdateComment(projectId, taskId, newCommentId);

            await TestGetComment(projectId, taskId, newCommentId);

            await TestDeleteComment(projectId, taskId, newCommentId);

            await TestGetComment(projectId, taskId, newCommentId);


            Console.WriteLine("\n=== Wszystkie testy zakończone ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas testowania: {ex.Message}");
        }

    }


    private async Task TestGetAllComments(int projectId, int taskId)
    {
        Console.WriteLine("Test GET - pobieranie wszystkich komentarzy dla konkretnego projektu i zadania...");

        try
        {
            var request = CreateRequest($"{baseUrl}/api/projects/{projectId}/tasks/{taskId}/comments", "GET");
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

    private async Task<int> TestCreateComment(int projectId, int taskId)
    {
        Console.WriteLine("Test POST - dodawanie nowego komentarza...");

        try
        {

            var commentData = new
            {
                taskItemId = taskId,
                Content = "To jest testowy komentarz",
                AuthorId = 1 // Zakładamy, że ID autora to 1
            };

            var request = CreateRequest($"{baseUrl}/api/projects/{projectId}/tasks/{taskId}/comments", "POST");
            var json = JsonSerializer.Serialize(commentData);

            await WriteRequestBodyAsync(request, json);
            var response = await GetResponseAsync(request);

            Console.WriteLine("Komentarz utworzony pomyślnie:");
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
                throw new Exception("Nie udało się wyciągnąć ID komentarza z odpowiedzi");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }

        Console.WriteLine();
        return 0;
    }

    private async Task TestGetComment(int projectId, int taskId, int commentId)
    {
        Console.WriteLine($"Test GET - pobieranie komentarza o ID {commentId}...");

        try
        {
            var request = CreateRequest($"{baseUrl}/api/projects/{projectId}/tasks/{taskId}/comments/{commentId}", "GET");
            var response = await GetResponseAsync(request);

            Console.WriteLine("Szczegóły komentarza:");
            Console.WriteLine(FormatJson(response));
        }
        catch (WebException ex)
        {
            if (ex.Response is HttpWebResponse httpResponse && httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Komentarz nie został znaleziony (404 Not Found)");
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

    private async Task TestUpdateComment(int projectId, int taskId, int commentId)
    {
        Console.WriteLine($"Test PUT - modyfikacja komentarza o ID {commentId}...");

        try
        {


            var updatedCommentData = new
            {
                taskItemId = taskId,
                Content = "To jest testowy komentarz - ZAKTUALIZOWANY",
                AuthorId = 1 // Zakładamy, że ID autora to 1
            };

            var request = CreateRequest($"{baseUrl}/api/projects/{projectId}/tasks/{taskId}/comments/{commentId}", "PUT");
            var json = JsonSerializer.Serialize(updatedCommentData);

            await WriteRequestBodyAsync(request, json);
            var response = await GetResponseAsync(request);

            Console.WriteLine("Komentarz zaktualizowany pomyślnie");
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
                        Console.WriteLine("Komentarz nie został znaleziony (404 Not Found)");
                        break;
                    case HttpStatusCode.Forbidden:
                        Console.WriteLine("Brak uprawnień do modyfikacji komentarzas (403 Forbidden)");
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

    private async Task TestDeleteComment(int projectId, int taskId, int commentId)
    {
        Console.WriteLine($"Test DELETE - usuwanie komentarza o ID {commentId}...");

        try
        {
            var request = CreateRequest($"{baseUrl}/api/projects/{projectId}/tasks/{taskId}/comments/{commentId}", "DELETE");
            var response = await GetResponseAsync(request);

            Console.WriteLine("Komentarz usunięty pomyślnie");
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
                        Console.WriteLine("KOmentarza nie został znaleziony (404 Not Found)");
                        break;
                    case HttpStatusCode.Forbidden:
                        Console.WriteLine("Brak uprawnień do usunięcia KOmentarza (403 Forbidden)");
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