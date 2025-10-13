using Lexplosion.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lexplosion.Tests
{
    [TestClass]
    public class JavaPathTests
    {
        [TestCase("C:\\Program Files\\Java\\jdk-17.0.1\\bin", "C:/Program Files/Java/jdk-17.0.1/bin/javaw.exe")]
        [TestCase("C:\\Program Files\\Java\\jdk-17.0.1\\bin\\javaw.exe", "C:/Program Files/Java/jdk-17.0.1/bin/javaw.exe")]
        [TestCase("C:\\Program Files\\Java\\jdk-17.0.1\\bin\\java.exe", "C:/Program Files/Java/jdk-17.0.1/bin/java.exe")]
        [TestCase("C:\\Program Files\\Java\\jdk-17.0.1\\bin\\javaw.exe\\", "C:/Program Files/Java/jdk-17.0.1/bin/javaw.exe")]
        [TestCase("C:\\Program Files\\Java\\jdk-17.0.1\\bin\\java.exe\\", "C:/Program Files/Java/jdk-17.0.1/bin/java.exe")]
        [TestCase("C:\\\\Program Files\\\\Java\\\\jdk-17.0.1\\\\bin\\\\java.exe\\", "C:/Program Files/Java/jdk-17.0.1/bin/java.exe")]
        public void SuccessPathReformat(string exceptedPath, string exceptedCurrentPath)
        {
            JavaHelper.TryValidateJavaPath(exceptedPath, out string result);
            NUnit.Framework.Assert.AreEqual(exceptedCurrentPath, result);
        }

        [TestCase("C:\\Program Files\\Java\\jdk-17.0.1\\bin", JavaHelper.JavaPathCheckResult.Success)]
        [TestCase("C:\\Program Files\\Java\\jdk-17.0.1\\bin\\javaw.exe", JavaHelper.JavaPathCheckResult.Success)]
        [TestCase("C:\\Program Files\\Java\\jdk-17.0.1\\bin\\java.exe", JavaHelper.JavaPathCheckResult.Success)]
        [TestCase("C:\\Program Files\\Java\\jdk-17.0.1\\bin\\javaw.exe\\", JavaHelper.JavaPathCheckResult.Success)]
        [TestCase("C:\\Program Files\\Java\\jdk-17.0.1\\bin\\java.exe\\", JavaHelper.JavaPathCheckResult.Success)]
        public void SuccessJavaPath(string exceptedPath, JavaHelper.JavaPathCheckResult exceptedCurrentPath)
        {
            var result = JavaHelper.TryValidateJavaPath(exceptedPath, out var s);
            NUnit.Framework.Assert.AreEqual(JavaHelper.JavaPathCheckResult.Success, result);
        }

        [TestCase("", JavaHelper.JavaPathCheckResult.EmptyOrNull)]
        [TestCase(null, JavaHelper.JavaPathCheckResult.EmptyOrNull)]
        [TestCase("       ", JavaHelper.JavaPathCheckResult.EmptyOrNull)]
        public void EmptyOrNullJavaPath(string exceptedPath, JavaHelper.JavaPathCheckResult exceptedCurrentPath)
        {
            var result = JavaHelper.TryValidateJavaPath(exceptedPath, out var s);
            NUnit.Framework.Assert.AreEqual(exceptedCurrentPath, result);
        }

        [TestCase("N:\\Lexplosion\\Lexplosion.WPF.NewInterface\\bin\\Debug\\Lexplosion.WPF.NewInterface.exe", JavaHelper.JavaPathCheckResult.WrongExe)]
        public void WrongExeJavaPath(string exceptedPath, JavaHelper.JavaPathCheckResult exceptedCurrentPath)
        {
            var result = JavaHelper.TryValidateJavaPath(exceptedPath, out var s);
            NUnit.Framework.Assert.AreEqual(JavaHelper.JavaPathCheckResult.WrongExe, result);
        }

        [TestCase("N:\\Lexplosion\\Lexplosion.WPF.NewInterface\\bin\\Debug\\", JavaHelper.JavaPathCheckResult.JaveExeDoesNotExists)]
        public void JaveExeDoesNotExistsJavaPath(string exceptedPath, JavaHelper.JavaPathCheckResult exceptedCurrentPath)
        {
            var result = JavaHelper.TryValidateJavaPath(exceptedPath, out var s);
            NUnit.Framework.Assert.AreEqual(JavaHelper.JavaPathCheckResult.JaveExeDoesNotExists, result);
        }

        [TestCase("N:\\Lexplosion\\Lexplosion.WPF.NewInterfaceasdjansd\\asdnnasdbin\\Debug\\", JavaHelper.JavaPathCheckResult.PathDoesNotExists)]
        public void PathDoesNotExistsJavaPath(string exceptedPath, JavaHelper.JavaPathCheckResult exceptedCurrentPath)
        {
            var result = JavaHelper.TryValidateJavaPath(exceptedPath, out var s);
            NUnit.Framework.Assert.AreEqual(JavaHelper.JavaPathCheckResult.PathDoesNotExists, result);
        }
    }
}