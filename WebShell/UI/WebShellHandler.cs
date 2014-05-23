using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;
using System.Web.Script.Serialization;

namespace WebShell.UI
{
    public class WebShellHandler : IRouteHandler, IHttpHandler
    {
        internal static HtmlString RenderIncludes()
        {
            if (Settings.Authorize != null && !Settings.Authorize(HttpContext.Current.Request))
                return new HtmlString(""); //not authorized dont render

            const string FORMAT =
@"<link rel=""stylesheet"" type=""text/css"" href=""{0}webshell-style-css?v={1}"">
<script type=""text/javascript"">
    if (!window.jQuery) document.write(unescape(""%3Cscript src='{0}webshell-jquery-js' type='text/javascript'%3E%3C/script%3E""));
    if(!window.key) document.write(unescape(""%3Cscript src='{0}webshell-keymaster-js' type='text/javascript'%3E%3C/script%3E""));
</script>
<script type=""text/javascript"" src=""{0}webshell-script-js?v={1}""></script>
<script type=""text/javascript"">var webshell = new NetBash(jQuery, window, {{ welcomeMessage: '{2}', version: '{3}', isHidden: {4}, routeBasePath: '{5}' }});</script>";

            var result = "";
            result = string.Format(FORMAT, 
                                   EnsureTrailingSlash(VirtualPathUtility.ToAbsolute(Settings.RouteBasePath)), 
                                   Settings.Hash, Settings.WelcomeMessage, 
                                   Settings.Version, 
                                   Settings.HiddenByDefault.ToString().ToLower(), 
                                   Settings.RouteBasePath.Replace("~", ""));

            return new HtmlString(result);
        }

        internal static void RegisterRoutes()
        {
            var urls = new[] 
            {  
                "webshell",
                "webshell-jquery-js",
                "webshell-keymaster-js",
                "webshell-style-css",
                "webshell-script-js"
            };

            var routes = RouteTable.Routes;
            var handler = new WebShellHandler();
            var prefix = EnsureTrailingSlash((Settings.RouteBasePath ?? "").Replace("~/", ""));

            using (routes.GetWriteLock())
            {
                foreach (var url in urls)
                {
                    var route = new Route(prefix + url, handler)
                    {
                        // we have to specify these, so no MVC route helpers will match, e.g. @Html.ActionLink("Home", "Index", "Home")
                        Defaults = new RouteValueDictionary(new { controller = "WebShellHandler", action = "ProcessRequest" })
                    };

                    // put our routes at the beginning, like a boss
                    routes.Insert(0, route);
                }
            }
        }

        private static string EnsureTrailingSlash(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return Regex.Replace(input, "/+$", "") + "/";
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return this; // elegant? I THINK SO.
        }

        /// <summary>
        /// Try to keep everything static so we can easily be reused.
        /// </summary>
        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Returns either includes' css/javascript or results' html.
        /// </summary>
        public void ProcessRequest(HttpContext context)
        {
            string output;
            string path = context.Request.AppRelativeCurrentExecutionFilePath;

            switch (Path.GetFileNameWithoutExtension(path).ToLower())
            {
                case "webshell-jquery-js":
                case "webshell-script-js":
                case "webshell-style-css":
                case "webshell-keymaster-js":
                    output = Includes(context, path);
                    break;

                case "webshell":
                    output = RenderCommand(context);
                    break;

                default:
                    output = NotFound(context);
                    break;
            }

            context.Response.Write(output);
        }

        private static string RenderCommand(HttpContext context)
        {
            if (Settings.Authorize != null && !Settings.Authorize(HttpContext.Current.Request))
                throw new UnauthorizedAccessException();

            string commandResponse;
            var success = true;
            var isHtml = true;

            try
            {
                var result = CommandEngine.Current.Process(context.Request.Params["Command"]);
                if (result.IsHtml)
                {
                    //on your way
                    commandResponse = result.Result;
                }
                else
                {
                    //encode it
                    commandResponse = HttpUtility.HtmlEncode(result.Result);
                }
                isHtml = result.IsHtml;
            }
            catch (Exception ex)
            {
                success = false;
                commandResponse = ex.Message;
            }

            var response = new { Success = success, IsHtml = isHtml, Content = commandResponse };

            context.Response.ContentType = "application/json";

            var sb = new StringBuilder();
            var serializer = new JavaScriptSerializer();
            serializer.Serialize(response, sb);

            return sb.ToString();
        }

        /// <summary>
        /// Handles rendering static content files.
        /// </summary>
        private static string Includes(HttpContext context, string path)
        {
            var response = context.Response;
            var extension = "." + path.Split('-').LastOrDefault();

            switch (extension)
            {
                case ".js":
                    response.ContentType = "application/javascript";
                    break;
                case ".css":
                    response.ContentType = "text/css";
                    break;
                case ".gif":
                    response.ContentType = "image/gif";
                    break;
                default:
                    return NotFound(context);
            }

            var cache = response.Cache;
            cache.SetCacheability(HttpCacheability.Public);
            cache.SetExpires(DateTime.Now.AddDays(7));
            cache.SetValidUntilExpires(true);

            var embeddedFile = Path.GetFileName(path).Replace("webshell-", "") + extension;
            return GetResource(embeddedFile);
        }

        private static string GetResource(string filename)
        {
            string result;

            if (!ResourceCache.TryGetValue(filename, out result))
            {
                using (var stream = typeof(WebShellHandler).Assembly.GetManifestResourceStream("WebShell.UI." + filename))
                using (var reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }

                ResourceCache[filename] = result;
            }

            return result;
        }

        /// <summary>
        /// Embedded resource contents keyed by filename.
        /// </summary>
        private static readonly Dictionary<string, string> ResourceCache = new Dictionary<string, string>();

        /// <summary>
        /// Helper method that sets a proper 404 response code.
        /// </summary>
        private static string NotFound(HttpContext context, string contentType = "text/plain", string message = null)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = contentType;

            return message;
        }
    }
}
