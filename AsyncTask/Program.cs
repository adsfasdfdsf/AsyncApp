using System;
using System.Threading.Tasks;
using System.Threading;
using AsyncTask.Library;

namespace AsyncTask;

public class Program 
{
    public static async Task Main(string[] args)
    {
        var fetcher = new Fetcher("https://jsonplaceholder.typicode.com/comments/");
        await fetcher.Fetch();
        Console.WriteLine(string.Join('\n', fetcher.GetTopWords()));
    }
}