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

namespace InstaFollow
{
    public static class EnumTypes
    {
        public enum FollowsListSelectedTypes
        {
            Following,
            Followers,
            Blockers,
            GainedFollowers,
            LostFollowers,
            NonFollowers,
            MutualFriends,
            Fans
        }
    }
}