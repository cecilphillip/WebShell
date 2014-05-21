namespace WebShell.Commands
{
    [WebCommand("load-commands", "Reloads command assemblies")]
    public class ReloadCommands : IWebCommand
    {
        public bool ReturnHtml
        {
            get { return false; }
        }

        public string Process(string[] args)
        {
            CommandEngine.Current.LoadCommands();
            return "Commands loaded, type 'help' to see them";
        }
    }
}
