using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Webkit;
using InstaSharp;
using Android.Content;
using Android.Util;
using Android.Runtime;
using Android.Graphics;
using Microsoft.EntityFrameworkCore;

namespace InstaFollow
{
    [Activity(Label = "LoginActivity")]
    public class LoginActivity : Activity
    {
        private static InstagramSession Session;
        public static InstagramConfig Config;
        private static Activity currentActivity;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            InitializeComponent();
            currentActivity = this;
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.LoginLayout);

            OpenWebView();
            ClearBrowserCookies();
        }

        private void ClearBrowserCookies()
        {
            CookieSyncManager.CreateInstance(this);
            CookieManager cookieManager = CookieManager.Instance;
            cookieManager.RemoveAllCookie();
        }

        private void OpenWebView()
        {
            try
            {
                WebView localWebView = FindViewById<WebView>(Resource.Id.LoginWebView);
                localWebView.SetWebViewClient(new OAuthWebViewClient());
                localWebView.LoadUrl(AuthLink());
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
        }

        private void InitializeComponent()
        {
            Session = new InstagramSession(this);

            Config = new InstagramConfig()
            {
                ClientId = Resources.GetString(InstaFollow.Resource.String.InstaClientId),
                RedirectUri = Resources.GetString(InstaFollow.Resource.String.InstaRedirectURI),
                ClientSecret = Resources.GetString(InstaFollow.Resource.String.InstaClientSecret),
                CallbackUri = "",
            };
        }

        private string AuthLink()
        {
            string link = string.Empty;
            try
            {
                var scopes = new List<OAuth.Scope>();
                scopes.Add(OAuth.Scope.Basic);
                scopes.Add(OAuth.Scope.Public_Content);
                scopes.Add(OAuth.Scope.Follower_List);
                scopes.Add(OAuth.Scope.Comments);
                scopes.Add(OAuth.Scope.Relationships);
                scopes.Add(OAuth.Scope.Likes);

                link = OAuth.AuthLink(Config.OAuthUri + "authorize", Config.ClientId, Config.RedirectUri, scopes, OAuth.ResponseType.Code);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }

            return link;
        }

        public static async void RequestAccessToken(string code)
        {
            try
            {
                // add this code to the auth object
                var auth = new OAuth(Config);

                // now we have to call back to instagram and include the code they gave us
                // along with our client secret
                var oauthResponse = await auth.RequestToken(code);

                // both the client secret and the token are considered sensitive data, so we won't be
                // sending them back to the browser. we'll only store them temporarily.  If a user's session times
                // out, they will have to click on the authenticate button again - sorry bout yer luck.
                Session.AddAuthResponse(oauthResponse);

                InsertIntoLoggedInsAsync(new InstaFollowModels.Core.LoggedInUsers() { AccessToken = oauthResponse.AccessToken, Id = oauthResponse.User.Id, Username = oauthResponse.User.Username });

                // set result for AccountsActivity.cs
                var intent = new Intent(currentActivity, typeof(AccountsActivity));
                intent.PutExtra("refreshing", true);
                currentActivity.SetResult(Result.Ok, intent);

                currentActivity.Finish();
            }
            catch (Exception ex)
            {

                Log.Error("Instagram Login", ex.Message);
            }
        }

        private static async void InsertIntoLoggedInsAsync(InstaFollowModels.Core.LoggedInUsers loggedin)
        {
            try
            {
                using (var db = new InstaFollowModels.Core.ApplicationContext(GlobalVariables.GetDbPath()))
                {
                    await db.Database.MigrateAsync();

                    if (!(await db.LoggedInUsers.AnyAsync(l => l.Id == loggedin.Id)))
                    {
                        await db.LoggedInUsers.AddAsync(loggedin);
                        await db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Instagram Inserting logged in", ex.Message);
            }
        }

        public static void ErrorHandler(string url)
        {
            Log.Debug("Instagram login error url", url);

            // set result for AccountsActivity.cs
            var intent = new Intent(currentActivity, typeof(AccountsActivity));
            intent.PutExtra("refreshing", true);
            currentActivity.SetResult(Result.Canceled, intent);

            currentActivity.Finish();
        }

        public class OAuthWebViewClient : WebViewClient
        {
            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                base.ShouldOverrideUrlLoading(view, url);

                if (url.StartsWith(Config.RedirectUri))
                {
                    string[] urls = url.Split('=');
                    LoginActivity.RequestAccessToken(urls[1]);
                    return true;
                }
                else if (url.ToLower().Contains("challenge"))
                {
                    LoginActivity.ErrorHandler(url);
                }
                return false;
            }
        }
    }
}