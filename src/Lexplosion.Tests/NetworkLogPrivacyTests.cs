using System;
using System.Reflection;
using Lexplosion.Logic.Network;
using NUnit.Framework;

namespace Lexplosion.Tests
{
    [TestFixture]
    public sealed class NetworkLogPrivacyTests
    {
        [TestCase("https://example.test/api/user/setActivity?sessionToken=SECRET&UUID=PRIVATE",
            "https://example.test")]
        [TestCase("https://user:password@example.test:8443/private/path?access_token=SECRET#fragment",
            "https://example.test:8443")]
        [TestCase("not-a-url", "[invalid URL]")]
        public void NetworkRequestLogContainsHostButNeverCredentials(string input, string expected)
        {
            var format = typeof(ToServer).GetMethod("SafeRequestHost",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(format, Is.Not.Null);
            var result = (string)format.Invoke(null, new object[] { input });
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(result, Does.Not.Contain("SECRET"));
            Assert.That(result, Does.Not.Contain("password"));
            Assert.That(result, Does.Not.Contain("UUID"));
        }
    }
}
