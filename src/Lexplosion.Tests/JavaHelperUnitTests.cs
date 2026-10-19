using Lexplosion.Core.Tools;

namespace Lexplosion.Tests
{
    [TestFixture]
    public class JavaPathTests
    {
        private string _root = string.Empty;
        private string _javaHome = string.Empty;
        private string _binDirectory = string.Empty;

        [SetUp]
        public void SetUp()
        {
            // No dependency on locally installed Java or the developer's drive letters.
            _root = Path.Combine(Path.GetTempPath(), "Lexplosion-JavaPathTests-" + Guid.NewGuid().ToString("N"));
            _javaHome = Path.Combine(_root, "Java with spaces", "jdk-17.0.1");
            _binDirectory = Path.Combine(_javaHome, "bin");

            Directory.CreateDirectory(_binDirectory);
            File.WriteAllText(Path.Combine(_binDirectory, "java.exe"), string.Empty);
            File.WriteAllText(Path.Combine(_binDirectory, "javaw.exe"), string.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        [TestCase("", "javaw.exe")]
        [TestCase("bin", "javaw.exe")]
        [TestCase("bin/javaw.exe", "javaw.exe")]
        [TestCase("bin/java.exe", "java.exe")]
        [TestCase("bin/javaw.exe/", "javaw.exe")]
        [TestCase("bin/java.exe/", "java.exe")]
        [TestCase("bin//java.exe/", "java.exe")]
        public void ExistingJavaPathIsAcceptedAndNormalized(string relativePath, string expectedExecutable)
        {
            string input = string.IsNullOrEmpty(relativePath)
                ? _javaHome
                : _javaHome + "\\" + relativePath.Replace('/', '\\');

            var status = JavaHelper.TryValidateJavaPath(input, out string normalized);

            Assert.That(status, Is.EqualTo(JavaHelper.JavaPathCheckResult.Success));
            Assert.That(normalized, Is.EqualTo(
                Path.Combine(_binDirectory, expectedExecutable).Replace('\\', '/')));
        }

        [Test]
        public void RepeatedBackslashesAreNormalized()
        {
            string input = _javaHome.Replace("\\", "\\\\") + "\\\\bin\\\\javaw.exe\\";

            var status = JavaHelper.TryValidateJavaPath(input, out string normalized);

            Assert.That(status, Is.EqualTo(JavaHelper.JavaPathCheckResult.Success));
            Assert.That(normalized, Is.EqualTo(Path.Combine(_binDirectory, "javaw.exe").Replace('\\', '/')));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void EmptyOrNullJavaPathIsRejected(string path)
        {
            var status = JavaHelper.TryValidateJavaPath(path, out string normalized);

            Assert.That(status, Is.EqualTo(JavaHelper.JavaPathCheckResult.EmptyOrNull));
            Assert.That(normalized, Is.Empty);
        }

        [Test]
        public void WrongExecutableNameIsRejected()
        {
            string invalidExecutable = Path.Combine(_binDirectory, "launcher.exe");
            File.WriteAllText(invalidExecutable, string.Empty);

            var status = JavaHelper.TryValidateJavaPath(invalidExecutable, out _);

            Assert.That(status, Is.EqualTo(JavaHelper.JavaPathCheckResult.WrongExe));
        }

        [Test]
        public void MissingJavaExecutableIsRejected()
        {
            File.Delete(Path.Combine(_binDirectory, "javaw.exe"));

            var status = JavaHelper.TryValidateJavaPath(_binDirectory, out _);

            Assert.That(status, Is.EqualTo(JavaHelper.JavaPathCheckResult.JaveExeDoesNotExists));
        }

        [Test]
        public void MissingJavaExeIsRejected()
        {
            File.Delete(Path.Combine(_binDirectory, "java.exe"));

            var status = JavaHelper.TryValidateJavaPath(Path.Combine(_binDirectory, "java.exe"), out _);

            Assert.That(status, Is.EqualTo(JavaHelper.JavaPathCheckResult.JaveExeDoesNotExists));
        }

        [Test]
        public void NonexistentDirectoryIsRejected()
        {
            string nonexistentPath = Path.Combine(_root, "does-not-exist", "bin");

            var status = JavaHelper.TryValidateJavaPath(nonexistentPath, out _);

            Assert.That(status, Is.EqualTo(JavaHelper.JavaPathCheckResult.PathDoesNotExists));
        }
    }
}
