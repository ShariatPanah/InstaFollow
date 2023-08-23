using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstaFollowModels.Core
{
    public class LoggedInUsers
    {
        public LoggedInUsers()
        {
            Followers = new List<Follower>();
            Followings = new List<Following>();
            LostFollowers = new List<LostFollower>();
            GainedFollowers = new List<GainedFollower>();
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        [MaxLength(50)]
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the access_ token.
        /// </summary>
        /// <value>
        /// The access_ token.
        /// </value>
        [MaxLength(70)]
        public string AccessToken { get; set; }

        public virtual IList<Following> Followings { get; set; }
        public virtual IList<Follower> Followers { get; set; }
        public virtual IList<GainedFollower> GainedFollowers { get; set; }
        public virtual IList<LostFollower> LostFollowers { get; set; }
    }
}
