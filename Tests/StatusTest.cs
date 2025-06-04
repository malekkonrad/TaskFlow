using System.Net;
using System.Text;
using System.Text.Json;

namespace TaskFlow.Tests;
public class StatusTest
{
    private string baseUrl;
    private string username;
    private string token;


    public StatusTest(string baseUrl, string username, string token)
    {
        this.baseUrl = baseUrl;
        this.username = username;
        this.token = token;
    }

    public async Task TestTaskApi()
    {
        Console.WriteLine("=== Test REST API dla StatusApi ===\n");

        try
        {
            await TestGetAllStatuses();

            int newStatusId = await TestCreateStatus();

            await TestGetStatus(newStatusId);

            await TestUpdateStatus(newStatusId);

            await TestGetStatus(newStatusId);

            await TestDeleteStatus(newStatusId);

            await TestGetStatus(newStatusId);


            Console.WriteLine("\n=== Wszystkie testy zakończone ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas testowania: {ex.Message}");
        }

    }


    private async Task TestGetAllStatuses()
    {
        Console.WriteLine("Test GET - pobieranie wszystkich statusów...");

        try
        {
            var request = CreateRequest($"{baseUrl}/api/statuses", "GET");
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

    private async Task<int> TestCreateStatus()
    {
        Console.WriteLine("Test POST - dodawanie nowego statusu...");

        try
        {
            var statusData = new
            {
                Name = "Test Status",
            };

            var request = CreateRequest($"{baseUrl}/api/statuses", "POST");
            var json = JsonSerializer.Serialize(statusData);

            await WriteRequestBodyAsync(request, json);
            var response = await GetResponseAsync(request);

            Console.WriteLine("Status utworzony pomyślnie:");
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
                throw new Exception("Nie udało się wyciągnąć ID status z odpowiedzi");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }

        Console.WriteLine();
        return 0;
    }

    private async Task TestGetStatus(int statusId)
    {
        Console.WriteLine($"Test GET - pobieranie statusu o ID {statusId}...");

        try
        {
            var request = CreateRequest($"{baseUrl}/api/statuses/{statusId}", "GET");
            var response = await GetResponseAsync(request);

            Console.WriteLine("Szczegóły task'a:");
            Console.WriteLine(FormatJson(response));
        }
        catch (WebException ex)
        {
            if (ex.Response is HttpWebResponse httpResponse && httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Status nie został znaleziony (404 Not Found)");
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

    private async Task TestUpdateStatus(int statusId)
    {
        Console.WriteLine($"Test PUT - modyfikacja status o ID {statusId}...");

        try
        {
            var updatedTaskData = new
            {
                Name = "Zaktualizowany Status",
            };

            var request = CreateRequest($"{baseUrl}/api/statuses/{statusId}", "PUT");
            var json = JsonSerializer.Serialize(updatedTaskData);

            await WriteRequestBodyAsync(request, json);
            var response = await GetResponseAsync(request);

            Console.WriteLine("Status zaktualizowany pomyślnie");
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

    private async Task TestDeleteStatus(int statusId)
    {
        Console.WriteLine($"Test DELETE - usuwanie status o ID {statusId}...");

        try
        {
            var request = CreateRequest($"{baseUrl}/api/statuses/{statusId}", "DELETE");
            var response = await GetResponseAsync(request);

            Console.WriteLine("Status usunięty pomyślnie");
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
                        Console.WriteLine("Status nie został znaleziony (404 Not Found)");
                        break;
                    case HttpStatusCode.Forbidden:
                        Console.WriteLine("Brak uprawnień do usunięcia status (403 Forbidden)");
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