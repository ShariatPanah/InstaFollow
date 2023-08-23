using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using InstaSharp.Models.Responses;
using InstaSharp;
using System.ComponentModel;

namespace InstaFollow
{
    [Activity(Label = "AccountsActivity")]
    public class AccountsActivity : Activity
    {
        InstagramSession Session;
        private ListView lstAccounts;
        private AccountsListAdapter AccountsAdapter;
        private static InstagramConfig Config;
        private static InstaSharp.Endpoints.Users CurrentUser;
        private static UserResponse FetchedUser;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            InitializeComponent();
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.AccountsLayout);

            NewAccountButtonEvent();
            BackToMainButtonEvent();
            ListViewItemsClick();

            BackgroundWorker oWorker = new BackgroundWorker();
            oWorker.DoWork += OWorker_DoWork;
            oWorker.RunWorkerCompleted += OWorker_RunWorkerCompleted;
            oWorker.RunWorkerAsync();

            //ShowSubmittedAccounts();
        }

        private void OWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RunOnUiThread(() => { ShowSubmittedAccounts(); });
        }

        private async void OWorker_DoWork(object sender, DoWorkEventArgs e)
        {
           await UpdateAuthorizesAsync(RetrieveAllAuthos());
        }

        private void ListViewItemsClick()
        {
            lstAccounts = FindViewById<ListView>(Resource.Id.lstAccounts);
        }

        public void InitializeComponent()
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

        public void HandleDeleteButtonClick(object sender, ResponsesEventArgs e)
        {
            if (e.OAuthResponse != null)
            {
                Session.RemoveAuthResponse(e.OAuthResponse);

                ShowSubmittedAccounts();

                Toast.MakeText(this, "حذف شد.", ToastLength.Short).Show();
            }
        }

        public void HandleCustomLayoutClick(object sender, ResponsesEventArgs e)
        {
            // store lastusedauth
            Session.StoreLastUsedAuthorizeResponse(e.OAuthResponse);

            // set result for MainActivity.cs
            var oIntent = new Intent(this, typeof(MainActivity));
            oIntent.PutExtra("accountinfo", Newtonsoft.Json.JsonConvert.SerializeObject(e.OAuthResponse));
            this.SetResult(Result.Ok, oIntent);

            this.Finish();
        }

        public void ShowSubmittedAccounts()
        {
            if (!Session.IsSessionNull())
            {
                var allAccounts = Session.GetAllAuthoResponses();

                AccountsAdapter = new AccountsListAdapter(this, allAccounts);
                lstAccounts.Adapter = AccountsAdapter;
            }
            else
            {
                lstAccounts.Adapter = null;
            }
        }

        public List<OAuthResponse> RetrieveAllAuthos()
        {
            if (!Session.IsSessionNull())
            {
                return Session.GetAllAuthoResponses();
            }

            return null;
        }

        public void NewAccountButtonEvent()
        {
            var btnNewAccount = FindViewById<ImageButton>(Resource.Id.btnNewAccount);
            btnNewAccount.Click += BtnNewAccount_Click;
        }

        public void BackToMainButtonEvent()
        {
            var btnBackToMain = FindViewById<ImageButton>(Resource.Id.btnBackToMain);
            btnBackToMain.Click += BtnBackToMain_Click;
        }

        private void BtnBackToMain_Click(object sender, EventArgs e)
        {
            Finish();
        }

        private void BtnNewAccount_Click(object sender, EventArgs e)
        {
            Intent oIntent = new Intent(this, typeof(LoginActivity));
            StartActivityForResult(oIntent, 0);
        }

        private async System.Threading.Tasks.Task UpdateAuthorizesAsync(List<OAuthResponse> responses)
        {
            if (responses != null)
            {
                try
                {
                    for (int i = 0; i < responses.Count; i++)
                    {
                        CurrentUser = new InstaSharp.Endpoints.Users(Config, responses[i]);

                        FetchedUser = await CurrentUser.GetSelf();
                        responses[i].User = FetchedUser.Data;
                    }

                    Session.StoreBunchAuthorizeResponses(responses);
                }
                catch (Exception ex)
                {
                    Android.Util.Log.Error("Instagram accountacc", ex.Message);
                }
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                if (data.GetBooleanExtra("refreshing", true))
                {
                    ShowSubmittedAccounts();
                }
            }
            else if (resultCode == Result.Canceled)
            {
                Toast.MakeText(this, "خطا هنگام ورود به اکانت اینستاگرام، لطفا اینستاگرام خود را بررسی نمایید.", ToastLength.Long);
            }
        }
    }
}