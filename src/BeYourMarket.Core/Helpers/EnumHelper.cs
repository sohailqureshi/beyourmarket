using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System;

namespace BeYourMarket.Core.Helpers
{
  public static class EnumHelper
  {
    public static string DisplayName(this Enum value)
    {
      // the following is my variation on the extension method you linked to
      if (value == null) { return null; }

      FieldInfo field = value.GetType().GetField(value.ToString());
      if (field != null)
      {
        DisplayAttribute[] display = (DisplayAttribute[])field.GetCustomAttributes(typeof(DisplayAttribute), false);
        if (display.Length > 0)
        {
          return display[0].Name;
        }

        DescriptionAttribute[] description = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (description.Length > 0)
        {
          return description[0].Description;
        }
      }

      return value.ToString();
    }
  }  
}
