using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lexplosion.Core.Extensions;

namespace Lexplosion.Tests
{
    [TestClass]
    public class HexUnitTests
    {
        [Theory]
        [TestCase("#fff", true)]          // Короткий формат, допустимые символы
        [TestCase("#1a2", true)]          // Короткий формат, допустимые символы
        [TestCase("#FFFFFF", true)]       // Длинный формат, допустимые символы
        [TestCase("#1a2b3c", true)]       // Длинный формат, допустимые символы
        [TestCase("#1A2B3C", true)]       // Длинный формат, допустимые символы (заглавные)
        [TestCase("#123", true)]          // Короткий формат, допустимые символы
        [TestCase("#123456", true)]       // Длинный формат, допустимые символы
        [TestCase("#ggg", false)]         // Короткий формат, недопустимые символы
        [TestCase("#12345", false)]       // Некорректная длина (не 4 и не 7)
        [TestCase("#1234567", false)]     // Некорректная длина (слишком длинная)
        [TestCase("123456", false)]       // Отсутствует '#'
        [TestCase("#12 34", false)]       // Недопустимые символы (пробел)
        [TestCase("", false)]             // Пустая строка
        [TestCase("#", false)]            // Только '#'
        [TestCase("#xyz", false)]         // Короткий формат, недопустимые символы
        [TestCase("#12345G", false)]      // Длинный формат, недопустимые символы
        public void IsHex(string hex, bool exceptedResult)
        {
            var result = hex.IsHexColor();
            NUnit.Framework.Assert.AreEqual(exceptedResult, result);
        }
    }
}
