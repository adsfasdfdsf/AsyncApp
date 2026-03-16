using System.Text.Json;

namespace AsyncTask;

public class ResponseModel
{
    public int postId  { get; set; }
    public int id   { get; set; }
    public string name  { get; set; }
    public string email  { get; set; }
    public string body   { get; set; }

    public static List<ResponseModel> StringToResponseModel(List<string> resList)
    {
        var jsonRespList = new List<ResponseModel>();
        foreach (var r in resList)
        {
            var item = JsonSerializer.Deserialize<ResponseModel>(r);
            if (item == null)
            {
                continue;
            }
            jsonRespList.Add(item);
        }
        return jsonRespList;
    }
}