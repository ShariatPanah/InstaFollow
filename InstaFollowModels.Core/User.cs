using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstaFollowModels.Core
{
    public class User : IEquatable<User>
    {
        public User()
        {
            FollowedByLoggedInUsers = new List<Following>();
            FollowingLoggedInUsers = new List<Follower>();
            UnfollowedLoggedInUsers = new List<LostFollower>();
            GainedFollowingLoggedInUsers = new List<GainedFollower>();
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
        /// Gets or sets the full name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        [MaxLength(50)]
        public string FullName { get; set; }
        /// <summary>
        /// Gets or sets the profile picture.
        /// </summary>
        /// <value>
        /// The profile picture.
        /// </value>
        [MaxLength(160)]
        public string ProfilePicture { get; set; }

        /// <summary>
        /// از طرف کاربر لاگین شده، فالو شده است، یعنی فالووینگ های کاربر لاگین شده
        /// </summary>
        public virtual IList<Following> FollowedByLoggedInUsers { get; set; }
        /// <summary>
        /// کاربر لاگین شده رو فالو کرده، یعنی فالوور برای کاربر لاگین شده
        /// </summary>
        public virtual IList<Follower> FollowingLoggedInUsers { get; set; }
        /// <summary>
        /// کاربر لاگین شده رو انفالو کرده، یعنی لاست فالوور برای کاربر لاگین شده
        /// </summary>
        public virtual IList<LostFollower> UnfollowedLoggedInUsers { get; set; }
        /// <summary>
        /// کاربر لاگین شده رو فالو کرده، یعنی فالوور بدست امده برای کاربر لاگین شده
        /// </summary>
        public virtual IList<GainedFollower> GainedFollowingLoggedInUsers { get; set; }


        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(User other)
        {
            //Check whether the objects are the same object.
            if (Object.ReferenceEquals(other, null))
            {
                return false;
            }

            // Check for same reference
            if (ReferenceEquals(this, other))
                return true;

            //Check whether the users' properties are equal.
            return (other.Id == this.Id && other.Username == this.Username);
        }

        public static bool operator ==(User user1, User user2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(user1, user2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)user1 == null) || ((object)user2 == null))
            {
                return false;
            }

            return user1.Equals(user2);
        }

        public static bool operator !=(User user1, User user2)
        {
            return !(user1 == user2);
        }

        public override int GetHashCode()
        {
            //Get hash code for the Name field if it is not null. 
            int hashProductName = this.Username == null ? 0 : this.Username.GetHashCode();

            //Get hash code for the Code field. 
            int hashProductCode = this.Id.GetHashCode();

            //Calculate the hash code for the product. 
            return hashProductName ^ hashProductCode;
        }
    }
}
