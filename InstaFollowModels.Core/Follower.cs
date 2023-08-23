using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstaFollowModels.Core
{
    public class Follower
    {
        public Follower()
        {

        }

        /// <summary>
        /// آیدی کاربری که لاگین شده
        /// </summary>
        [Key]
        public long FollowedLoggedInUserId { get; set; }
        /// <summary>
        /// آیدی یوزری که کاربر لاگین شده رو فالوو کرده
        /// </summary>
        [Key]
        public long LoggedInUserFollowedByUserId { get; set; }

        [ForeignKey("FollowedLoggedInUserId")]
        public LoggedInUsers FollowedLoggedUder { get; set; }

        [ForeignKey("LoggedInUserFollowedByUserId")]
        public User LoggedInUserFollowedByUser { get; set; }
    }
}
