using Lexplosion.Tools;
using NUnit.Framework;

namespace Lexplosion.Tests
{
    [TestFixture]
    public class ImageToolsTests
    {
        [Test]
        public void CanResizeKnownRasterFormats()
        {
            Assert.That(ImageTools.CanResizeImage(new byte[]
            {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
            }), Is.True);
            Assert.That(ImageTools.CanResizeImage(new byte[]
            {
                0xFF, 0xD8, 0xFF, 0xE0
            }), Is.True);
        }

        [Test]
        public void DoesNotSendWebPOrInvalidDataToGdiDecoder()
        {
            Assert.That(ImageTools.CanResizeImage(new byte[]
            {
                0x52, 0x49, 0x46, 0x46, 0, 0, 0, 0, 0x57, 0x45, 0x42, 0x50
            }), Is.False);
            Assert.That(ImageTools.CanResizeImage(new byte[]
            {
                0x3C, 0x68, 0x74, 0x6D, 0x6C, 0x3E
            }), Is.False);
            Assert.That(ImageTools.CanResizeImage(null), Is.False);
        }
    }
}
