using System;

namespace Lexplosion.UI.WPF.Tools
{
    public static class WordHelper
    {
        #region Public Methods


        /// <summary>
        /// Выбирает правильную форму РУССКОГО слова для использования с числом.
        /// <br/>Например:
        ///  <br/>1 (одно) скачивание
        /// <br/>2...4 (два) скачивания
        /// <br/>5...21 (5) скачиваний
        /// </summary>
        /// <param name="number">Число</param>
        /// <param name="wordForms">Формы слова с числами</param>
        /// <returns>Правильную форму слова из представленных.</returns>
        public static string GetWordWithRightEndingForNumber(int number, string[] wordForms)
        {
            if (wordForms.Length != 3)
                throw new ArgumentException("Размер массива с формами слова должен быть равен 3");

            return wordForms[GetIndexWithRightEndingForNumber(number % 10)];
        }


        /// <summary>
        /// Возвращает ключ с правильной формой РУССКОГО слова для использования с числом.
        /// <br/>Например:
        /// <br/>1 (одно) скачивание
        /// <br/>2...4 (два) скачивания
        /// <br/>5...21 (5) скачиваний
        /// </summary>
        /// <param name="number">Число</param>
        /// <param name="wordForms">Ключи формы слова с числами</param>
        /// <returns>Правильную форму слова из представленных.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GetWordKeyWithRightEndingForNumber(int number, string[] wordKeys)
        {
            if (wordKeys.Length != 3)
                throw new ArgumentException("Размер массива с ключами слова должен быть равен 3");

            return wordKeys[GetIndexWithRightEndingForNumber(number % 100)];
        }


        #endregion Public Methods


        #region Private Methods


        /// <summary>
        /// Возвращаем индекс правильной формы РУССКОГО для массива форм слова для использование с числом. 
        /// </summary>
        /// <param name="numberEnding">Число окончания</param>
        /// <returns>Индекс правильной формы</returns>
        private static int GetIndexWithRightEndingForNumber(int numberEnding)
        {
            if (1 < numberEnding && numberEnding < 5)
                return 1;
            else if (4 < numberEnding && numberEnding < 21)
                return 2;
            else if (numberEnding % 10 == 1)
                return 0;
            else
                return 2;
        }


        #endregion Private Methods
    }
}
