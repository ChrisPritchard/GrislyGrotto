namespace GrislyGrotto.Core.Entities
{
    public class User
    {
        public string FullName { get; set; }

        public string LoginName { get; set; }
        public string LoginPassword { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (User)) return false;
            return Equals((User) obj);
        }

        public bool Equals(User other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.FullName, FullName) && Equals(other.LoginName, LoginName) && Equals(other.LoginPassword, LoginPassword);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = (FullName != null ? FullName.GetHashCode() : 0);
                result = (result*397) ^ (LoginName != null ? LoginName.GetHashCode() : 0);
                result = (result*397) ^ (LoginPassword != null ? LoginPassword.GetHashCode() : 0);
                return result;
            }
        }
    }
}