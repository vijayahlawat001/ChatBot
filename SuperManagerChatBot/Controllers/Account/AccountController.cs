using Newtonsoft.Json;
using SuperManagerChatBot.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace SuperManagerChatBot.Controllers.Account
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ForgetPassword()
        {
            return View();
        }
        public ActionResult Signout()
        {
            HttpCookie cookie = new HttpCookie("SuperManagerAuth", "");
            cookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie);

            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account", null);
        }
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult ResetPassword(string Email = "", string Validity = "")
        {
            TempData["Email"] = Email;
            TempData["Validity"] = Validity;
            DateTime OTPValidity = DateTime.Now;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "ValidateChangePasswordValidity";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Email", Email);
            cmd.Parameters.AddWithValue("@OTP", Validity);
            cmd.Parameters.AddWithValue("@OTPValidity", OTPValidity);
            DBAccess db = new DBAccess();
            DataTable dt = new DataTable();
            dt = db.GetRcdSetByCmdTrans(cmd);
            if (dt.Rows.Count > 0)
            {
                return View();
            }
            else
            {
                return View();
                //return View("ValidityExpired");
            }
        }
        [HttpPost]
        public ActionResult AuthenticateUser(string UserName, string Password)
        {
            try
            {
                if (Membership.ValidateUser(UserName, Comman.Encrypt(Password)))
                {
                    var user = Membership.GetUser(UserName, false);
                    if (user != null)
                    {
                        UserModel userModel = new UserModel()
                        {
                            EmailID = user.Email,
                            UserName = user.UserName,
                            RoleName = "User"
                        };
                        string userData = JsonConvert.SerializeObject(userModel);
                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, UserName, DateTime.Now, DateTime.Now.AddMinutes(15), false, userData);
                        string enTicket = FormsAuthentication.Encrypt(authTicket);
                        HttpCookie faCookie = new HttpCookie("SuperManagerAuth", enTicket);
                        Response.Cookies.Add(faCookie);
                        //return RedirectToAction("Index", new RouteValueDictionary( new { controller = "Home",Email = UserName }));
                        return Json(new { success = true, responseText = "1" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = true, responseText = "0" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { success = true, responseText = "0" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult RegisterUser(string FullName, string Country, string Email, string Password)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "RegisterUser";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FullName", FullName);
                cmd.Parameters.AddWithValue("@Country", Country);
                cmd.Parameters.AddWithValue("@Email", Email);
                cmd.Parameters.AddWithValue("@Password", Comman.Encrypt(Password));
                DBAccess db = new DBAccess();
                DataTable dt = new DataTable();
                dt = db.GetRcdSetByCmdTrans(cmd);
                if (dt.Rows.Count > 0)
                {
                    return Json(new { success = true, responseText = Newtonsoft.Json.JsonConvert.SerializeObject(dt.Rows[0]["Result"].ToString()) }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, responseText = Newtonsoft.Json.JsonConvert.SerializeObject("Failed to register. Please try later.") }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = true, responseText = Newtonsoft.Json.JsonConvert.SerializeObject(ex.Message) }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult ForgetPassword(string Email)
        {
            try
            {
                string OTP = Comman.Encrypt(GenerateOTP());

                string ResetPasswordLink = Request.Url.AbsoluteUri.Replace("ForgetPassword", "ResetPassword");
                ResetPasswordLink = ResetPasswordLink + "?Email=" + Email + "&Validity=" + OTP;

                DateTime OTPValidity = DateTime.Now.AddMinutes(15);
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "ForgetPassword";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", Email);
                cmd.Parameters.AddWithValue("@OTP", OTP);
                cmd.Parameters.AddWithValue("@OTPValidity", OTPValidity);
                DBAccess db = new DBAccess();
                DataTable dt = new DataTable();
                dt = db.GetRcdSetByCmdTrans(cmd);
                if (dt.Rows.Count > 0)
                {
                    string MailBody = @"<div style='margin: 0px; padding: 0px; color: rgb(41, 52, 58); font-family: ""Arial MT"", Arial, Helvetica, sans-serif; font-size: 15px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: left; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(235, 250, 251); text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial; line-height: 30px;'><p>Hi,<br>We have received a request to reset the password for your SuperManager ID.<br>If you did not submit the request, please disregard this email and continue to use your current password. To reset your password, please click the following link <strong>within 15 Minutes:&nbsp;</strong><strong><br><a href= " + ResetPasswordLink + @" rel=""noopener noreferrer"" target=""_blank"">Reset Password</a><br><br>Team SuperManager</strong><br><br></p></div>";
                    Comman.SendEmail(Email, "", "", "SuperManager - Password Reset", MailBody);
                    return Json(new { success = true, responseText = Newtonsoft.Json.JsonConvert.SerializeObject(dt.Rows[0]["Result"].ToString()) }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, responseText = Newtonsoft.Json.JsonConvert.SerializeObject("Failed to register. Please try later.") }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = true, responseText = Newtonsoft.Json.JsonConvert.SerializeObject(ex.Message) }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult ChangePassword(string NewPassword)
        {
            try
            {
                string Email = TempData["Email"].ToString();
                string Validity = TempData["Validity"].ToString();

                DateTime OTPValidity = DateTime.Now;
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "ChangePassword";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", Email);
                cmd.Parameters.AddWithValue("@OTP", Validity);
                cmd.Parameters.AddWithValue("@OTPValidity", OTPValidity);
                cmd.Parameters.AddWithValue("@NewPassword", Comman.Encrypt(NewPassword));
                DBAccess db = new DBAccess();
                DataTable dt = new DataTable();
                dt = db.GetRcdSetByCmdTrans(cmd);
                if (dt.Rows.Count > 0)
                {
                    return Json(new { success = true, responseText = Newtonsoft.Json.JsonConvert.SerializeObject(dt.Rows[0]["Result"].ToString()) }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, responseText = Newtonsoft.Json.JsonConvert.SerializeObject("Failed to register. Please try later.") }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = true, responseText = Newtonsoft.Json.JsonConvert.SerializeObject(ex.Message) }, JsonRequestBehavior.AllowGet);
            }
        }
        private string GenerateOTP()
        {
            string alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string small_alphabets = "abcdefghijklmnopqrstuvwxyz";
            string numbers = "1234567890";

            string characters = numbers;
            characters += alphabets + small_alphabets + numbers;
            string otp = string.Empty;
            for (int i = 0; i < 10; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                } while (otp.IndexOf(character) != -1);
                otp += character;
            }
            return otp;
        }
    }
}