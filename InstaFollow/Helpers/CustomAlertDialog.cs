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

namespace InstaFollow.Helpers
{
    public class CustomAlertDialog : Dialog
    {
        private Activity _acitivity;
        private string _message;
        private TextView txtMessage;
        private LinearLayout btnOK;

        public CustomAlertDialog(Activity activity, string message):base(activity)
        {
            this._acitivity = activity;
            this._message = message;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestWindowFeature((int)WindowFeatures.NoTitle);
            this.Window.SetBackgroundDrawableResource(Resource.Color.MyApplicationBlackColor);
            SetContentView(Resource.Layout.CustomDialogLayout);
            btnOK = FindViewById<LinearLayout>(Resource.Id.btnOK);
            txtMessage = FindViewById<TextView>(Resource.Id.txtMessage);
            txtMessage.Text = _message;
            btnOK.Click += BtnOK_Click;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            this.Dismiss();
        }
    }
}