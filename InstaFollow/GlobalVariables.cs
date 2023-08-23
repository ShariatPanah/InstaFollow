using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InstaSharp.Models.Responses;
using InstaSharp;
using Android.Content.Res;
using System.IO;

public static class GlobalVariables
{
    static GlobalVariables()
    {

    }

    //public static InstagramConfig Config = new InstagramConfig()
    //{
    //    ClientId = Resources.System.GetString(InstaFollow.Resource.String.InstaClientId),
    //    RedirectUri = Resources.System.GetString(InstaFollow.Resource.String.InstaRedirectURI),
    //    ClientSecret = Resources.System.GetString(InstaFollow.Resource.String.InstaClientSecret),
    //    CallbackUri = "",
    //};

    public static string GetDbPath()
    {
        var dbFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        var fileName = "InstaFollow.db";
        return Path.Combine(dbFolder, fileName);
    }
}