using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Text;
using System.Web.Http.Dispatcher;

namespace ApiDoc
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

        }

        /// <summary>
        /// 全局异常处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Error(object sender, EventArgs e)
        {

            HttpRequest request = Context.Request;
            StringBuilder msg = new StringBuilder()
            .Append(Environment.NewLine)
            .Append(DateTime.Now.ToShortDateString())
            .Append("UserAgent:   ").Append(request.UserAgent).Append(Environment.NewLine)
            .Append("AbsoluteUri: ").Append(request.Url.AbsoluteUri).Append(Environment.NewLine)
            .Append("UrlReferrer:   ").Append(request.UrlReferrer).Append(Environment.NewLine)
            .Append("Exception:   ").Append(Server.GetLastError()).Append(Environment.NewLine)
            .Append("-------------------------------------------------------------------------------").Append(Environment.NewLine);
            //Logger.Default.Error(msg.ToString());


            bool iserrorview = true;
            if (iserrorview) //是否开启错误视图
            {
                var lastError = Server.GetLastError();
                if (lastError != null)
                {
                    var httpError = lastError as HttpException;
                    if (httpError != null)
                    {
                        //400与404错误不记录日志，并都以自定义404页面响应
                        var httpCode = httpError.GetHttpCode();
                        if (httpCode == 400 || httpCode == 404)
                        {
                            Response.StatusCode = 404;
                            Server.ClearError();
                            Response.Redirect("/Error_404.html", true);
                            return;
                        }
                    }
                    //对于路径错误不记录日志，并都以自定义404页面响应
                    if (lastError.TargetSite.ReflectedType == typeof(System.IO.Path))
                    {
                        Response.StatusCode = 404;
                        Server.ClearError();
                        Response.Redirect("/Error_404.html");
                        return;
                    }
                    Response.StatusCode = 500;
                    Server.ClearError();
                    var httprequestwrapper = new HttpRequestWrapper(request);
                    if (!httprequestwrapper.IsAjaxRequest())
                    {
                        Response.Redirect("/Error_500.html", true);
                    }
                }
            }
        }
    }
}