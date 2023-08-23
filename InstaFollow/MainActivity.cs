using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using System.Linq;
using Android.Runtime;
using InstaSharp.Models.Responses;
using System;
using InstaFollow.Helpers;
using InstaSharp;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.Util;
using Android.Support.V4.Widget;
using System.Threading;
using System.ComponentModel;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace InstaFollow
{
    [Activity(Label = "InstaFollow", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private Button btnRefresh;
        private Button btnAccounts;
        private TextView txtUsername;
        private TextView txtFollowingCount;
        private TextView txtFollowersCount;
        private TextView txtBlockersCount;
        private TextView txtGainedFollowersCount;
        private TextView txtLostFollowersCount;
        private TextView txtNonFollowersCount;
        private TextView txtMutualFriendsCount;
        private TextView txtFansCount;
        private LinearLayout btnFollowing;
        private LinearLayout btnFollowers;
        private LinearLayout btnBlockers;
        private LinearLayout btnGainedFollowers;
        private LinearLayout btnLostFollowers;
        private LinearLayout btnNonFollowers;
        private LinearLayout btnMutualFriends;
        private LinearLayout btnFans;
        private SwipeRefreshLayout oRefreshLayout;
        private ProgressDialog oProgressing;
        private Refractored.Controls.CircleImageView imgProfilePicture;
        public static InstagramSession Session;
        public static OAuthResponse CurrentAuth;
        private static InstagramConfig Config;
        private static InstaSharp.Endpoints.Relationships UserRelationships;
        private static InstaSharp.Endpoints.Users CurrentUser;
        private static InstaFollowModels.Core.LoggedInUsers LoggedInUser;
        private static List<InstaSharp.Models.User> TotalFollowers;
        private static List<InstaSharp.Models.User> TotalFollowings;
        public static int UnfollowedCount = 0;
        public static DateTime StartUnfollowingTime;
        int NewLostFollowersCount = 0;
        int NewGainedFollowersCount = 0;
        RelationshipResponse relationshipsResults;

        private CustomAlertDialog oDialog;

        protected override void OnCreate(Bundle bundle)
        {
            InitializeComponent();
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.MainLayout);

            Nito.AsyncEx.AsyncContext.Run(() => WarmUpEF());

            AccountsButtonEvent();
            RefreshButtonEvent();

            GetAllViewsInLayout();

            //oProgressing = new ProgressDialog(this);
            //oProgressing.RequestWindowFeature((int)Android.Views.WindowFeatures.NoTitle);
            //oProgressing.SetMessage("Loading...");

            if (IsLoginSessionNull())
            {
                btnAccounts.PerformClick();
            }
            else
            {
                //CurrentAuth = Session.GetAllAuthoResponses().FirstOrDefault();
                CurrentAuth = Session.GetLastUsedAuthResponse();

                //GetUnfollowedCount();

                UpdateViewAsync();

                ButtonFollowingEvents();
                ButtonFollowersEvents();
                ButtonBlockersEvents();
                ButtonGainedFollowersEvents();
                ButtonLostFollowersEvents();
                ButtonNonFollowersEvents();
                ButtonMutualFriendsEvents();
                ButtonFansEvents();
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            Session.StoreUnfollowedCount(CurrentAuth, UnfollowedCount, StartUnfollowingTime);
        }

        protected override void OnResume()
        {
            base.OnResume();

            GetUnfollowedCount();
        }

        private async Task WarmUpEF()
        {
            try
            {
                using (var db = new InstaFollowModels.Core.ApplicationContext(GlobalVariables.GetDbPath()))
                {
                    await db.Database.MigrateAsync();

                    var testing = (await db.Users.FirstOrDefaultAsync()).Id;
                }
            }
            catch (Exception ex)
            {

                Log.Error("WarmUpEF MainActivity", ex.Message);
            }
        }

        private async Task DoSomeDbStuffAsync()
        {
            try
            {
                using (var db = new InstaFollowModels.Core.ApplicationContext(GlobalVariables.GetDbPath()))
                {
                    //await db.Database.EnsureCreatedAsync();

                    /////////////////////////////////// Updating Users list ///////////////////////////////////////
                    var tempUsersList = TotalFollowers.Select(selecting => new InstaFollowModels.Core.User()
                    {
                        Id = selecting.Id,
                        FullName = selecting.FullName,
                        ProfilePicture = selecting.ProfilePicture,
                        Username = selecting.Username
                    }).ToList();

                    tempUsersList.AddRange(TotalFollowings.Except(TotalFollowers).Select(selecting => new InstaFollowModels.Core.User()
                    {
                        Id = selecting.Id,
                        FullName = selecting.FullName,
                        ProfilePicture = selecting.ProfilePicture,
                        Username = selecting.Username
                    }).ToList());

                    var userFromdb = await db.Users.ToListAsync();
                    await db.Users.AddRangeAsync(tempUsersList.Except(userFromdb));

                    await db.SaveChangesAsync();
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////

                    LoggedInUser = db.LoggedInUsers.Find(CurrentAuth.User.Id);

                    db.Entry(LoggedInUser).Collection(log => log.Followers).Load();
                    db.Entry(LoggedInUser).Collection(log => log.Followings).Load();
                    db.Entry(LoggedInUser).Collection(log => log.LostFollowers).Load();
                    db.Entry(LoggedInUser).Collection(log => log.GainedFollowers).Load();

                    var newLostFollowers = LoggedInUser.Followers.Select(entry => entry.LoggedInUserFollowedByUser).Except(TotalFollowers.Select(selecting => new InstaFollowModels.Core.User()
                    {
                        Id = selecting.Id,
                        FullName = selecting.FullName,
                        ProfilePicture = selecting.ProfilePicture,
                        Username = selecting.Username
                    }));

                    NewLostFollowersCount = newLostFollowers.Count();

                    /////////////////////////////////// Updating lost followers list ///////////////////////////////////////
                    foreach (var item in newLostFollowers)
                    {
                        if (!LoggedInUser.LostFollowers.Any(lost => lost.LoggedInUserFollowedByUserId == item.Id))
                        {
                            LoggedInUser.LostFollowers.Add(new InstaFollowModels.Core.LostFollower() { LoggedInUserFollowedByUserId = item.Id, FollowedLoggedInUserId = LoggedInUser.Id });
                        }
                    }
                    await db.SaveChangesAsync();
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////

                    txtLostFollowersCount.Text = LoggedInUser.LostFollowers.Count + $" ({NewLostFollowersCount} جدید)";


                    var newGainedFollowers = TotalFollowers.Select(selecting => new InstaFollowModels.Core.User()
                    {
                        Id = selecting.Id,
                        FullName = selecting.FullName,
                        ProfilePicture = selecting.ProfilePicture,
                        Username = selecting.Username
                    }).ToList().Except(LoggedInUser.Followers.Select(entry => entry.LoggedInUserFollowedByUser));

                    NewGainedFollowersCount = newGainedFollowers.Count();

                    /////////////////////////////////// Updating gained followers list ///////////////////////////////////////
                    foreach (var item in newGainedFollowers)
                    {
                        if (!LoggedInUser.GainedFollowers.Any(lost => lost.LoggedInUserFollowedByUserId == item.Id))
                        {
                            LoggedInUser.GainedFollowers.Add(new InstaFollowModels.Core.GainedFollower() { LoggedInUserFollowedByUserId = item.Id, FollowedLoggedInUserId = LoggedInUser.Id });
                        }
                    }
                    await db.SaveChangesAsync();
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////

                    txtGainedFollowersCount.Text = LoggedInUser.GainedFollowers.Count + $" ({NewGainedFollowersCount} جدید)";

                    /////////////////////////////////// Updating Followers List ///////////////////////////////////////////
                    LoggedInUser.Followers.Clear();
                    await db.SaveChangesAsync();

                    foreach (var item in TotalFollowers.Select(selecting => new InstaFollowModels.Core.User()
                    {
                        Id = selecting.Id,
                        FullName = selecting.FullName,
                        ProfilePicture = selecting.ProfilePicture,
                        Username = selecting.Username
                    }))
                    {
                        LoggedInUser.Followers.Add(new InstaFollowModels.Core.Follower() { FollowedLoggedUder = LoggedInUser, LoggedInUserFollowedByUser = item });
                    }
                    await db.SaveChangesAsync();
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
            }
            catch (Exception ex)
            {

                Log.Error("DoSomeDbStuff MainActivity", ex.Message);
            }

            oRefreshLayout.Refreshing = false;
        }

        private void GetAllViewsInLayout()
        {
            imgProfilePicture = FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.imgMainProfilePicture);
            txtUsername = FindViewById<TextView>(Resource.Id.txtMainUsername);
            txtFollowersCount = FindViewById<TextView>(Resource.Id.txtMainFollowersCount);
            txtFollowingCount = FindViewById<TextView>(Resource.Id.txtMainFollowingCount);

            btnFollowers = FindViewById<LinearLayout>(Resource.Id.btnMainFollowers);
            btnFollowing = FindViewById<LinearLayout>(Resource.Id.btnMainFollowing);

            txtBlockersCount = FindViewById<TextView>(Resource.Id.txtMainBlockersCount);
            txtGainedFollowersCount = FindViewById<TextView>(Resource.Id.txtMainGainedFollowersCount);
            txtLostFollowersCount = FindViewById<TextView>(Resource.Id.txtMainLostFollowersCount);
            txtNonFollowersCount = FindViewById<TextView>(Resource.Id.txtMainNonFollowersCount);
            txtMutualFriendsCount = FindViewById<TextView>(Resource.Id.txtMainMutualFriendsCount);
            txtFansCount = FindViewById<TextView>(Resource.Id.txtMainFansCount);

            btnBlockers = FindViewById<LinearLayout>(Resource.Id.btnMainBlockers);
            btnGainedFollowers = FindViewById<LinearLayout>(Resource.Id.btnMainGainedFollowers);
            btnLostFollowers = FindViewById<LinearLayout>(Resource.Id.btnMainLostFollowers);
            btnNonFollowers = FindViewById<LinearLayout>(Resource.Id.btnMainNonFollowers);
            btnMutualFriends = FindViewById<LinearLayout>(Resource.Id.btnMainMutualFriends);
            btnFans = FindViewById<LinearLayout>(Resource.Id.btnMainFans);

            oRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.refreshLayout);
            oRefreshLayout.SetColorScheme(Android.Resource.Color.HoloBlueBright, Android.Resource.Color.HoloBlueDark, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloRedLight);
            oRefreshLayout.Refresh += ORefreshLayout_Refresh;
        }

        public void GetUnfollowedCount()
        {
            string strValue = Session.GetUnfollowedCount(CurrentAuth);
            if (strValue == string.Empty)
            {
                StartUnfollowingTime = DateTime.Now;
                UnfollowedCount = 0;

                Session.StoreUnfollowedCount(CurrentAuth, UnfollowedCount, StartUnfollowingTime);
            }
            else
            {
                UnfollowedCount = Convert.ToInt32(strValue.Substring(0, strValue.IndexOf('-')));
                StartUnfollowingTime = DateTime.Parse(strValue.Substring(strValue.IndexOf('-') + 1));

                if (DateTime.Now >= StartUnfollowingTime.AddHours(1))
                {
                    StartUnfollowingTime = DateTime.Now;
                    UnfollowedCount = 0;
                    Session.StoreUnfollowedCount(CurrentAuth, UnfollowedCount, StartUnfollowingTime);
                }
            }
        }

        private void ORefreshLayout_Refresh(object sender, EventArgs e)
        {
            BackgroundWorker oWorker = new BackgroundWorker();
            oWorker.DoWork += OWorker_DoWork;
            oWorker.RunWorkerCompleted += OWorker_RunWorkerCompleted;
            oWorker.RunWorkerAsync();
        }

        private void OWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //RunOnUiThread(() => { oRefreshLayout.Refreshing = false; });
        }

        private void OWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateViewAsync();
            //Thread.Sleep(3000);
        }

        private async Task InitializeUsersStuffAsync()
        {
            UserRelationships = new InstaSharp.Endpoints.Relationships(Config, CurrentAuth);
            CurrentUser = new InstaSharp.Endpoints.Users(Config, CurrentAuth);

            TotalFollowings = await GetAllFollowings();
            TotalFollowers = await GetAllFollowers();
        }

        private void ButtonBlockersEvents()
        {
            if (!btnBlockers.HasOnClickListeners)
            {
                btnBlockers.Click += delegate
                {
                    HandleBlockersButtonClick(btnBlockers, new ResponsesEventArgs() { OAuthResponse = CurrentAuth });
                };
            }
        }

        private void ButtonGainedFollowersEvents()
        {
            if (!btnGainedFollowers.HasOnClickListeners)
            {
                btnGainedFollowers.Click += delegate
                {
                    HandleGainedFollowersButtonClick(btnGainedFollowers, new ResponsesEventArgs() { OAuthResponse = CurrentAuth });
                };
            }
        }

        private void ButtonLostFollowersEvents()
        {
            if (!btnLostFollowers.HasOnClickListeners)
            {
                btnLostFollowers.Click += delegate
                {
                    HandleLostFollowersButtonClick(btnLostFollowers, new ResponsesEventArgs() { OAuthResponse = CurrentAuth });
                };
            }
        }

        private void ButtonNonFollowersEvents()
        {
            if (!btnNonFollowers.HasOnClickListeners)
            {
                btnNonFollowers.Click += delegate
                {
                    HandleNonFollowersButtonClick(btnNonFollowers, new ResponsesEventArgs() { OAuthResponse = CurrentAuth });
                };
            }
        }

        private void ButtonMutualFriendsEvents()
        {
            if (!btnMutualFriends.HasOnClickListeners)
            {
                btnMutualFriends.Click += delegate
                {
                    HandleMutualFriendsButtonClick(btnMutualFriends, new ResponsesEventArgs() { OAuthResponse = CurrentAuth });
                };
            }
        }

        private void ButtonFansEvents()
        {
            if (!btnFans.HasOnClickListeners)
            {
                btnFans.Click += delegate
                {
                    HandleFansButtonClick(btnFans, new ResponsesEventArgs() { OAuthResponse = CurrentAuth });
                };
            }
        }

        private void ButtonFollowersEvents()
        {
            if (!btnFollowers.HasOnClickListeners)
            {
                btnFollowers.Click += delegate
                {
                    HandleFollowersButtonClick(btnFollowers, new ResponsesEventArgs() { OAuthResponse = CurrentAuth });
                };
            }
        }

        private void ButtonFollowingEvents()
        {
            if (!btnFollowing.HasOnClickListeners)
            {
                btnFollowing.Click += delegate
                {
                    HandleFollowingBottonClick(btnFollowing, new ResponsesEventArgs() { OAuthResponse = CurrentAuth });
                };
            }
        }

        public void AccountsButtonEvent()
        {
            btnAccounts = FindViewById<Button>(Resource.Id.btnAccounts);

            btnAccounts.Click += BtnAccounts_Click;
        }

        public void RefreshButtonEvent()
        {
            btnRefresh = FindViewById<Button>(Resource.Id.btnRefresh);
            btnRefresh.Click += BtnRefresh_Click;
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

        public bool IsLoginSessionNull()
        {
            if (!Session.IsSessionNull())
            {
                return false;
            }

            return true;
        }

        private void BtnAccounts_Click(object sender, System.EventArgs e)
        {
            Intent oIntent = new Intent(this, typeof(AccountsActivity));
            StartActivityForResult(oIntent, 1);
        }

        private void BtnRefresh_Click(object sender, System.EventArgs e)
        {
            //InitializeUsersStuffAsync();
            UpdateViewAsync();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                if (requestCode == 1)
                {
                    CurrentAuth = Newtonsoft.Json.JsonConvert.DeserializeObject<OAuthResponse>(data.GetStringExtra("accountinfo"));
                    //GetUnfollowedCount();
                    UpdateViewAsync();
                }
            }
        }

        private async void UpdateViewAsync()
        {
            if (!oRefreshLayout.Refreshing)
            {
                oRefreshLayout.Refreshing = true;
            }

            //oProgressing.Show();
            try
            {
                await InitializeUsersStuffAsync();

                RunOnUiThread(async () =>
                {
                    txtUsername.Text = CurrentAuth.User.Username.ToUpper();
                    new DownloadImageTask(imgProfilePicture).Execute(CurrentAuth.User.ProfilePicture);

                    var userResponse = await CurrentUser.GetSelf();
                    txtFollowingCount.Text = userResponse.Data.Counts.Follows.ToString();
                    txtFollowersCount.Text = userResponse.Data.Counts.FollowedBy.ToString();
                    txtBlockersCount.Text = (await GetBlockedCount()).ToString();

                    await DoSomeDbStuffAsync();

                    txtNonFollowersCount.Text = (await GetNonFollowersCount()).ToString();
                    txtMutualFriendsCount.Text = (await GetMutualFriendsCount()).ToString();
                    txtFansCount.Text = (await GetFansCount()).ToString();


                    //txtGainedFollowersCount.Text = "0";
                    //txtLostFollowersCount.Text = "0";
                });
            }
            catch (Exception ex)
            {

                Log.Error("Instafollow mainacc", ex.Message);
            }

            //oRefreshLayout.Refreshing = false;
            //oProgressing.Dismiss();
        }

        public void HandleFollowingBottonClick(object sender, ResponsesEventArgs e)
        {
            try
            {
                var oIntent = new Intent(this, typeof(FollowsListActivity));
                oIntent.PutExtra("list", Newtonsoft.Json.JsonConvert.SerializeObject(TotalFollowings.Select(selecting =>
                new Helpers.Models.CustomUserInfo
                {
                    Bio = selecting.Bio,
                    Counts = selecting.Counts,
                    FullName = selecting.FullName,
                    Id = selecting.Id,
                    IsFollowedByMe = false,
                    IsFollower = false,
                    ProfilePicture = selecting.ProfilePicture,
                    Username = selecting.Username,
                    Website = selecting.Website
                })));
                oIntent.PutExtra("currentauth", Newtonsoft.Json.JsonConvert.SerializeObject(CurrentAuth));
                oIntent.PutExtra("selected", EnumTypes.FollowsListSelectedTypes.Following.ToString());
                StartActivity(oIntent);
            }
            catch (Exception ex)
            {

                Log.Error("Instagram mainacc", ex.Message);
            }
        }

        public void HandleFollowersButtonClick(object sender, ResponsesEventArgs e)
        {
            try
            {
                var oIntent = new Intent(this, typeof(FollowsListActivity));
                oIntent.PutExtra("list", Newtonsoft.Json.JsonConvert.SerializeObject(TotalFollowers.Select(selecting =>
                new Helpers.Models.CustomUserInfo
                {
                    Bio = selecting.Bio,
                    Counts = selecting.Counts,
                    FullName = selecting.FullName,
                    Id = selecting.Id,
                    IsFollowedByMe = false,
                    IsFollower = false,
                    ProfilePicture = selecting.ProfilePicture,
                    Username = selecting.Username,
                    Website = selecting.Website
                })));
                oIntent.PutExtra("currentauth", Newtonsoft.Json.JsonConvert.SerializeObject(CurrentAuth));
                oIntent.PutExtra("selected", EnumTypes.FollowsListSelectedTypes.Followers.ToString());
                StartActivity(oIntent);
            }
            catch (Exception ex)
            {

                Log.Error("Instagram mainacc", ex.Message);
            }
        }

        private void HandleBlockersButtonClick(object sender, ResponsesEventArgs e)
        {
            Toast.MakeText(this, "Hello", ToastLength.Long).Show();
        }

        private void HandleLostFollowersButtonClick(object sender, ResponsesEventArgs e)
        {
            try
            {
                var oIntent = new Intent(this, typeof(FollowsListActivity));
                oIntent.PutExtra("list", Newtonsoft.Json.JsonConvert.SerializeObject(LoggedInUser.LostFollowers.Select(selecting => selecting.LoggedInUserFollowedByUser)
                    .Select(select => new Helpers.Models.CustomUserInfo
                    {
                        Bio = null,
                        Counts = null,
                        FullName = select.FullName,
                        Id = select.Id,
                        IsFollowedByMe = false,
                        IsFollower = false,
                        ProfilePicture = select.ProfilePicture,
                        Username = select.Username,
                        Website = null
                    })));
                oIntent.PutExtra("currentauth", Newtonsoft.Json.JsonConvert.SerializeObject(CurrentAuth));
                oIntent.PutExtra("selected", EnumTypes.FollowsListSelectedTypes.LostFollowers.ToString());
                StartActivity(oIntent);
            }
            catch (Exception ex)
            {

                Log.Error("Instagram mainacc", ex.Message);
            }
        }

        private async void HandleNonFollowersButtonClick(object sender, ResponsesEventArgs e)
        {
            try
            {
                var oIntent = new Intent(this, typeof(FollowsListActivity));
                oIntent.PutExtra("list", Newtonsoft.Json.JsonConvert.SerializeObject(await GetAllNonFollowers()));
                oIntent.PutExtra("currentauth", Newtonsoft.Json.JsonConvert.SerializeObject(CurrentAuth));
                oIntent.PutExtra("selected", EnumTypes.FollowsListSelectedTypes.NonFollowers.ToString());
                StartActivity(oIntent);
            }
            catch (Exception ex)
            {

                Log.Error("Instagram mainacc", ex.Message);
            }
        }

        private async void HandleMutualFriendsButtonClick(object sender, ResponsesEventArgs e)
        {
            try
            {
                var oIntent = new Intent(this, typeof(FollowsListActivity));
                oIntent.PutExtra("list", Newtonsoft.Json.JsonConvert.SerializeObject(await GetAllMutualFriends()));
                oIntent.PutExtra("currentauth", Newtonsoft.Json.JsonConvert.SerializeObject(CurrentAuth));
                oIntent.PutExtra("selected", EnumTypes.FollowsListSelectedTypes.MutualFriends.ToString());
                StartActivity(oIntent);
            }
            catch (Exception ex)
            {

                Log.Error("Instagram mainacc", ex.Message);
            }
        }

        private async void HandleFansButtonClick(object sender, ResponsesEventArgs e)
        {
            try
            {
                var oIntent = new Intent(this, typeof(FollowsListActivity));
                oIntent.PutExtra("list", Newtonsoft.Json.JsonConvert.SerializeObject(await GetAllFans()));
                oIntent.PutExtra("currentauth", Newtonsoft.Json.JsonConvert.SerializeObject(CurrentAuth));
                oIntent.PutExtra("selected", EnumTypes.FollowsListSelectedTypes.Fans.ToString());
                StartActivity(oIntent);
            }
            catch (Exception ex)
            {

                Log.Error("Instagram mainacc", ex.Message);
            }
        }

        private void HandleGainedFollowersButtonClick(object sender, ResponsesEventArgs e)
        {
            try
            {
                var oIntent = new Intent(this, typeof(FollowsListActivity));
                oIntent.PutExtra("list", Newtonsoft.Json.JsonConvert.SerializeObject(LoggedInUser.GainedFollowers.Select(selecting => selecting.LoggedInUserFollowedByUser)
                    .Select(select => new Helpers.Models.CustomUserInfo
                    {
                        Bio = null,
                        Counts = null,
                        FullName = select.FullName,
                        Id = select.Id,
                        IsFollowedByMe = false,
                        IsFollower = false,
                        ProfilePicture = select.ProfilePicture,
                        Username = select.Username,
                        Website = null
                    })));
                oIntent.PutExtra("currentauth", Newtonsoft.Json.JsonConvert.SerializeObject(CurrentAuth));
                oIntent.PutExtra("selected", EnumTypes.FollowsListSelectedTypes.GainedFollowers.ToString());
                StartActivity(oIntent);
            }
            catch (Exception ex)
            {

                Log.Error("Instagram mainacc", ex.Message);
            }
        }

        private async Task<List<InstaSharp.Models.User>> GetAllFollowings()
        {
            return await UserRelationships.FollowsAll();
        }

        private async Task<List<InstaSharp.Models.User>> GetAllFollowers()
        {
            return await UserRelationships.FollowedByAll();
        }

        private async Task<int> GetNonFollowersCount()
        {
            int counter = 0;

            try
            {
                if (TotalFollowings != null)
                {
                    foreach (var item in TotalFollowings)
                    {
                        relationshipsResults = await UserRelationships.Relationship(item.Id);
                        if (relationshipsResults.Data.OutgoingStatus == InstaSharp.Models.OutgoingStatus.Follows && relationshipsResults.Data.IncomingStatus == InstaSharp.Models.IncomingStatus.None)
                        {
                            counter++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Log.Error("Instafollow mainacc", ex.Message);
            }

            return counter;
        }

        private async Task<List<Helpers.Models.CustomUserInfo>> GetAllNonFollowers()
        {
            var nonFollowers = new List<Helpers.Models.CustomUserInfo>();

            try
            {
                if (TotalFollowings != null)
                {
                    foreach (var item in TotalFollowings)
                    {
                        relationshipsResults = await UserRelationships.Relationship(item.Id);
                        if (relationshipsResults.Data.OutgoingStatus == InstaSharp.Models.OutgoingStatus.Follows && relationshipsResults.Data.IncomingStatus == InstaSharp.Models.IncomingStatus.None)
                        {
                            nonFollowers.Add(new Helpers.Models.CustomUserInfo()
                            {
                                Bio = null,
                                Counts = null,
                                FullName = item.FullName,
                                Id = item.Id,
                                IsFollowedByMe = false,
                                IsFollower = false,
                                ProfilePicture = item.ProfilePicture,
                                Username = item.Username,
                                Website = null
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Log.Error("Instafollow mainacc", ex.Message);
            }

            return nonFollowers;
        }

        private async Task<int> GetMutualFriendsCount()
        {
            int counter = 0;

            try
            {
                if (TotalFollowings != null)
                {
                    foreach (var item in TotalFollowings)
                    {
                        relationshipsResults = await UserRelationships.Relationship(item.Id);
                        if (relationshipsResults.Data.OutgoingStatus == InstaSharp.Models.OutgoingStatus.Follows && relationshipsResults.Data.IncomingStatus == InstaSharp.Models.IncomingStatus.FollowedBy)
                        {
                            counter++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Log.Error("Instafollow mainacc", ex.Message);
            }
            return counter;
        }

        private async Task<List<Helpers.Models.CustomUserInfo>> GetAllMutualFriends()
        {
            var mutualsList = new List<Helpers.Models.CustomUserInfo>();

            try
            {
                if (TotalFollowings != null)
                {
                    foreach (var item in TotalFollowings)
                    {
                        relationshipsResults = await UserRelationships.Relationship(item.Id);
                        if (relationshipsResults.Data.OutgoingStatus == InstaSharp.Models.OutgoingStatus.Follows && relationshipsResults.Data.IncomingStatus == InstaSharp.Models.IncomingStatus.FollowedBy)
                        {
                            mutualsList.Add(new Helpers.Models.CustomUserInfo()
                            {
                                Bio = null,
                                Counts = null,
                                FullName = item.FullName,
                                Id = item.Id,
                                IsFollowedByMe = false,
                                IsFollower = false,
                                ProfilePicture = item.ProfilePicture,
                                Username = item.Username,
                                Website = null
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Log.Error("Instafollow mainacc", ex.Message);
            }
            return mutualsList;
        }

        private async Task<int> GetFansCount()
        {
            int counter = 0;

            try
            {
                if (TotalFollowers != null)
                {
                    foreach (var item in TotalFollowers)
                    {
                        relationshipsResults = await UserRelationships.Relationship(item.Id);
                        if ((relationshipsResults.Data.OutgoingStatus == InstaSharp.Models.OutgoingStatus.None || relationshipsResults.Data.IncomingStatus == InstaSharp.Models.IncomingStatus.RequestedBy)
                                && relationshipsResults.Data.IncomingStatus == InstaSharp.Models.IncomingStatus.FollowedBy)
                        {
                            counter++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Log.Error("Instafollow mainacc", ex.Message);
            }

            return counter;
        }

        private async Task<List<Helpers.Models.CustomUserInfo>> GetAllFans()
        {
            var fansList = new List<Helpers.Models.CustomUserInfo>();
            try
            {
                if (TotalFollowers != null)
                {
                    foreach (var item in TotalFollowers)
                    {
                        relationshipsResults = await UserRelationships.Relationship(item.Id);
                        if ((relationshipsResults.Data.OutgoingStatus == InstaSharp.Models.OutgoingStatus.None || relationshipsResults.Data.IncomingStatus == InstaSharp.Models.IncomingStatus.RequestedBy)
                                && relationshipsResults.Data.IncomingStatus == InstaSharp.Models.IncomingStatus.FollowedBy)
                        {
                            fansList.Add(new Helpers.Models.CustomUserInfo()
                            {
                                Bio = null,
                                Counts = null,
                                FullName = item.FullName,
                                Id = item.Id,
                                IsFollowedByMe = false,
                                IsFollower = false,
                                ProfilePicture = item.ProfilePicture,
                                Username = item.Username,
                                Website = null
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Log.Error("Instafollow mainacc", ex.Message);
            }

            return fansList;
        }

        private async Task<int> GetBlockedCount()
        {
            int counter = 0;
            RelationshipResponse result;

            try
            {
                if (TotalFollowers != null)
                {
                    foreach (var item in TotalFollowers)
                    {
                        result = await UserRelationships.Relationship(item.Id);
                        if (result.Data.IncomingStatus == InstaSharp.Models.IncomingStatus.BlockedbyYou)
                        {
                            counter++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Log.Error("Instafollow mainacc", ex.Message);
            }

            return counter;
        }
    }
}

