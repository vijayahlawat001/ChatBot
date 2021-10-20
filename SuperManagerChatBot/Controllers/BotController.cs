using CustomAuthenticationMVC.CustomAuthentication;
using SuperManagerChatBot.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SuperManagerChatBot.Controllers
{
    [CustomAuthorize(Roles = "User")]
    public class BotController : Controller
    {
        // GET: Treeview
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult designBot()
        {
            List<TopicDetail> topicDetails = new List<TopicDetail>();
            using (ChatbotEntities dc = new ChatbotEntities())
            {
                topicDetails = dc.TopicDetails.OrderBy(a => a.ParentTopicID).ToList();
            }
            return View(topicDetails);
        }
        [HttpPost]
        public ActionResult SaveBotDetail(string TopicID, string ResponseType, string BotResponse, string option)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "SaveBotDetail";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TopicID", TopicID);
                cmd.Parameters.AddWithValue("@ResponseType", ResponseType);
                cmd.Parameters.AddWithValue("@BotResponse", BotResponse);
                cmd.Parameters.AddWithValue("@option", option);
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
    }
}