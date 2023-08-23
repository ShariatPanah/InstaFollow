using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstaFollowModels.Core
{
    public class Following
    {
        public Following()
        {

        }

        /// <summary>
        /// آیدی کاربری که لاگین کرده
        /// </summary>
        [Key]
        public long FollowedByLoggedInUserId { get; set; }

        /// <summary>
        /// آیدی یوزری که کاربر لاگین شده، اون رو فالو کرده
        /// </summary>
        [Key]
        public long FollowedUserId { get; set; }

        [ForeignKey("FollowedByLoggedInUserId")]
        public LoggedInUsers FollowedByLoggedInUser { get; set; }

        [ForeignKey("FollowedUserId")]
        public User FollowedUser { get; set; }
    }
}
