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

namespace InstaFollow.Helpers.Models
{
    public class CustomUserInfo : InstaSharp.Models.UserInfo
    {
        /// <summary>
        /// Gets or sets the bio.
        /// </summary>
        /// <value>
        /// The bio.
        /// </value>
        public string Bio { get; set; }
        /// <summary>
        /// Gets or sets the website.
        /// </summary>
        /// <value>
        /// The website.
        /// </value>
        public string Website { get; set; }
        /// <summary>
        /// Gets or sets the counts.
        /// </summary>
        /// <value>
        /// The counts.
        /// </value>
        public Count Counts { get; set; }

        /////////////////////// Custom properties //////////////////////////////
        public bool IsFollower { get; set; }

        public bool IsFollowedByMe { get; set; }
    }
}