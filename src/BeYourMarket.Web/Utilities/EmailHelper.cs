using BeYourMarket.Model.Models;
using BeYourMarket.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using BeYourMarket.Service;
using BeYourMarket.Model.Enum;
using Postal;
using System.Net.Mail;
using System.Net;
using System.IO;

namespace BeYourMarket.Web.Utilities
{
  public static class EmailHelper
  {
    public static IEmailService EmailService = Postal.Email.CreateEmailService();

    public static void SendEmail(Email email, bool preMailer = true)
    {
      var httpContext = Elmah.ErrorSignal.FromCurrentContext();

      Task.Factory.StartNew(() =>
      {
        try
        {
          if (CacheHelper.Settings.SmtpDeliveryMethod == Enum_SmtpDeliveryMethod.SpecifiedPickupDirectory)
          {
            //skip email if there is no directory settings or physical directory and Pickup Directory Location method has been selected
            if (string.IsNullOrEmpty(CacheHelper.Settings.PickupDirectoryLocation)) { return; }
            if(!Directory.Exists(CacheHelper.Settings.PickupDirectoryLocation)) { return; }
          }
          else
          {
            //skip email if there is no host settings and network or IIS delivery method has been selected
            if (string.IsNullOrEmpty(CacheHelper.Settings.SmtpHost) && string.IsNullOrEmpty(CacheHelper.Settings.SmtpPassword))
              return;
          }

          var message = EmailService.CreateMailMessage(email);

          using (var smtpClient = new SmtpClient())
          {
            if (CacheHelper.Settings.SmtpDeliveryMethod == Enum_SmtpDeliveryMethod.SpecifiedPickupDirectory)
            {
              smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
              smtpClient.PickupDirectoryLocation = CacheHelper.Settings.PickupDirectoryLocation;
              smtpClient.Host = "localhost";
            }
            else
            {
              smtpClient.UseDefaultCredentials = false;

              // set credential if there is one
              if (!string.IsNullOrEmpty(CacheHelper.Settings.SmtpUserName) && !string.IsNullOrEmpty(CacheHelper.Settings.SmtpPassword))
              {
                var credential = new NetworkCredential
                {
                  UserName = CacheHelper.Settings.SmtpUserName,
                  Password = CacheHelper.Settings.SmtpPassword
                };
                smtpClient.Credentials = credential;
              }
              smtpClient.Host = CacheHelper.Settings.SmtpHost;
              smtpClient.EnableSsl = CacheHelper.Settings.SmtpSSL;

              if (CacheHelper.Settings.SmtpPort.HasValue)
                smtpClient.Port = CacheHelper.Settings.SmtpPort.Value;
            }

            //moving CSS to inline style attributes, to gain maximum E-mail client compatibility.
            if (preMailer) { message.Body = PreMailer.Net.PreMailer.MoveCssInline(message.Body).Html; }

            smtpClient.Send(message);
          }
        }
        catch (Exception ex)
        {
          //http://stackoverflow.com/questions/7441062/how-to-use-elmah-to-manually-log-errors
          httpContext.Raise(ex);
        }
      });
    }
  }
}
