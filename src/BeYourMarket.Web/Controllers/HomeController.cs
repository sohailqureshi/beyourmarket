using BeYourMarket.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BeYourMarket.Web.Extensions;
using BeYourMarket.Web.Utilities;
using System.Threading.Tasks;
using BeYourMarket.Model.Models;
using BeYourMarket.Web.Models;
using PagedList;
using BeYourMarket.Web.Models.Grids;
using i18n;
using i18n.Helpers;
using System.Web.Security;
using Microsoft.AspNet.Identity;

namespace BeYourMarket.Web.Controllers
{
  public class HomeController : BaseController
  {
    #region Fields
    private readonly ICategoryService _categoryService;
    private readonly IListingService _listingService;
    private readonly IContentPageService _contentPageService;
    private readonly IContentPageRoleService _contentPageRoleService;
    private readonly ICustomFieldCategoryService _customFieldCategoryService;
    private readonly IMetaFieldService _metaFieldService;
    #endregion

    #region Constructor
    public HomeController(
        ICategoryService categoryService,
        IListingService listingService,
        IContentPageService contentPageService,
        IContentPageRoleService contentPageRoleService,
        ICustomFieldCategoryService customFieldCategoryService,
        IMetaFieldService metaFieldService)
    {
      _categoryService = categoryService;
      _listingService = listingService;
      _contentPageService = contentPageService;
      _contentPageRoleService = contentPageRoleService;
      _customFieldCategoryService = customFieldCategoryService;
      _metaFieldService = metaFieldService;
    }
    #endregion

    #region Methods
    public async Task<ActionResult> Index(string id)
    {
      if (!string.IsNullOrEmpty(id))
        return RedirectToAction("ContentPage", "Home", new { id = id.ToLowerInvariant() });

      var model = new SearchListingModel();
      model.ListingTypeID = CacheHelper.ListingTypes.Select(x => x.ID).ToList();
      await GetSearchResult(model);

      return View(model);
    }

    public async Task<ActionResult> ContentPage(string id)
    {
      if (string.IsNullOrEmpty(id))
        return RedirectToAction("Index", "Home");

      var slug = id.ToLowerInvariant();
      var contentPageQuery = await _contentPageService.Query(x => x.Slug == slug && x.Published).SelectAsync();
      var contentPage = contentPageQuery.FirstOrDefault();

      if (contentPage == null)
      {
        return new HttpNotFoundResult();
      }

      return View(contentPage);
    }

    public ActionResult About()
    {
      ViewBag.Message = "Your application description page.";

      return View();
    }

    public ActionResult Contact()
    {
      ViewBag.Message = "Your contact page.";

      var model = new ContactModel();

      if (User.Identity.IsAuthenticated)
      {
        model.Email = User.Identity.User().Email;
      }

      return View(model);
    }

    public async Task<ActionResult> Search(SearchListingModel model)
    {
      await GetSearchResult(model);

      return View("~/Views/Listing/Listings.cshtml", model);
    }

    private async Task GetSearchResult(SearchListingModel model)
    {
      IEnumerable<Listing> items = null;
      List<MetaFieldSearchModel> metaFields = model.CustomFields;

      // Category
      if (model.CategoryID != 0)
      {
        items = await _listingService.Query(x => x.CategoryID == model.CategoryID)
            .Include(x => x.ListingPictures)
            .Include(x => x.Category)
            .Include(x => x.ListingType)
            .Include(x => x.AspNetUser)
            .Include(x => x.ListingReviews)
            .Include(x => x.ListingMetas)
            .SelectAsync();

        // Set listing types
        model.ListingTypes = CacheHelper.ListingTypes.Where(x => x.CategoryListingTypes.Any(y => y.CategoryID == model.CategoryID)).ToList();

        //Custom fields which are searchable for selected category
        var customFieldCategoryQuery = await _customFieldCategoryService.Query(x => x.CategoryID == model.CategoryID && x.MetaField.Searchable).Include(x => x.MetaField.ListingMetas).SelectAsync();
        var customFieldCategories = customFieldCategoryQuery.ToList();
        model.CustomFields = customFieldCategoryQuery.Select(x => new MetaFieldSearchModel
        {
          ID = x.MetaField.ID,
          Name = x.MetaField.Name,
          Placeholder = x.MetaField.Placeholder,
          ControlTypeID = x.MetaField.ControlTypeID,
          Options = x.MetaField.Options,
          Ordering = x.MetaField.Ordering
        }).OrderBy(ob=>ob.Ordering).ToList();
      }
      else
      { 
        model.ListingTypes = CacheHelper.ListingTypes;

        // All Custom fields which are searchable
        var metaFieldQuery = await _metaFieldService.Query(x => x.Searchable).SelectAsync();
        model.CustomFields = metaFieldQuery.Select(x => new MetaFieldSearchModel
        {
          ID = x.ID,
          Name = x.Name,
          Placeholder = x.Placeholder,
          ControlTypeID = x.ControlTypeID,
          Options = x.Options,
          Ordering = x.Ordering
        }).OrderBy(ob => ob.Ordering).ToList();
      }

      // Set default Listing Type if it's not set or listing type is not set
      if (model.ListingTypes.Count > 0 &&
          (model.ListingTypeID == null || !model.ListingTypes.Any(x => model.ListingTypeID.Contains(x.ID))))
      {
        model.ListingTypeID = new List<int>();
        model.ListingTypeID.Add(model.ListingTypes.FirstOrDefault().ID);
      }

      // Search Text
      if (!string.IsNullOrEmpty(model.SearchText))
      {
        model.SearchText = model.SearchText.ToLower();

        // Search by title, description, location
        if (items != null)
        {
          items = items.Where(x =>
              x.Title.ToLower().Contains(model.SearchText) ||
              x.Description.ToLower().Contains(model.SearchText) ||
              x.Location.ToLower().Contains(model.SearchText));
        }
        else
          items = await _listingService.Query(
              x => x.Title.ToLower().Contains(model.SearchText) ||
              x.Description.ToLower().Contains(model.SearchText) ||
              x.Location.ToLower().Contains(model.SearchText))
              .Include(x => x.ListingPictures)
              .Include(x => x.Category)
              .Include(x => x.AspNetUser)
              .Include(x => x.ListingReviews)
              .SelectAsync();
      }

      // Latest
      if (items == null)
      {
        items = await _listingService.Query().OrderBy(x => x.OrderByDescending(y => y.Created))
            .Include(x => x.ListingPictures)
            .Include(x => x.Category)
            .Include(x => x.AspNetUser)
            .Include(x => x.ListingReviews)
            .Include(x => x.ListingMetas)
            .SelectAsync();
      }

      // Filter items by Listing Type
      items = items.Where(x => model.ListingTypeID.Contains(x.ListingTypeID)).ToList();

      // Location
      if (!string.IsNullOrEmpty(model.Location))
      {
        items = items.Where(x => !string.IsNullOrEmpty(x.Location) && x.Location.IndexOf(model.Location, StringComparison.OrdinalIgnoreCase) != -1);
      }

      // Custom fields
      if (metaFields != null)
      {
        metaFields.RemoveAll(x => x.Options == null); // ignore any blank default values
        if (metaFields.Count>0)
        {
          var metaFieldSelected = metaFields.Select(x => new { x.ID, x.Options }).ToList();
          //var metaFieldTest = items.Select(i => i.ListingMetas).ToList(); //debug test....
          foreach (var metaField in metaFields)
          {
            items = items.Where(x => x.ListingMetas.Any(y => metaField.ID == y.FieldID && metaField.Options.Contains(y.Value))).ToList();
            model.CustomFields.FirstOrDefault(x => x.ID == metaField.ID).Selected = metaField.Options;
          }
        }
      }

      // Picture
      if (model.PhotoOnly)
        items = items.Where(x => x.ListingPictures.Count > 0);

      /// Price
      if (model.PriceFrom.HasValue)
        items = items.Where(x => x.Price >= model.PriceFrom.Value);

      if (model.PriceTo.HasValue)
        items = items.Where(x => x.Price <= model.PriceTo.Value);

      // Show active and enabled only
      var itemsModelList = new List<ListingItemModel>();
      foreach (var item in items.Where(x => x.Active && x.Enabled).OrderByDescending(x => x.Created))
      {
        itemsModelList.Add(new ListingItemModel()
        {
          ListingCurrent = item,
          UrlPicture = item.ListingPictures.Count == 0 ? ImageHelper.GetListingImagePath(0) : ImageHelper.GetListingImagePath(item.ListingPictures.OrderBy(x => x.Ordering).FirstOrDefault().PictureID)
        });
      }
      var breadCrumb = GetParents(model.CategoryID).Reverse().ToList();

      model.BreadCrumb = breadCrumb;
      model.Categories = CacheHelper.Categories;

      model.Listings = itemsModelList;
      model.ListingsPageList = itemsModelList.ToPagedList(model.PageNumber, model.PageSize);
      model.Grid = new ListingModelGrid(model.ListingsPageList.AsQueryable());
    }

    IEnumerable<Category> GetParents(int categoryId)
    {
      Category category = _categoryService.Find(categoryId);
      while (category != null && category.Parent != category.ID)
      {
        yield return category;
        category = _categoryService.Find(category.Parent);
      }
    }

    [ChildActionOnly]
    public ActionResult NavigationSide()
    {
      var rootId = 0;
      var categories = CacheHelper.Categories.ToList();

      var categoryTree = categories.OrderBy(x => x.Parent).ThenBy(x => x.Ordering).ToList().GenerateTree(x => x.ID, x => x.Parent, rootId);

      // Save content page user access roles
      var contentPages = CacheHelper.ContentPages.Where(x => x.Published).OrderBy(x => x.Ordering).ToList();
      foreach(var contentPage in contentPages)
      {
        var contentPageRoles = _contentPageRoleService.Query(x => x.ContentPageID == contentPage.ID).Select();
        contentPage.ContentPageRoles= contentPageRoles.ToList();
      }

      // Get content pages that current user is authorised to access
      var userRoleIds = new List<string>();
      if (User != null && User.Identity.IsAuthenticated)
      {
        var userId = User.Identity.GetUserId();
        var userRoleNames = UserManager.GetRoles(userId).ToList();
        foreach (var userRoleName in userRoleNames) { userRoleIds.Add(RoleManager.FindByName(userRoleName).Id); }
        contentPages = contentPages.Where(x => (x.ContentPageRoles.Count < 1) || x.ContentPageRoles.Any(sm => userRoleIds.Contains(sm.AspNetRoleID))).ToList();
      }
      else
      {
        contentPages = contentPages.Where(x => (x.ContentPageRoles.Count < 1)).ToList();
      }

      var model = new NavigationSideModel()
      {
        CategoryTree = categoryTree,
        ContentPages = contentPages
      };


      return View("_NavigationSide", model);
    }

    [ChildActionOnly]
    public ActionResult LanguageSelector()
    {
      //var languages = i18n.LanguageHelpers.GetAppLanguages();
      var languages = LanguageHelper.AvailableLanguges.Languages;
      var languageCurrent = ControllerContext.RequestContext.HttpContext.GetPrincipalAppLanguageForRequest();

      var model = new LanguageSelectorModel();
      model.Culture = languageCurrent.GetLanguage();
      model.DisplayName = languageCurrent.GetCultureInfo().NativeName;

      foreach (var language in languages)
      {
        if (language.Culture != languageCurrent.GetLanguage() && language.Enabled)
        {
          model.LanguageList.Add(new LanguageSelectorModel()
          {
            Culture = language.Culture,
            DisplayName = language.LanguageTag.CultureInfo.NativeName
          });
        }
      }

      return PartialView("_LanguageSelector", model);
    }

    [AllowAnonymous]
    public ActionResult SetLanguage(string langtag, string returnUrl)
    {
      // If valid 'langtag' passed.
      i18n.LanguageTag lt = i18n.LanguageTag.GetCachedInstance(langtag);
      if (lt.IsValid())
      {
        // Set persistent cookie in the client to remember the language choice.
        Response.Cookies.Add(new HttpCookie("i18n.langtag")
        {
          Value = lt.ToString(),
          HttpOnly = true,
          Expires = DateTime.UtcNow.AddYears(1)
        });
      }
      // Owise...delete any 'language' cookie in the client.
      else
      {
        var cookie = Response.Cookies["i18n.langtag"];
        if (cookie != null)
        {
          cookie.Value = null;
          cookie.Expires = DateTime.UtcNow.AddMonths(-1);
        }
      }
      // Update PAL setting so that new language is reflected in any URL patched in the 
      // response (Late URL Localization).
      HttpContext.SetPrincipalAppLanguageForRequest(lt);
      // Patch in the new langtag into any return URL.
      if (returnUrl.IsSet())
      {
        returnUrl = LocalizedApplication.Current.UrlLocalizerForApp.SetLangTagInUrlPath(HttpContext, returnUrl, UriKind.RelativeOrAbsolute, lt == null ? null : lt.ToString()).ToString();
      }
      //Redirect user agent as approp.
      return this.Redirect(returnUrl);
    }
    #endregion
  }
}