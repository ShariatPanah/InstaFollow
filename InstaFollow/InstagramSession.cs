using System;
using System.Linq;
using Android.Content;
using System.Collections.Generic;
using InstaSharp.Models.Responses;
using Android.Util;

namespace InstaFollow
{
    public class InstagramSession
    {
        private ISharedPreferences SharedPref;
        private ISharedPreferencesEditor Editor;
        private static string Shared = "InstagramPreferences";
        private static string USERAUTHOS = "USERAUTHOS";
        private static string LASTUSEDACCOUNT = "LASTUSEDACCOUNT";
        private static string UNFOLLOWEDCOUNT = "-UNFOLLOWEDCOUNT";

        public InstagramSession(Context context)
        {
            SharedPref = context.GetSharedPreferences(Shared, FileCreationMode.Private);
            Editor = SharedPref.Edit();
        }

        public bool IsSessionNull()
        {
            var allResponses = GetAllAuthoResponses();
            if (allResponses != null && allResponses.Count != 0)
            {
                return false;
            }

            return true;
        }

        public void StoreUnfollowedCount(OAuthResponse authos, int count, DateTime now)
        {
            Editor.PutString(authos.User.Id + UNFOLLOWEDCOUNT, count + "-" + now);
            Editor.Commit();
        }

        public void StoreLastUsedAuthorizeResponse(OAuthResponse authos)
        {
            string strJsonAuthos = Newtonsoft.Json.JsonConvert.SerializeObject(authos);

            Editor.PutString(LASTUSEDACCOUNT, strJsonAuthos);
            Editor.Commit();
        }

        public void StoreBunchAuthorizeResponses(List<OAuthResponse> authos)
        {
            string strJsonAuthos = Newtonsoft.Json.JsonConvert.SerializeObject(authos);

            Editor.PutString(USERAUTHOS, strJsonAuthos);
            Editor.Commit();
        }

        public void AddAuthResponse(OAuthResponse oauth)
        {
            List<OAuthResponse> storedAuthos = GetAllAuthoResponses();
            if (storedAuthos == null)
            {
                storedAuthos = new List<OAuthResponse>();
            }

            if (GetOAuthResponse(oauth.User.Id) == null)
            {
                storedAuthos.Add(oauth);
            }

            StoreBunchAuthorizeResponses(storedAuthos);
        }

        public void RemoveAuthResponse(OAuthResponse oauth)
        {
            try
            {
                List<OAuthResponse> storedAuthos = GetAllAuthoResponses();
                if (storedAuthos != null)
                {
                    storedAuthos.Remove(storedAuthos.FirstOrDefault(a => a.User.Id == oauth.User.Id));
                    //storedAuthos.Remove(storedAuthos.FirstOrDefault(a => a.AccessToken == oauth.AccessToken));
                    StoreBunchAuthorizeResponses(storedAuthos);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Instagram Session", ex.Message);
            }
        }

        public void RemoveAllAuthResponses()
        {
            Editor.PutString(USERAUTHOS, null);
            Editor.Commit();
        }

        public List<OAuthResponse> GetAllAuthoResponses()
        {
            List<OAuthResponse> storedAuthos;

            if (SharedPref.Contains(USERAUTHOS))
            {
                String strJsonAuthos = SharedPref.GetString(USERAUTHOS, null);

                if (strJsonAuthos == "[]")
                {
                    storedAuthos = null;
                }
                else
                {
                    int lastIndex = strJsonAuthos.ToLower().LastIndexOf("\"user\"");
                    if (lastIndex > 1)
                    {
                        storedAuthos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OAuthResponse>>(strJsonAuthos);
                    }
                    else
                    {
                        storedAuthos = new List<OAuthResponse>();
                        storedAuthos.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<OAuthResponse>(strJsonAuthos));
                    }
                }
            }
            else
            {
                return null;
            }

            return storedAuthos;
        }

        public OAuthResponse GetOAuthResponse(long userId)
        {
            OAuthResponse oAuth = null;
            var allAuthos = GetAllAuthoResponses();

            if (allAuthos != null)
            {
                oAuth = allAuthos.FirstOrDefault(o => o.User.Id == userId);
            }

            return oAuth;
        }

        public OAuthResponse GetLastUsedAuthResponse()
        {
            OAuthResponse oLastUsedAuth = null;

            if (SharedPref.Contains(LASTUSEDACCOUNT))
            {
                String strJsonAuthos = SharedPref.GetString(LASTUSEDACCOUNT, null);

                if (strJsonAuthos == "[]")
                {
                    oLastUsedAuth = null;
                }
                else
                {
                    oLastUsedAuth = Newtonsoft.Json.JsonConvert.DeserializeObject<OAuthResponse>(strJsonAuthos);
                }
            }
            else
            {
                return null;
            }

            return oLastUsedAuth;
        }

        public string GetUnfollowedCount(OAuthResponse auth)
        {
            string value = string.Empty;
            if (SharedPref.Contains(auth.User.Id + UNFOLLOWEDCOUNT))
            {
                value = SharedPref.GetString(auth.User.Id + UNFOLLOWEDCOUNT, string.Empty);
            }

            return value;
        }
    }
}