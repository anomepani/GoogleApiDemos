using Google.Apis.Analytics.v3;
using Google.Apis.Analytics.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;

namespace GoogleApiDemos.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult GetGoogleAnalyticsData(string fromDate = "2017-01-01", string toDate = "2017-06-06", string dimensions = "ga:pagePath,ga:pageTitle", string metrics = "ga:pageviews", string sortBy = "-ga:pageviews", int maxResult = 20)
        {
            try
            {
                string[] scopes = new string[] { AnalyticsService.Scope.Analytics }; // view and manage your Google Analytics data

                var keyFilePath = Server.MapPath(ConfigurationManager.AppSettings["privateKeyPath"]); // @"c:\Private\DemoAPI-728b73608a30.p12";    // Downloaded from https://console.developers.google.com
                var gaTableId = ConfigurationManager.AppSettings["AccountTableId"];

                var serviceAccountEmail = ConfigurationManager.AppSettings["ServiceAccountEmail"];// found 

                //loading the Key file
                var certificate = new X509Certificate2(keyFilePath, "notasecret", X509KeyStorageFlags.Exportable);
                var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccountEmail)
                {
                    Scopes = scopes
                }.FromCertificate(certificate));
                var gas = new AnalyticsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Google Analytics API Sample",
                });

                //var accountList = gas.Management.Accounts.List().Execute();
                //AnalyticsService gas = AuthenticateUser();
                //var gaId = ConfigurationManager.AppSettings["AccountTableId"];

                // Creating our query
                // metric: ga:visits, ga:pageviews, ga:users, ga:newUsers, ga:sessions
                DataResource.GaResource.GetRequest r = gas.Data.Ga.Get(gaTableId, fromDate, toDate, metrics);

                r.Sort = sortBy;
                r.Dimensions = dimensions;
                r.MaxResults = maxResult;
                //Execute and fetch the results based on requested query
                GaData d = r.Execute();
                ViewBag.isError = false;
                ViewBag.AnalyticsData = d.Rows;
            }
            catch (Exception ex)
            {
                ViewBag.isError = true;
                ViewBag.StatusMsg = ex.Message;
            }
            return View();
        }
    }
}