using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstaFollowModels.Core
{
    public class GainedFollower
    {
        public GainedFollower()
        {

        }

        [Key]
        public long FollowedLoggedInUserId { get; set; }

        [Key]
        public long LoggedInUserFollowedByUserId { get; set; }

        [ForeignKey("FollowedLoggedInUserId")]
        public LoggedInUsers FollowedLoggedUder { get; set; }

        [ForeignKey("LoggedInUserFollowedByUserId")]
        public User LoggedInUserFollowedByUser { get; set; }
    }
}
