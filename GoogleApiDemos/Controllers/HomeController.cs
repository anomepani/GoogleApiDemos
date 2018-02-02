using Google.Apis.Analytics.v3;
using Google.Apis.Analytics.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
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

                var gaTableId = ConfigurationManager.AppSettings["AccountTableId"];

                //Authenticate Get Google credential from json client secret
                //var credential = getUserCredentialFromJson();

                ////Authenticate and get credential from clientId and Client Secret
                var credential = getUserCredential();

                //Authenticate google api using service credential
                var serviceCredential = getServiceCredentialFromPrivateKey();
                var gas = new AnalyticsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential.Result,
                    //Below code is for service account
                    //HttpClientInitializer = serviceCredential,

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
        /// <summary>
        /// Authenticate google api using OAuth 2.0
        /// </summary>
        /// <returns></returns>
        public async Task<UserCredential> getUserCredentialFromJson()
        {
            UserCredential credential;
            string[] scopes = new string[] { AnalyticsService.Scope.Analytics }; // view and manage your Google Analytics data

            //Read client id and client secret from json file
            using (var stream = new FileStream(Server.MapPath(ConfigurationManager.AppSettings["ClientSecretJsonPath"]), FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user", CancellationToken.None, new FileDataStore("Auth.Api.Store"));
            }
            return credential;
        }
        /// <summary>
        /// Authenticate google api using Service account
        /// </summary> 
        /// <returns></returns>
        public ServiceCredential getServiceCredentialFromPrivateKey()
        {
            ServiceCredential credential;
            string[] scopes = new string[] { AnalyticsService.Scope.Analytics }; // view and manage your Google Analytics data

            var keyFilePath = Server.MapPath(ConfigurationManager.AppSettings["privateKeyPath"]); // @"c:\Private\DemoAPI-728b73608a30.p12";    // Downloaded from https://console.developers.google.com

            var serviceAccountEmail = ConfigurationManager.AppSettings["ServiceAccountEmail"];// found 

            //loading the Key file
            var certificate = new X509Certificate2(keyFilePath, "notasecret", X509KeyStorageFlags.Exportable);
            credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccountEmail)
            {
                Scopes = scopes
            }.FromCertificate(certificate));


            return credential;
        }

        public async Task<UserCredential> getUserCredential()
        {
            UserCredential credential;
            string[] scopes = new string[] { AnalyticsService.Scope.Analytics }; // view and manage your Google Analytics data

            //Read client id and client secret from json file

            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                       new ClientSecrets
                       {
                           ClientId = ConfigurationManager.AppSettings["ClientId"],
                           ClientSecret = ConfigurationManager.AppSettings["ClientSecret"]
                       }, scopes,
                "user", CancellationToken.None, new FileDataStore("Auth.Api.Store"));

            return credential;
        }
    }
}