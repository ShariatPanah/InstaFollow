using System;
using Android.OS;
using Android.Widget;
using Android.Graphics;

namespace InstaFollow.Helpers
{
    public class DownloadImageTask : AsyncTask<String, string, Bitmap>
    {
        ImageView bmImage;

        public DownloadImageTask(ImageView bmImage)
        {
            this.bmImage = bmImage;
        }

        protected override void OnPostExecute(Bitmap result)
        {
            bmImage.SetImageBitmap(result);
        }

        protected override Bitmap RunInBackground(params string[] @params)
        {
            Bitmap mIcon11 = null;
            try
            {
                System.IO.Stream input = new Java.Net.URL(@params[0]).OpenStream();
                mIcon11 = BitmapFactory.DecodeStream(input);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("AccountsListAdapter Error", "Encountered an error attempting to download profile picture: {0}", ex.Message);
            }
            return mIcon11;
        }
    }
}
