namespace Lexplosion.UI.WPF.Core.Objects.Users
{
    public class NightWorldUserStatus
    {
        public ActivityStatus Priority { get; set; }
        public string Value { get; set; }

        public NightWorldUserStatus(ActivityStatus activityStatus)
        {
            Priority = activityStatus;
            Value = activityStatus.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is NightWorldUserStatus))
                return false;


            var priority = (obj as NightWorldUserStatus).Priority;

            return Priority == priority;
        }

        public override int GetHashCode()
        {
            return Priority.GetHashCode();
        }
    }
}
