namespace AsyncTask.Library;

public class Utils
{
    public static List<string> ParseTopWords(List<ResponseModel> jsonRespList)
    {
        var topWords = jsonRespList
            .SelectMany(c => c.body.Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '-', '\n', '\r', '\t' },
                StringSplitOptions.RemoveEmptyEntries))
            .Select(word => word.ToLower())
            .GroupBy(word => word)
            .Select(group => new { Word = group.Key, Count = group.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10).ToList();
        return topWords.Select(w => w.Word).ToList();
    }
}