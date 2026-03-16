namespace AsyncTask;
using AsyncTask.Library;

public class Fetcher
{
    private string _url;
    private List<string> _responses = new();
    private Progress<bool> _progressTracker;
    private int _totalCompleted = 0;
    
    public Fetcher(string url)
    {
        _url = url; 
        _progressTracker = new Progress<bool>(_ =>
        {
            int newValue = Interlocked.Increment(ref _totalCompleted);
            Console.WriteLine($"{newValue}/500");
        });
    }

    public async Task Fetch()
    {
        using SemaphoreSlim semaphore = new SemaphoreSlim(3);
        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (s, e) =>
        {
            Console.WriteLine("Canceling...");
            cts.Cancel();
        };
        var tasks = new List<Task<String>>();
        try
        {
            for (int i = 1; i <= 500; ++i)
            {
                tasks.Add(MakeRequest(i, semaphore, cts.Token, 3, _progressTracker));
            }
            _responses = (await Task.WhenAll(tasks)).ToList();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cancelled");
        }
    }

    public async Task<string> MakeRequest(int id, SemaphoreSlim semaphore, 
        CancellationToken token, int maxRetries, IProgress<bool> progress)
    {
        await semaphore.WaitAsync();
        for (int i = 0; i < maxRetries; ++i)
        {
            string response;
            token.ThrowIfCancellationRequested();
            try
            {
                using HttpClient client = new();
                
                response = await client.GetStringAsync(_url + id);
                progress.Report(true);
                semaphore.Release();
                return response;
            }
            catch (Exception)
            {
                if (i == maxRetries)
                {
                    semaphore.Release();
                }
            }
        }
        return "";
    }
    
    public List<string> GetTopWords()
    {
        var jsonRespList = ResponseModel.StringToResponseModel(_responses);
        var topWords = Utils.ParseTopWords(jsonRespList);
        return topWords;
    }
}