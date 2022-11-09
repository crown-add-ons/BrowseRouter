using System.Diagnostics;
using Microsoft.Toolkit.Uwp.Notifications;

namespace BrowseRouter;

public class BrowserService
{
  private readonly IConfigService _config;

  public BrowserService(IConfigService config)
  {
    _config = config;
  }

  public void Launch(string url)
  {
    try
    {
      IEnumerable<UrlPreference> prefs = _config.GetUrlPreferences();
      Uri uri = UriFactory.Get(url);

      if (!prefs.TryGetPreference(uri, out UrlPreference pref))
      {
        Log.Write($"Unable to find a browser matching {url}.");
        return;
      }

      (string path, string args) = Executable.GetPathAndArgs(pref.Browser.Location);

      // display toast
      // see https://learn.microsoft.com/en-us/windows/apps/design/shell/tiles-and-notifications/send-local-toast?tabs=uwp
      // requires addition of <BuiltInComInteropSupport>true</BuiltInComInteropSupport> to your PropertyGroup in your csproj file.
      // TODO? add button to edit config
      new ToastContentBuilder()
        .AddText("Starting browser " + pref.Browser.Name + " for url " + url)
        .Show(toast =>
        {
          toast.ExpirationTime = DateTime.Now.AddMinutes(1);
        });

      // We need to use an absolute URI value, to prevent uri.ToString() - in this case some symbols in HTML encoding are replaced (for example %20)
      Process.Start(path, $"{args} {uri.AbsoluteUri}");
    }
    catch (Exception e)
    {
      Log.Write($"{e}");
    }
  }
}