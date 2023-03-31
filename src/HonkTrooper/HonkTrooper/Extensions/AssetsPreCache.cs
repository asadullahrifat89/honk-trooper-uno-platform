
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HonkTrooper
{
    public static class AssetsPreCache
    {
        public static async Task PreloadImageAssets(Func<bool> progressAction)
        {
            try
            {
#if __ANDROID__ || __IOS__
                foreach (var template in Constants.CONSTRUCT_TEMPLATES)
                {
                    await LoadImageAsync(template.Uri);
                    progressAction();
                }
#else
                using HttpClient httpClient = new();

                foreach (var template in Constants.CONSTRUCT_TEMPLATES)
                {
                    await LoadImageAsync(template.Uri);

                    var indexUrl = Uno.Foundation.WebAssemblyRuntime.InvokeJS("window.location.href;");
                    var appPackageId = Environment.GetEnvironmentVariable("UNO_BOOTSTRAP_APP_BASE");

                    var baseUrl = $"{indexUrl}{appPackageId}";

                    var source = $"{baseUrl}/{template.Uri.AbsoluteUri.Replace("ms-appx:///", "")}";

                    var response = await httpClient.GetAsync(source);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsByteArrayAsync();
                        if (content is not null && content.Length > 0)
                        {   
                            progressAction();
                        }
#if DEBUG
                        LoggingExtensions.Log("image source: " + source);
#endif
                    }
                }
#endif
            }
            catch (Exception ex)
            {
                LoggingExtensions.Log(ex.Message);
            }
        }

        private static async Task LoadImageAsync(Uri uri)
        {
            await ImageCache.Instance.PreCacheAsync(uri: uri);
        }
    }
}
