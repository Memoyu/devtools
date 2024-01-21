namespace DevTools.Models;

public class LogLoginResult
{
    public string State { get; set; }

    public string Msg { get; set; }

    public static LogLoginResult Error()
    {
        return new LogLoginResult { State = "error", Msg = "请求错误" };
    }
}
