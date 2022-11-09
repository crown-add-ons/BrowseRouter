using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Text;

namespace BrowseRouter;

public static class UrlPreferenceExtensions
{

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hwnd, StringBuilder ss, int count);

    private static string ActiveWindowTitle()
    {
        //Create the variable
        const int nChar = 256;
        StringBuilder ss = new StringBuilder(nChar);

        //Run GetForeGroundWindows and get active window informations
        //assign them into handle pointer variable
        IntPtr handle = IntPtr.Zero;
        handle = GetForegroundWindow();

        if (GetWindowText(handle, ss, nChar) > 0) return ss.ToString() + " ";
        else return "";
    }

    public static bool TryGetPreference(this IEnumerable<UrlPreference> prefs, Uri uri, out UrlPreference pref)
  {
    pref = prefs.FirstOrDefault(pref =>
    {
      (string domain, string pattern) = pref.GetDomainAndPattern(uri);
        domain = ActiveWindowTitle() + domain;
      return Regex.IsMatch(domain, pattern);
    })!;

    return pref != null;
  }

  public static (string, string) GetDomainAndPattern(this UrlPreference pref, Uri uri)
  {
    string urlPattern = pref.UrlPattern;

    if (urlPattern.StartsWith("/") && urlPattern.EndsWith("/"))
    {
      // The domain from the INI file is a regex
      string domain = uri.Authority + uri.AbsolutePath;
      string pattern = urlPattern.Substring(1, urlPattern.Length - 2);

      return (domain, pattern);
    }

    if (urlPattern.StartsWith("?") && urlPattern.EndsWith("?"))
    {
        // The domain from the INI file is a query filter
        string domain = uri.Authority + uri.PathAndQuery;
        string pattern = urlPattern.Substring(1, urlPattern.Length - 2);

        // Escape the input for regex; the only special character we support is a *
        var regex = Regex.Escape(pattern);

        // Unescape * as a wildcard.
        pattern = $"^{regex.Replace("\\*", ".*")}$";

        return (domain, pattern);
    }

    {
      // We're only checking the domain.
      string domain = uri.Authority;

      // Escape the input for regex; the only special character we support is a *
      var regex = Regex.Escape(urlPattern);

      // Unescape * as a wildcard.
      string pattern = $"^{regex.Replace("\\*", ".*")}$";

      return (domain, pattern);
    }
  }
}