using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.Networking;

namespace SupersonicWisdomSDK.Editor
{
    internal static class SwPlatformCommunication
    {
        #region --- Public Methods ---

        public static Dictionary<string, string> CreateAuthorizationHeadersDictionary ()
        {
            return string.IsNullOrWhiteSpace(SwAccountUtils.AccountToken) ? new Dictionary<string, string>() : new Dictionary<string, string>
            {
                { "authorization", "Bearer " + SwAccountUtils.AccountToken }
            };
        }

        #endregion


        #region --- Inner Classes ---

        internal static class URLs
        {
            #region --- Constants ---

            internal const string CurrentStageApi = BaseWisdomPartners + "current-stage";
            internal const string DownloadWisdomPackage = BaseWisdomPartners + "download-package";

            internal const string Login = BasePartners + "login";

            internal const string Titles = BasePartners + "titles?embed=games&limit=0&order=asc&page=0&sort=name&includePrototypes=1";
            internal const string UpdateMessagesConfig = "https://assets.mobilegamestats.com/docs/stage-update-config.json";

            private const string Base = "https://super-api.supersonic.com/v1/";
            private const string BasePartners = Base + "partners/";
            private const string BaseWisdomPartners = Base + "partners/wisdom/";

            #endregion
        }

        #endregion
    }
}