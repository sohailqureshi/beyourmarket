using BeYourMarket.Core.Web;
using BeYourMarket.Model.Enum;
using BeYourMarket.Model.Models;
using BeYourMarket.Service;
using BeYourMarket.Service.Models;
using BeYourMarket.Web.Areas.Admin.Models;
using BeYourMarket.Web.Extensions;
using BeYourMarket.Web.Models;
using BeYourMarket.Web.Models.Grids;
using BeYourMarket.Web.Utilities;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Postal;
using Repository.Pattern.UnitOfWork;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BeYourMarket.Web.Areas.Admin.Controllers
{
  [Authorize(Roles = "Administrator")]
  public class ContentPageController : Controller
  {
    #region Fields
    private ApplicationSignInManager _signInManager;
    private ApplicationUserManager _userManager;
    private ApplicationRoleManager _roleManager;

    private readonly ISettingService _settingService;
    private readonly ISettingDictionaryService _settingDictionaryService;

    private readonly ICategoryService _categoryService;
    private readonly IListingService _listingService;

    private readonly ICustomFieldService _customFieldService;
    private readonly ICustomFieldCategoryService _customFieldCategoryService;

    private readonly IContentPageService _contentPageService;
    private readonly IContentPageRoleService _contentPageRoleService;

    private readonly IOrderService _orderService;

    private readonly IEmailTemplateService _emailTemplateService;

    private readonly DataCacheService _dataCacheService;
    private readonly SqlDbService _sqlDbService;

    private readonly IUnitOfWorkAsync _unitOfWorkAsync;
    #endregion

    #region Properties
    public ApplicationSignInManager SignInManager
    {
      get
      {
        return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
      }
      private set
      {
        _signInManager = value;
      }
    }

    public ApplicationUserManager UserManager
    {
      get
      {
        return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
      }
      private set
      {
        _userManager = value;
      }
    }

    public ApplicationRoleManager RoleManager
    {
      get
      {
        return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
      }
      private set
      {
        _roleManager = value;
      }
    }
    #endregion

    #region Constructor
    public ContentPageController(
        IUnitOfWorkAsync unitOfWorkAsync,
        ISettingService settingService,
        ICategoryService categoryService,
        IListingService listingService,
        ICustomFieldService customFieldService,
        ICustomFieldCategoryService customFieldCategoryService,
        IContentPageService contentPageService,
        IContentPageRoleService contentPageRoleService,
        IOrderService orderService,
        ISettingDictionaryService settingDictionaryService,
        IEmailTemplateService emailTemplateService,
        DataCacheService dataCacheService,
        SqlDbService sqlDbService)
    {
      _settingService = settingService;
      _settingDictionaryService = settingDictionaryService;

      _categoryService = categoryService;
      _listingService = listingService;
      _customFieldService = customFieldService;
      _customFieldCategoryService = customFieldCategoryService;

      _orderService = orderService;

      _emailTemplateService = emailTemplateService;
      _contentPageService = contentPageService;
      _contentPageRoleService = contentPageRoleService;
      _unitOfWorkAsync = unitOfWorkAsync;
      _dataCacheService = dataCacheService;
      _sqlDbService = sqlDbService;
    }
    #endregion

    #region Methods
    public ActionResult ContentPages()
    {
      var grid = new ContentPagesGrid(_contentPageService.Queryable().OrderByDescending(x => x.Created));

      var model = new ContentPageModel()
      {
        Grid = grid
      };

      return View(model);
    }

    public async Task<ActionResult> ContentPageUpdate(int? id)
    {
      var userId = User.Identity.GetUserId();
      var user = await UserManager.FindByIdAsync(userId);
      ViewBag.Roles = RoleManager.Roles.OrderBy(ob => ob.Name).ToList();

      var model = new ContentPage();

      var modelPageRoles = await _contentPageRoleService.Query().SelectAsync();
      model.ContentPageRoles = modelPageRoles.ToList();

      if (!id.HasValue || id == 0)
      {
        model.Author = user.FullName;
      }
      else
      {
        model = await _contentPageService.FindAsync(id);
        var contentPageRoles = await _contentPageRoleService.Query(x => x.ContentPageID == id.Value).SelectAsync();
        model.ContentPageRoles = contentPageRoles.ToList();
      }

      return View(model);
    }

    [HttpPost]
    [ValidateInput(false)]
    public async Task<ActionResult> ContentPageUpdate(ContentPage contentPage, string[] selectedRoles = null)
    {
      var userId = User.Identity.GetUserId();
      ViewBag.Roles = RoleManager.Roles.OrderBy(ob => ob.Name).ToList();

      if (contentPage.ID == 0)
      {
        contentPage.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added;
        contentPage.Created = DateTime.Now;
        contentPage.LastUpdated = DateTime.Now;

        var query = await _contentPageService.Query(x => x.Slug.ToLower() == contentPage.Slug.ToLower()).SelectAsync();
        if (query.Any())
        {
          TempData[TempDataKeys.UserMessageAlertState] = "bg-danger";
          TempData[TempDataKeys.UserMessage] = string.Format("[[[Slug {0} already exists]]]", contentPage.Slug);

          return View(contentPage);
        }

        if (_contentPageService.Queryable().Any())
        {
          contentPage.Ordering = _contentPageService.Queryable().Max(x => x.Ordering);
        }

        _contentPageService.Insert(contentPage);
      }
      else
      {
        var contentPageExisting = await _contentPageService.FindAsync(contentPage.ID);

        contentPageExisting.Title = contentPage.Title;
        contentPageExisting.Description = contentPage.Description;
        contentPageExisting.Slug = contentPage.Slug;
        contentPageExisting.Html = contentPage.Html;
        contentPageExisting.UserID = userId;
        contentPageExisting.Author = contentPage.Author;
        contentPageExisting.Published = contentPage.Published;

        contentPageExisting.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;
        contentPageExisting.LastUpdated = DateTime.Now;

        _contentPageService.Update(contentPageExisting);

        // Delete existing content page roles
        var contentPageRoles = _contentPageRoleService.Queryable().Where(x => x.ContentPageID == contentPageExisting.ID).Select(x => x.ID).ToList();
        if (contentPageRoles.Any())
        {
          foreach (var contentPageRole in contentPageRoles)
          {
            await _contentPageRoleService.DeleteAsync(contentPageRole);
          }
        }
      }

      if (selectedRoles != null)
      {
        for (int roleCount = 0; roleCount < selectedRoles.Length; roleCount++)
        {
          var contentPageRole = new ContentPageRole()
          {
            ContentPageID = contentPage.ID,
            AspNetRoleID = selectedRoles[roleCount],
            ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added
          };

          _contentPageRoleService.Insert(contentPageRole);
        }
      }

      await _unitOfWorkAsync.SaveChangesAsync();

      _dataCacheService.RemoveCachedItem(CacheKeys.ContentPages);

      return RedirectToAction("ContentPages");
    }
    #endregion
  }
}