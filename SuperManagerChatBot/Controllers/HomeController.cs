using CustomAuthenticationMVC.CustomAuthentication;
using Newtonsoft.Json;
using SuperManagerChatBot.Models;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SuperManagerChatBot.Controllers
{
    [CustomAuthorize(Roles = "User")]

    public class HomeController : Controller
    {
        public ActionResult Index(string Email)
        {
            return View("Index");
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        [HttpPost]
        public ActionResult GetDetail(string CurrentID)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "GetTopicByParent";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CurrentID", CurrentID);
                DataTable dt = new DataTable();
                dt = (new DBAccess()).GetRcdSetByCmdTrans(cmd);
                if (dt.Rows.Count > 0)
                {
                    return Json(new { success = true, responseText = Newtonsoft.Json.JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseText = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public async Task<ActionResult> GenerateCertificate()
        {
            try
            {
                string UserEmail = string.Empty;
                string UserName = string.Empty;
                HttpCookie authCookie = Request.Cookies["SuperManagerAuth"];
                if (authCookie != null)
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    var serializeModel = JsonConvert.DeserializeObject<UserModel>(authTicket.UserData);
                    UserName = serializeModel.UserName;
                    UserEmail = serializeModel.EmailID;
                }
                string WebAPIPath = ConfigurationManager.AppSettings["WebAPIPath"].ToString();
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(WebAPIPath);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Vijay", "Vijay@123");
                var stringContent = new StringContent("", UnicodeEncoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("/api/GenerateCertificate?Name=" + UserName + "&Model=Hi&Email=" + UserEmail, stringContent);
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, responseText = Newtonsoft.Json.JsonConvert.SerializeObject(response) }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseText = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpPost]
        public ActionResult GetQuestionList()
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "SELECT * FROM TOPICDETAIL";
                cmd.CommandType = CommandType.Text;
                DataTable dt = new DataTable();
                dt = (new DBAccess()).GetRcdSetByCmd(cmd);
                if (dt.Rows.Count > 0)
                {
                    return Json(new { success = true, responseText = Newtonsoft.Json.JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseText = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}