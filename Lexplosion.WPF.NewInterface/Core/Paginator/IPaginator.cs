using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Core.Paginator
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
        int PageCount { get; }
        /// <summary>
        /// Количество элементов на странице.
        /// </summary>
        int PageSize { get; }
        /// <summary>
        /// Индекс выбранной страницы.
        /// </summary>
        int CurrentPageIndex { get; }
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
    }
}
