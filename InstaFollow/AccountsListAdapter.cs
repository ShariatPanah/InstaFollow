using System;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using InstaSharp.Models.Responses;
using InstaFollow.Helpers;

namespace InstaFollow
{
    public class AccountsListAdapter : BaseAdapter<OAuthResponse>
    {
        private List<OAuthResponse> _responsesList = null;
        private Activity _activity = null;

        public AccountsListAdapter(Activity myActivity, List<OAuthResponse> list)
        {
            this._activity = myActivity;
            this._responsesList = list;
        }

        public override OAuthResponse this[int position]
        {
            get
            {
                return _responsesList[position];
            }
        }

        public override int Count
        {
            get
            {
                return _responsesList.Count;
            }
        }

        public override long GetItemId(int position)
        {
            return _responsesList[position].User.Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            try
            {
                if (convertView == null)
                {
                    convertView = _activity.LayoutInflater.Inflate(Resource.Layout.AccountsListView, parent, false);
                }

                var txtAccountUsername = convertView.FindViewById<TextView>(Resource.Id.txtAccountUsername);
                var txtAccountFullName = convertView.FindViewById<TextView>(Resource.Id.txtAccountFullName);
                var imgAccountProfilePicture = convertView.FindViewById<ImageView>(Resource.Id.imgAccountProfilePicture);
                var btnDeleteRow = convertView.FindViewById<ImageButton>(Resource.Id.btnAccountDelete);
                var customListViewAccounts = convertView.FindViewById<LinearLayout>(Resource.Id.customListViewAccounts);

                if (!btnDeleteRow.HasOnClickListeners)
                {
                    btnDeleteRow.Click += delegate
                    {
                        ((AccountsActivity)_activity).HandleDeleteButtonClick(typeof(ImageView), new ResponsesEventArgs() { OAuthResponse = _responsesList[position] });
                    };
                }

                if (!customListViewAccounts.HasOnClickListeners)
                {
                    customListViewAccounts.Click += delegate
                    {
                        ((AccountsActivity)_activity).HandleCustomLayoutClick(typeof(ImageView), new ResponsesEventArgs() { OAuthResponse = _responsesList[position] });
                    };
                }

                txtAccountUsername.Text = _responsesList[position].User.Username.ToUpper();
                txtAccountFullName.Text = _responsesList[position].User.FullName;

                new DownloadImageTask(imgAccountProfilePicture).Execute(_responsesList[position].User.ProfilePicture);
            }
            catch (Exception ex)
            {

                Android.Util.Log.Error("AccountsListAdapter Error", "Encountered an error attempting to load profile infos: {0}", ex.Message);
            }

            return convertView;
        }
    }
}