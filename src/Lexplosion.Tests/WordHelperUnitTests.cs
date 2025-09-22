using Lexplosion.Core.Tools;
using Lexplosion.UI.WPF.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Tests
{
    [TestClass]
    public class WordHelperTests
    {
        [TestCase(1, "скачивание")]
        [TestCase(2, "скачивания")]
        [TestCase(5, "скачиваний")]
        [TestCase(21, "скачивание")]
        [TestCase(321, "скачивание")]
        [TestCase(0, "скачиваний")]
        [TestCase(15640, "скачиваний")]
        [TestCase(15641, "скачивание")]
        [TestCase(5000000, "скачиваний")]
        [TestCase(5000001, "скачивание")]
        [TestCase(5050501, "скачивание")]
        public void RuDownloads(int exceptedNumber, string exceptedCurrentPath)
        {
            var result = WordHelper.GetWordWithRightEndingForNumber(exceptedNumber, new string[3] { "скачивание", "скачивания", "скачиваний" });
            NUnit.Framework.Assert.AreEqual(exceptedCurrentPath, result);
        }
    }
}
