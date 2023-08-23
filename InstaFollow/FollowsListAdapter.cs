using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Views;
using Android.Widget;
using InstaSharp.Models.Responses;
using InstaFollow.Helpers;
using InstaSharp.Models;
using System.Threading.Tasks;
using InstaSharp;
using Nito.AsyncEx;
using Android.Util;

namespace InstaFollow
{
    public class FollowsListAdapter : BaseAdapter<Helpers.Models.CustomUserInfo>
    {
        /// <summary>
        /// base fields
        /// </summary>
        private List<Helpers.Models.CustomUserInfo> _usersList = null;
        private Activity _activity = null;
        private OAuthResponse CurrentAuthResponse;
        private InstagramConfig Config;
        private InstaSharp.Endpoints.Relationships relationships;
        private EnumTypes.FollowsListSelectedTypes SelectedType;
        private Button btnFollowsListDoStuff;
        private Helpers.CustomAlertDialog oDialog;

        public FollowsListAdapter(Activity activity, List<Helpers.Models.CustomUserInfo> users, OAuthResponse currentAuthResponse, EnumTypes.FollowsListSelectedTypes selectedType)
        {
            this._activity = activity;
            this._usersList = users;

            this.CurrentAuthResponse = currentAuthResponse;
            this.SelectedType = selectedType;
            Config = new InstagramConfig()
            {
                ClientId = _activity.Resources.GetString(InstaFollow.Resource.String.InstaClientId),
                RedirectUri = _activity.Resources.GetString(InstaFollow.Resource.String.InstaRedirectURI),
                ClientSecret = _activity.Resources.GetString(InstaFollow.Resource.String.InstaClientSecret),
                CallbackUri = "",
            };

            relationships = new InstaSharp.Endpoints.Relationships(Config, CurrentAuthResponse);

            //oDialog = new CustomAlertDialog(_activity, string.Format("شما ظرف مدت یک ساعت، تعداد {0} نفر را آنفالو کرده اید. پیشنهاد می شود جهت جلوگیری از بلاک شدن توسط اینستاگرام در هر ساعت حداکثر 100 نفر را آنفالو کنید.", MainActivity.UnfollowedCount));
            oDialog = new CustomAlertDialog(_activity, string.Format("شما ظرف مدت یک ساعت، تعداد 99 نفر را آنفالو کرده اید. پیشنهاد می شود جهت جلوگیری از بلاک شدن توسط اینستاگرام در هر ساعت حداکثر 100 نفر را آنفالو کنید."));
        }

        public override Helpers.Models.CustomUserInfo this[int position]
        {
            get
            {
                return _usersList[position];
            }
        }

        public override int Count
        {
            get
            {
                return _usersList.Count;
            }
        }

        public override long GetItemId(int position)
        {
            return _usersList[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ViewHolder oHolder = null;
            try
            {
                if (convertView == null)
                {
                    convertView = _activity.LayoutInflater.Inflate(Resource.Layout.FollowsListView, parent, false);
                }
                else
                {
                    oHolder = convertView.Tag as ViewHolder;
                }

                if (oHolder == null)
                {
                    oHolder = new ViewHolder();
                    oHolder.txtFollowListUsername = convertView.FindViewById<TextView>(Resource.Id.txtFollowListUsername);
                    oHolder.txtFollowListFullName = convertView.FindViewById<TextView>(Resource.Id.txtFollowListFullName);
                    oHolder.txtFollowListFollowingStatus = convertView.FindViewById<TextView>(Resource.Id.txtFollowListFollowingStatus);
                    oHolder.imgFollowListProfilePicture = convertView.FindViewById<ImageView>(Resource.Id.imgFollowListProfilePicture);
                    oHolder.btnFollowsListDoStuff = convertView.FindViewById<Button>(Resource.Id.btnFollowsListDoStuff);
                    oHolder.btnFollowListOpenMiniProfile = convertView.FindViewById<RelativeLayout>(Resource.Id.btnFollowListOpenMiniProfile);

                    convertView.Tag = oHolder;
                }
                btnFollowsListDoStuff = oHolder.btnFollowsListDoStuff;

                if (!oHolder.btnFollowListOpenMiniProfile.HasOnClickListeners)
                {
                    oHolder.btnFollowListOpenMiniProfile.Click += delegate
                    {
                        ((FollowsListActivity)_activity).HandleOpenMiniProfileClick(
                            _usersList.Where(user=>user.Id == _usersList[position].Id)
                            .Select(selecting => new User()
                            {
                                Id = selecting.Id,
                                Bio = selecting.Bio,
                                Counts = selecting.Counts,
                                FullName = selecting.FullName,
                                ProfilePicture = selecting.ProfilePicture,
                                Username = selecting.Username,
                                Website = selecting.Website
                            })
                            .FirstOrDefault());
                    };
                }

                if (!oHolder.btnFollowsListDoStuff.HasOnClickListeners)
                {
                    oHolder.btnFollowsListDoStuff.Click += delegate
                    {
                        try
                        {
                            if (oHolder.btnFollowsListDoStuff.Text == "آنفالو")
                            {
                                var unfollow = AsyncContext.Run(() => UnfollowUser(_usersList[position].Id));

                                if (unfollow.Data.OutgoingStatus == OutgoingStatus.None)
                                {
                                    if (DateTime.Now >= MainActivity.StartUnfollowingTime.AddHours(1))
                                    {
                                        MainActivity.UnfollowedCount = 0;
                                        MainActivity.StartUnfollowingTime = DateTime.Now;
                                    }
                                    else if (DateTime.Now < MainActivity.StartUnfollowingTime.AddHours(1) && MainActivity.UnfollowedCount >= 99)
                                    {
                                        oDialog.Show();

                                        MainActivity.UnfollowedCount = 0;
                                    }

                                    MainActivity.UnfollowedCount++;
                                    MainActivity.Session.StoreUnfollowedCount(CurrentAuthResponse, MainActivity.UnfollowedCount, MainActivity.StartUnfollowingTime);

                                    oHolder.btnFollowsListDoStuff.Text = "آنفالو شد";
                                    oHolder.btnFollowsListDoStuff.SetTextColor(_activity.Resources.GetColor(Resource.Color.BlueFollow));
                                    oHolder.btnFollowsListDoStuff.SetBackgroundResource(Resource.Drawable.CustomBorderFollowsListBlue);
                                }
                                else
                                {
                                    Toast.MakeText(_activity, "هنگام آنفالو خطایی رخ داده، لطفا مجددا اقدام نمایید.", ToastLength.Long).Show();
                                }
                            }
                            else if (oHolder.btnFollowsListDoStuff.Text == "فالو")
                            {
                                var follow = AsyncContext.Run(() => FollowUser(_usersList[position].Id));

                                if (follow.Data.OutgoingStatus == OutgoingStatus.Follows)
                                {
                                    oHolder.btnFollowsListDoStuff.Text = "فالو شد";
                                }
                                else if (follow.Data.OutgoingStatus == OutgoingStatus.Requested)
                                {
                                    oHolder.btnFollowsListDoStuff.Text = "درخواست شد";
                                    oHolder.btnFollowsListDoStuff.TextSize = 12;
                                }
                                else
                                {
                                    Toast.MakeText(_activity, "هنگام فالو خطایی رخ داده، لطفا مجددا اقدام نمایید.", ToastLength.Long).Show();
                                }
                            }
                            else if (oHolder.btnFollowsListDoStuff.Text == "آنفالو شد" || oHolder.btnFollowsListDoStuff.Text == "فالو شد")
                            {

                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Instagram FollowsList", ex.Message);
                        }
                    };
                }

                //_usersList[position].IsFollower = AsyncContext.Run(() => CheckFollowerStatus(_usersList[position].Id));
                //_usersList[position].IsFollowedByMe = AsyncContext.Run(() => CheckFollowingStatus(_usersList[position].Id));

                oHolder.txtFollowListUsername.Text = _usersList[position].Username.ToUpper();
                oHolder.txtFollowListFullName.Text = _usersList[position].FullName;
                oHolder.txtFollowListFollowingStatus.Text = (_usersList[position].IsFollower ? "فالوور" : "غیر فالوور");
                oHolder.txtFollowListFollowingStatus.SetTextColor((_usersList[position].IsFollower ? _activity.Resources.GetColor(Resource.Color.BlueFollow) : _activity.Resources.GetColor(Resource.Color.RedUnfollow)));

                SetProperColorsOfViews(_usersList[position].IsFollowedByMe);

                new DownloadImageTask(oHolder.imgFollowListProfilePicture).Execute(_usersList[position].ProfilePicture);
            }
            catch (Exception ex)
            {

                Android.Util.Log.Error("AccountsListAdapter Error", "Encountered an error attempting to load profile infos: {0}", ex.Message);
            }

            return convertView;
        }

        public void SetProperColorsOfViews(bool IsFollowedByMe)
        {
            switch (SelectedType)
            {
                case EnumTypes.FollowsListSelectedTypes.Following:
                case EnumTypes.FollowsListSelectedTypes.Followers:
                case EnumTypes.FollowsListSelectedTypes.GainedFollowers:
                case EnumTypes.FollowsListSelectedTypes.LostFollowers:
                case EnumTypes.FollowsListSelectedTypes.NonFollowers:
                case EnumTypes.FollowsListSelectedTypes.Blockers:
                case EnumTypes.FollowsListSelectedTypes.MutualFriends:
                case EnumTypes.FollowsListSelectedTypes.Fans:
                    //IsFollowedByMe = AsyncContext.Run(() => CheckFollowingStatus(id));

                    if (IsFollowedByMe)
                    {
                        btnFollowsListDoStuff.Text = "آنفالو";
                        btnFollowsListDoStuff.SetTextColor(_activity.Resources.GetColor(Resource.Color.RedUnfollow));
                        btnFollowsListDoStuff.SetBackgroundResource(Resource.Drawable.CustomBorderFollowsListRed);
                    }
                    else
                    {
                        btnFollowsListDoStuff.Text = "فالو";
                        btnFollowsListDoStuff.SetTextColor(_activity.Resources.GetColor(Resource.Color.BlueFollow));
                        btnFollowsListDoStuff.SetBackgroundResource(Resource.Drawable.CustomBorderFollowsListBlue);
                    }
                    break;

                default:
                    break;
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

        public async Task<RelationshipResponse> UnfollowUser(long userId)
        {
            return await relationships.Relationship(userId, InstaSharp.Endpoints.Relationships.Action.Unfollow);
        }

        public async Task<RelationshipResponse> FollowUser(long userId)
        {
            return await relationships.Relationship(userId, InstaSharp.Endpoints.Relationships.Action.Follow);
        }

        private class ViewHolder : Java.Lang.Object
        {
            public TextView txtFollowListUsername;
            public TextView txtFollowListFullName;
            public TextView txtFollowListFollowingStatus;
            public ImageView imgFollowListProfilePicture;
            public Button btnFollowsListDoStuff;
            public RelativeLayout btnFollowListOpenMiniProfile;

            public ViewHolder()
            {

            }
        }
    }
}