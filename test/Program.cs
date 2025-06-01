using System.Net;
using System.Text;
using System.Text.Json;

namespace TaskFlow.Tests;
class Program
{
    private static readonly string baseUrl = "http://localhost:5200";
    private static readonly string username = "admin";
    private static readonly string token = "d1260f40-c02c-46f8-8488-ea6947cc7967";

    static async Task Main(string[] args)
    {
        ProjectTest projectTest = new ProjectTest(baseUrl, username, token);
        TaskTest taskTest = new TaskTest(baseUrl, username, token);
        StatusTest statusTest = new StatusTest(baseUrl, username, token);
        CommentTest commentTest = new CommentTest(baseUrl, username, token);
        try
        {
            await projectTest.TestProjectApi();
            await taskTest.TestTaskApi();
            await statusTest.TestTaskApi();
            await commentTest.TestCommentApi();
            Console.WriteLine("All tests completed successfully.");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
