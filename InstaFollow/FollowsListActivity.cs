using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using InstaSharp.Models.Responses;
using InstaSharp.Models;
using InstaSharp;
using Android.Util;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Android.Views;

namespace InstaFollow
{
    [Activity(Label = "FollowsListActivity")]
    public class FollowsListActivity : Activity
    {
        private List<Helpers.Models.CustomUserInfo> UsersList;
        private OAuthResponse CurrentAuth;
        private EnumTypes.FollowsListSelectedTypes SelectedType;
        private ListView lstFollowsList;
        private TextView txtFollowsLayoutTitle;
        private InstagramConfig Config;
        private InstaSharp.Endpoints.Relationships relationships;
        private ProgressDialog progressing;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            InitializeComponent();
            relationships = new InstaSharp.Endpoints.Relationships(Config, CurrentAuth);

            base.OnCreate(savedInstanceState);

            //progressing = new ProgressDialog(this);
            //progressing.RequestWindowFeature((int)WindowFeatures.NoTitle);
            //progressing.SetMessage("منتظر بمانید...");
            //progressing.Show();

            // Create your application here
            SetContentView(Resource.Layout.FollowsListLayout);

            lstFollowsList = FindViewById<ListView>(Resource.Id.lstFollows);
            txtFollowsLayoutTitle = FindViewById<TextView>(Resource.Id.txtFollowsLayoutTitle);

            GetPassedValues();
            UpdateView();

            //BackgroundWorker oWorker = new BackgroundWorker();
            //oWorker.DoWork += OWorker_DoWork;
            //oWorker.RunWorkerCompleted += OWorker_RunWorkerCompleted;
            //oWorker.RunWorkerAsync();
        }

        private void OWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressing.Dismiss();
        }

        private void OWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            GetPassedValues();
            UpdateView();
        }

        private void InitializeComponent()
        {
            CurrentAuth = Newtonsoft.Json.JsonConvert.DeserializeObject<OAuthResponse>(Intent.GetStringExtra("currentauth"));

            Config = new InstagramConfig()
            {
                ClientId = Resources.GetString(InstaFollow.Resource.String.InstaClientId),
                RedirectUri = Resources.GetString(InstaFollow.Resource.String.InstaRedirectURI),
                ClientSecret = Resources.GetString(InstaFollow.Resource.String.InstaClientSecret),
                CallbackUri = "",
            };
        }

        private void UpdateView()
        {
            try
            {
                var FollowsListAdapter = new FollowsListAdapter(this, UsersList, CurrentAuth, SelectedType);
                lstFollowsList.Adapter = FollowsListAdapter;
            }
            catch (Exception ex)
            {

                Log.Error("Instagram Followslist", ex.Message);
            }
        }

        private void GetPassedValues()
        {
            try
            {
                UsersList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Helpers.Models.CustomUserInfo>>(Intent.GetStringExtra("list"));
                Nito.AsyncEx.AsyncContext.Run(() => StartParallelWorksAsync());
                ////StartParallelWorksAsync();

                switch (Intent.GetStringExtra("selected"))
                {
                    case "Following":
                        SelectedType = EnumTypes.FollowsListSelectedTypes.Following;
                        txtFollowsLayoutTitle.Text = "فالو شده ها";
                        break;

                    case "Followers":
                        SelectedType = EnumTypes.FollowsListSelectedTypes.Followers;
                        txtFollowsLayoutTitle.Text = "فالوور ها";
                        break;

                    case "Blockers":
                        SelectedType = EnumTypes.FollowsListSelectedTypes.Blockers;
                        txtFollowsLayoutTitle.Text = "بلاک کننده ها";
                        break;

                    case "GainedFollowers":
                        SelectedType = EnumTypes.FollowsListSelectedTypes.GainedFollowers;
                        txtFollowsLayoutTitle.Text = "فالوور های بدست آمده";
                        break;

                    case "LostFollowers":
                        SelectedType = EnumTypes.FollowsListSelectedTypes.LostFollowers;
                        txtFollowsLayoutTitle.Text = "فالوور های از دست رفته";
                        break;

                    case "NonFollowers":
                        SelectedType = EnumTypes.FollowsListSelectedTypes.NonFollowers;
                        txtFollowsLayoutTitle.Text = "غیر فالوور ها";
                        break;

                    case "MutualFriends":
                        SelectedType = EnumTypes.FollowsListSelectedTypes.MutualFriends;
                        txtFollowsLayoutTitle.Text = "دوستان مشترک";
                        break;

                    case "Fans":
                        SelectedType = EnumTypes.FollowsListSelectedTypes.Fans;
                        txtFollowsLayoutTitle.Text = "طرفدار ها";
                        break;

                    default:
                        SelectedType = EnumTypes.FollowsListSelectedTypes.Following;
                        txtFollowsLayoutTitle.Text = "فالو شده ها";
                        break;
                }
            }
            catch (Exception ex)
            {

                Log.Error("Instagram Followslist", ex.Message);
            }
        }

        public void StartParallelWorksAsync()
        {
            try
            {
                var RetrieveDetails = new ActionBlock<int>(async index =>
                {
                    UsersList[index].IsFollower = await CheckFollowerStatus(UsersList[index].Id);
                    UsersList[index].IsFollowedByMe = await CheckFollowingStatus(UsersList[index].Id);
                }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded });

                for (int i = 0; i < UsersList.Count; i++)
                {
                    // Post values to the block.
                    RetrieveDetails.Post(i);
                }

                // Wait for completion in a try/catch block.
                try
                {
                    RetrieveDetails.Complete();
                    RetrieveDetails.Completion.Wait();
                }
                catch (AggregateException ae)
                {
                    // If an unhandled exception occurs during dataflow processing, all
                    // exceptions are propagated through an AggregateException object.
                    ae.Handle(e =>
                    {
                        Log.Error("Instagram Followslist", "Encountered {0}: {1}",
                           e.GetType().Name, e.Message);
                        return true;
                    });
                }

                //Parallel.For(0, UsersList.Count,
                //    async index =>
                //    {
                //        UsersList[index].IsFollower = await CheckFollowerStatus(UsersList[index].Id);
                //        UsersList[index].IsFollowedByMe = await CheckFollowingStatus(UsersList[index].Id);
                //    });

                //for (int index = 0; index < UsersList.Count; index++)
                //{
                //    UsersList[index].IsFollower = await CheckFollowerStatus(UsersList[index].Id);
                //    UsersList[index].IsFollowedByMe = await CheckFollowingStatus(UsersList[index].Id);
                //}
            }
            catch (Exception ex)
            {
                Log.Error("Instagram Followslist", ex.Message);
            }
        }

        public async Task<bool> CheckFollowerStatus(long userId)
        {
            var result = await relationships.Relationship(userId);
            if (result.Data.IncomingStatus == IncomingStatus.FollowedBy)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> CheckFollowingStatus(long userId)
        {
            var result = await relationships.Relationship(userId);
            if (result.Data.OutgoingStatus == OutgoingStatus.Follows)
            {
                return true;
            }

            return false;
        }

        public void HandleOpenMiniProfileClick(User selectedUser)
        {
            var oIntent = new Intent(this, typeof(MiniProfileActivity));
            oIntent.PutExtra("selectedUser", Newtonsoft.Json.JsonConvert.SerializeObject(selectedUser));
            oIntent.PutExtra("currentauth", Newtonsoft.Json.JsonConvert.SerializeObject(CurrentAuth));
            StartActivity(oIntent);
        }
    }
}