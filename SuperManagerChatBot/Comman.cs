using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SuperManagerChatBot
{
    public class Comman
    {
        private static string Decrypt(string cipherText)
        {
            string EncryptionKey = "jhvfkjhgshdjkghs";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }

            return cipherText;
        }
        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "jhvfkjhgshdjkghs";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            int I = clearText.Length;
            return clearText.Replace(' ','+');

        }
        public static void SendEmail(string EmailTo, string CC, string BCC, string Subject, string Body)
        {
            try
            {
                string FromEmail = System.Configuration.ConfigurationManager.AppSettings["FromEmail"].ToString();
                string EmailPassword = System.Configuration.ConfigurationManager.AppSettings["EmailPassword"].ToString();
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress(FromEmail);
                if (EmailTo != "")
                {
                    foreach (var address in EmailTo.Split(','))
                    {
                        if (address != "")
                        {
                            message.To.Add(new MailAddress(address.Trim(), ""));
                        }
                    }
                }
                if (CC != "")
                {
                    foreach (var address in CC.Split(','))
                    {
                        if (address != "")
                        {
                            message.CC.Add(new MailAddress(address.Trim(), ""));
                        }
                    }
                }
                if (BCC != "")
                {
                    foreach (var address in BCC.Split(','))
                    {
                        if (address != "")
                        {
                            message.CC.Add(new MailAddress(address.Trim(), ""));
                        }
                    }
                }
                message.Subject = Subject;
                message.IsBodyHtml = true;
                message.Body = Body;
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(FromEmail, EmailPassword);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}