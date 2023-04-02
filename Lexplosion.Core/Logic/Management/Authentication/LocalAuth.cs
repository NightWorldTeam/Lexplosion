namespace Lexplosion.Logic.Management.Authentication
{
    internal class LocalAuth : IAuthHandler
    {
        public User Auth(ref string login, ref string accessData, out AuthCode code)
        {
            code = AuthCode.Successfully;
            return new User(login, "00000000-0000-0000-0000-000000000000", "null", null, AccountType.NoAuth, ActivityStatus.Offline);
        }

        public User ReAuth(ref string login, ref string accessData, out AuthCode code)
        {
            return Auth(ref login, ref accessData, out code);
        }
    }
}
