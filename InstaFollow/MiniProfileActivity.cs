using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InstaSharp.Models;
using InstaFollow.Helpers;
using InstaSharp;
using InstaSharp.Models.Responses;
using Nito.AsyncEx;
using System.Threading.Tasks;
using Android.Util;
using Android.Content.PM;
using System.Text.RegularExpressions;

namespace InstaFollow
{
    [Activity(Label = "MiniProfileActivity", Theme = "@android:style/Theme.Dialog")]
    public class MiniProfileActivity : Activity
    {
        private TextView txtFollowingCount;
        private TextView txtFollowersCount;
        private TextView txtPostsCount;
        private TextView txtUsername;
        private TextView txtFullname;
        private TextView txtBio;
        private TextView txtPostsStatus;
        private Refractored.Controls.CircleImageView imgProfilePicture;
        private ImageView imgPostImage1;
        private ImageView imgPostImage2;
        private ImageView imgPostImage3;
        private LinearLayout btnOpenInInstagram;
        private User _selectedUser;
        private InstagramConfig Config;
        private OAuthResponse CurrentAuth;
        private static InstaSharp.Endpoints.Users CurrentUser;
        private UserResponse SelectedUserResponse;
        private ProgressDialog progressing;
        System.Net.WebClient wc;
        Regex HrefRegex;
        Match HrefMatch;
        string[] arrRegex = new string[] { "\"biography\": \"(.+?)\"", "\"followed_by\": \\{\"count\": (.+?)\\}", "\"follows\": \\{\"count\": (.+?)\\}", ", \"count\": (.+?)," };
        string strSource;


        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            InitializeComponent();

            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetBackgroundDrawableResource(Resource.Color.MyApplicationBlackColor);

            progressing = new ProgressDialog(this);
            progressing.RequestWindowFeature((int)WindowFeatures.NoTitle);
            progressing.SetMessage("منتظر بمانید...");
            progressing.Show();

            // Create your application here
            SetContentView(Resource.Layout.MiniProfileLayout);

            FindAllViews();

            //AsyncContext.Run(() => SetValuesAsync());
            await SetValuesAsync();
            progressing.Dismiss();
        }

        private void InitializeComponent()
        {
            Config = new InstagramConfig()
            {
                ClientId = Resources.GetString(InstaFollow.Resource.String.InstaClientId),
                RedirectUri = Resources.GetString(InstaFollow.Resource.String.InstaRedirectURI),
                ClientSecret = Resources.GetString(InstaFollow.Resource.String.InstaClientSecret),
                CallbackUri = "",
            };

            _selectedUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(Intent.Extras.GetString("selectedUser"));
            CurrentAuth = Newtonsoft.Json.JsonConvert.DeserializeObject<OAuthResponse>(Intent.Extras.GetString("currentauth"));

            CurrentUser = new InstaSharp.Endpoints.Users(Config, CurrentAuth);
        }

        private async Task SetValuesAsync()
        {
            try
            {
                imgPostImage1.Visibility = imgPostImage2.Visibility = imgPostImage3.Visibility = txtBio.Visibility = ViewStates.Gone;

                SelectedUserResponse = await CurrentUser.Get(_selectedUser.Id);

                if (SelectedUserResponse != null && SelectedUserResponse.Data != null)
                {
                    txtFollowingCount.Text = SelectedUserResponse.Data.Counts.Follows.ToString();
                    txtFollowersCount.Text = SelectedUserResponse.Data.Counts.FollowedBy.ToString();
                    txtPostsCount.Text = SelectedUserResponse.Data.Counts.Media.ToString();
                    txtUsername.Text = SelectedUserResponse.Data.Username.ToUpper();
                    txtFullname.Text = SelectedUserResponse.Data.FullName;
                    txtBio.Text = SelectedUserResponse.Data.Bio.Replace("\n", " ");
                    new DownloadImageTask(imgProfilePicture).Execute(SelectedUserResponse.Data.ProfilePicture);

                    var mediaResponse = await CurrentUser.Recent(SelectedUserResponse.Data.Id, null, null, 3, null, null);
                    if (mediaResponse.Data.Count != 0)
                    {
                        new DownloadImageTask(imgPostImage1).Execute(mediaResponse.Data[0].Images.Thumbnail.Url);
                        new DownloadImageTask(imgPostImage2).Execute(mediaResponse.Data[1].Images.Thumbnail.Url);
                        new DownloadImageTask(imgPostImage3).Execute(mediaResponse.Data[2].Images.Thumbnail.Url);
                        imgPostImage1.Visibility = imgPostImage2.Visibility = imgPostImage3.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        txtPostsStatus.Visibility = ViewStates.Visible;
                        imgPostImage1.Visibility = imgPostImage2.Visibility = imgPostImage3.Visibility = ViewStates.Gone;
                    }
                }
                else
                {
                    txtUsername.Text = _selectedUser.Username.ToUpper();
                    txtFullname.Text = _selectedUser.FullName;
                    new DownloadImageTask(imgProfilePicture).Execute(_selectedUser.ProfilePicture);
                    GetPrivateAccountInfo();

                    txtPostsStatus.Text = "این اکانت خصوصی است!";
                    txtPostsStatus.Visibility = ViewStates.Visible;
                }

                if (txtBio.Text.Trim() != string.Empty)
                {
                    txtBio.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Instagram MiniProfile", ex.Message);
            }
        }

        private void FindAllViews()
        {
            txtFollowingCount = FindViewById<TextView>(Resource.Id.txtMiniProfileFollowingCount);
            txtFollowersCount = FindViewById<TextView>(Resource.Id.txtMiniProfileFollowersCount);
            txtPostsCount = FindViewById<TextView>(Resource.Id.txtMiniProfilePostsCount);
            txtUsername = FindViewById<TextView>(Resource.Id.txtMiniProfileUsername);
            txtFullname = FindViewById<TextView>(Resource.Id.txtMiniProfileFullName);
            txtBio = FindViewById<TextView>(Resource.Id.txtMiniProfileBio);
            txtPostsStatus = FindViewById<TextView>(Resource.Id.txtMiniProfilePostsStatus);
            imgProfilePicture = FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.imgMiniProfilePicture);
            imgPostImage1 = FindViewById<ImageView>(Resource.Id.imgMiniPostImage1);
            imgPostImage2 = FindViewById<ImageView>(Resource.Id.imgMiniPostImage2);
            imgPostImage3 = FindViewById<ImageView>(Resource.Id.imgMiniPostImage3);
            btnOpenInInstagram = FindViewById<LinearLayout>(Resource.Id.btnMiniProfileOpenInInstagram);

            if (!btnOpenInInstagram.HasOnClickListeners)
            {
                btnOpenInInstagram.Click += delegate
                {
                    var uri = Android.Net.Uri.Parse(string.Format("https://www.instagram.com/_u/{0}", _selectedUser.Username));
                    Intent likeIng = new Intent(Intent.ActionView, uri);

                    likeIng = likeIng.SetPackage("com.instagram.android");

                    try
                    {
                        if (IsIntentAvailable(likeIng))
                        {
                            StartActivity(likeIng);
                        }
                        else
                        {
                            StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(string.Format("https://www.instagram.com/_u/{0}", _selectedUser.Username))));
                        }
                    }
                    catch (ActivityNotFoundException ex)
                    {
                        Log.Debug("Instagram MiniProfile", ex.Message);
                    }
                };
            }
        }

        private bool IsIntentAvailable(Intent intent)
        {
            Android.Content.PM.PackageManager packageManager = this.PackageManager;
            var list = packageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            if (list.Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void GetPrivateAccountInfo()
        {
            wc = new System.Net.WebClient();
            try
            {
                strSource = wc.DownloadString($"https://www.instagram.com/{_selectedUser.Username}");
                strSource = strSource.Substring(strSource.IndexOf("\"entry_data\":"));

                for (int i = 0; i < 4; i++)
                {
                    HrefRegex = new Regex(arrRegex[i], RegexOptions.IgnoreCase | RegexOptions.Compiled);

                    // parse the values from the source
                    HrefMatch = HrefRegex.Match(strSource);

                    // while there are matches
                    if (HrefMatch.Success)
                    {
                        switch (i)
                        {
                            case 0:
                                txtBio.Text = Regex.Unescape(HrefMatch.Groups[1].Value).Replace("\n", " ");
                                break;

                            case 1:
                                txtFollowersCount.Text = HrefMatch.Groups[1].Value;
                                break;

                            case 2:
                                txtFollowingCount.Text = HrefMatch.Groups[1].Value;
                                break;

                            case 3:
                                txtPostsCount.Text = HrefMatch.Groups[1].Value;
                                break;

                            default:
                                txtBio.Text = HrefMatch.Groups[1].Value;
                                break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error("Instagram Webclient", ex.Message);
            }
            finally
            {
                if (wc != null)
                {
                    wc.Dispose();
                    wc = null;
                }
            }
        }
    }
}