[assembly: WebActivator.PreApplicationStartMethod(typeof($rootnamespace$.App_Start.WebShellStart), "Start")] 
namespace $rootnamespace$.App_Start {
	using WebShell;
    public static class WebShellStart {
        public static void Start() {
			WebShell.Init();
			
			//TODO: replace with your own auth code
			//WebShell.Settings.Authorize = (request) =>
			//	{
			//		return request.IsLocal;
			//	};
        }
    }
}