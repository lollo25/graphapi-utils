using static System.Net.WebRequestMethods;

namespace Graphapi.Utils;
internal static class Constants
{
    internal const string MicrosoftLoginClient = "MSFT_LOGIN_CLIENT";
    internal const string GraphApiClient = "MSFT_GRAPH_API_CLIENT";
    internal const string GroupsDownloadResiliencePipeline = "GROUPS_DOWNLOAD_RESILIENCE_PIPELINE";
    internal const string TokenUrlFormat = "{0}/oauth2/v2.0/token";
    internal const string LoginApiRootUrl = "https://login.microsoftonline.com";
    internal const string GraphApiRootUrl = "https://graph.microsoft.com";
    internal const string GraphApiVersion = "v1.0";
    internal const int DefaultPageSize = 100;
}
