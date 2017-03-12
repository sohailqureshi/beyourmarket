using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace BeYourMarket.Web.Controllers
{
  public class BaseController : Controller
  {
    public ApplicationUserManager UserManager
    {
      get
      {
        return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
      }
    }

    public ApplicationRoleManager RoleManager
    {
      get
      {
        return HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
      }
    }

    public ApplicationSignInManager SignInManager
    {
      get
      {
        return HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
      }
    }

    public string RenderRazorViewToString(string viewName, object model)
    {
      ViewData.Model = model;
      using (var sw = new StringWriter())
      {
        var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
        if (viewResult != null)
        {
          var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);

          viewResult.View.Render(viewContext, sw);
          return sw.GetStringBuilder().ToString();
        }
        return string.Empty;
      }
    }

    public static string SeoName(string name)
    {
      return Regex.Replace(name.ToLower().Replace(@"'", String.Empty), @"[^\w]+", "-");
    }

    public void AddErrors(IdentityResult result)
    {
      foreach (var error in result.Errors)
      {
        ModelState.AddModelError("", error);
      }
    }

    // This method helps to get the error information from the MVC "ModelState".
    // We can not directly send the ModelState to the client in Json. The "ModelState"
    // object has some circular reference that prevents it to be serialized to Json.
    public Dictionary<string, object> GetErrorsFromModelState()
    {
      var errors = new Dictionary<string, object>();
      foreach (var key in ModelState.Keys)
      {
        // Only send the errors to the client.
        if (ModelState[key].Errors.Count > 0)
        {
          errors[key] = ModelState[key].Errors;
        }
      }

      return errors;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && UserManager != null)
      {
        UserManager.Dispose();
      }

      if (disposing && RoleManager != null)
      {
        RoleManager.Dispose();
      }

      if (disposing && SignInManager != null)
      {
        SignInManager.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}