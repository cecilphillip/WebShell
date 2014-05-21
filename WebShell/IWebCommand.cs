namespace WebShell
{
    public interface IWebCommand
    {
        string Process(string[] args);
        bool ReturnHtml { get; }
    }
}
