using System;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Core.Paginator
{
    public sealed class Paginator<T> : IPaginator<T>
    {
        private readonly Func<uint, IEnumerable<T>> _paginate;


        #region Properties


        public uint CurrentPageIndex { get; private set; } = 1;
        public uint PageSize { get; } = 0;
        public uint PageCount { get; } = 0;

        public bool IsFirst => CurrentPageIndex == 1;
        public bool IsLast => CurrentPageIndex == PageCount;


        #endregion Properties


        #region Constructors


        /// <summary>
        /// Конструктор класса paginator
        /// </summary>
        /// <param name="pageCount">Количество элементов на странице</param>
        /// <param name="paginate">Функция принимающая индекс выбранной страницы, возвращает IEnumerable элементов на странице.</param>
        public Paginator(uint pageCount, uint pageSize, Func<uint, IEnumerable<T>> paginate)
        {
            PageCount = pageCount;
            _paginate += paginate;
        }


        #endregion Constructors


        #region Public Methods


        public IEnumerable<T> Next()
        {
            if (CurrentPageIndex + 1 <= PageCount)
                CurrentPageIndex++;
            return _paginate.Invoke(CurrentPageIndex - 1);
        }

        public IEnumerable<T> Prev()
        {
            // TODO: индекс нужно считать как index-- или как просто index;
            if (CurrentPageIndex - 1 > 0)
                CurrentPageIndex--;
            return _paginate.Invoke(CurrentPageIndex - 1);
        }

        public override string ToString()
        {
            var s = string.Empty;
            if (CurrentPageIndex < 4)
            {
                for (var i = 1; i < 5; i++)
                {
                    s += CurrentPageIndex == i ? "_" + i + "_ " : i + " ";
                }

                s += "... " + PageCount;
            }
            else if (PageCount - 3 < CurrentPageIndex)
            {
                s += "1 ... ";

                for (var i = PageCount - 3; i < PageCount + 1; i++)
                {
                    s += CurrentPageIndex == i ? "_" + i + "_ " : i + " ";
                }
            }
            else
            {
                s += "1 ... ";
                s += CurrentPageIndex - 1;
                s += " _" + CurrentPageIndex + "_ ";
                s += CurrentPageIndex + 1;
                s += " ... " + PageCount;
            }
            return s;
        }

        public IEnumerable<T> ToFirst()
        {
            CurrentPageIndex = 1;
            return _paginate.Invoke(CurrentPageIndex);
        }

        public IEnumerable<T> ToLast()
        {
            CurrentPageIndex = PageCount - 1;
            return _paginate.Invoke(CurrentPageIndex);
        }

        public IEnumerable<T> To(uint pageIndex) 
        {
            if (!(pageIndex - 1 > 0 && CurrentPageIndex + 1 <= PageCount))             
                throw new ArgumentException($"{pageIndex} out of range max page index {PageCount}");

            CurrentPageIndex = pageIndex;
            return _paginate.Invoke(CurrentPageIndex - 1);
        }


        #endregion Public Methods
    }
}
