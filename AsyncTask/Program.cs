using System;
using System.Threading.Tasks;
using System.Threading;
using AsyncTask.Library;

namespace AsyncTask;

public class Program 
{
    public static async Task Main(string[] args)
    {
        using SemaphoreSlim semaphore = new SemaphoreSlim(3);
        using var cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        Console.CancelKeyPress += (s, e) =>
        {
            Console.WriteLine("Canceling...");
            cts.Cancel();
        };
        
        var tasks = new List<Task<String>>();
        var resList = new List<String>();
        int totalCompleted = 0;
    
        var progressTracker = new Progress<bool>(_ =>
        {
            int newValue = Interlocked.Increment(ref totalCompleted);
            Console.WriteLine($"{newValue}/500");
        });

        try
        {
            for (int i = 1; i <= 500; ++i)
            {
                tasks.Add(MakeRequest(i, semaphore, token, 3, progressTracker));
            }

            resList = (await Task.WhenAll(tasks)).ToList();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cancelled");
            return;
        }

        var jsonRespList = ResponseModel.StringToResponseModel(resList);
        
        var topWords = Utils.ParseTopWords(jsonRespList);

        for (int i = 1; i <= 10; ++i)
        {
            Console.WriteLine($"{i}) {topWords[i - 1]}");
        }
    }

    public static async Task<string> MakeRequest(int id, SemaphoreSlim semaphore, 
        CancellationToken token, int maxRetries, IProgress<bool> progress)
    {
        for (int i = 0; i < maxRetries; ++i)
        {
            await semaphore.WaitAsync();
            string response;
            token.ThrowIfCancellationRequested();
            try
            {
                using HttpClient client = new();
                string url = $"https://jsonplaceholder.typicode.com/comments/{id}";
                response = await client.GetStringAsync(url);
                progress.Report(true);
                semaphore.Release();
                return response;
            }
            catch (Exception)
            {
                if (i == maxRetries)
                {
                    semaphore.Release();
                    throw;
                }
            }
        }
        return "";
    }
    
    
}