using System.Collections.Generic;

namespace Lexplosion.UI.WPF.Core.Paginator
{
    /// <summary>
    /// Описывает базу для пагинатора
    /// </summary>
    /// <typeparam name="T">Тип элемента пагинируемой страницы</typeparam>
    public interface IPaginator<out T>
    {
        /// <summary>
        /// Количество страниц.
        /// </summary>
        uint PageCount { get; }
        /// <summary>
        /// Количество элементов на странице.
        /// </summary>
        uint PageSize { get; }
        /// <summary>
        /// Индекс выбранной страницы.
        /// </summary>
        uint CurrentPageIndex { get; }
        /// <summary>
        /// Является ли выбранная страница первой.
        /// </summary>
        bool IsFirst { get; }
        /// <summary>
        /// Является ли выбранная страница последней.
        /// </summary>
        bool IsLast { get; }
        /// <summary>
        /// Перейти на следуйщую страницу
        /// </summary>
        IEnumerable<T> Next();
        /// <summary>
        /// Перейти на предыдущая страница
        /// </summary>
        IEnumerable<T> Prev();
        /// <summary>
        /// Перейти на первую страницу.
        /// </summary>
        IEnumerable<T> ToFirst();
        /// <summary>
        /// Перейти на последнюю страницу.
        /// </summary>
        IEnumerable<T> ToLast();
        /// <summary>
        /// Перейти на страницу с номером pageIndex;
        /// </summary>
        /// <param name="pageIndex">Номер страницы на которую нужно перейти</param>
        IEnumerable<T> To(uint pageIndex);
    }
}
