using System;
using InstaSharp.Models.Responses;

namespace InstaFollow
{
    public class ResponsesEventArgs : EventArgs
    {
        public ResponsesEventArgs()
        {

        }

        public OAuthResponse OAuthResponse { get; set; }
    }
}