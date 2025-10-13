using System.Text;

namespace Lexplosion.Logic.Management
{
    internal class RunCommandBuilder
    {
        private StringBuilder _jvmArguments = new();
        private StringBuilder _gameArguments = new();

        public void AddJvmArgs(string args)
        {
            AddArgs(args, _jvmArguments);
        }

        public void AddGameArgs(string args)
        {
            AddArgs(args, _gameArguments);
        }

        private void AddArgs(string args, StringBuilder builder)
        {
            if (string.IsNullOrEmpty(args)) return;

            builder.Append(args.Trim());
            builder.Append(' ');
        }

        public string Build(string mainClass)
        {
            var jvmArgs = _jvmArguments.ToString();
            var gameArgs = _gameArguments.ToString();

            return jvmArgs + mainClass + " " + gameArgs;
        }
    }
}
