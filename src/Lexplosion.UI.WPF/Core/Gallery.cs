using Lexplosion.UI.WPF.Core.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Lexplosion.UI.WPF.Core
{
	public class ThumbnailItem
	{
		public object OriginalSource { get; set; }
		public BitmapSource ThumbnailImage { get; set; }
	}

	public class Gallery : ObservableObject
    {
        public event Action StateChanged;


        private int _imageSourceIndex;

        /// <summary>
        /// ImageSources требуется для возможности листать избражения.
        /// Image Sources не будет иметь возможно изменяться из вне в обход метода ChangeContext.
        /// </summary>
        private List<object> _imageSources = [];
        public IReadOnlyCollection<object> ImageSources { get => _imageSources; }

        public object? SelectedImageSource { get; private set; }
        /// <summary>
        /// Наличие следующего изображения.
        /// </summary>
        public bool HasNext { get => _imageSourceIndex < _imageSources.Count - 1; }
        /// <summary>
        /// Наличие предыдущего изображения.
        /// </summary>
        public bool HasPrev { get => _imageSourceIndex > 0; }
        /// <summary>
        /// Наличие выбранного изображения
        /// </summary>
        public bool HasSelectedImage { get => SelectedImageSource != null; }

        /// <summary>
        /// Закрывает изображение и очищает ImageSources
        /// </summary>
        public void CloseImage() 
        {
			_imageSources.Clear();
			_imageSourceIndex = -1;
			SelectedImageSource = null;
			UpdateState();
		}

		/// <summary>
		/// Заменяет контекст
		/// </summary>
		public void ChangeContext(IEnumerable<object> imageSources)
		{
			_imageSources = new(imageSources);
			_imageSourceIndex = -1;
			SelectedImageSource = null;
			UpdateState();
		}

		/// <summary>
		/// Заменяет контекст и выбирает изображение за один вызов UpdateState.
		/// Если изображение не найдено в списке, выбирает первое по умолчанию.
		/// </summary>
		public void ChangeContext(IEnumerable<object> imageSources, object initialImage)
		{
			_imageSources = new(imageSources);
			_imageSourceIndex = FindImageIndex(initialImage);
			if (_imageSourceIndex == -1 && _imageSources.Count > 0)
			{
				_imageSourceIndex = 0;
			}
			SelectedImageSource = _imageSourceIndex >= 0 ? _imageSources[_imageSourceIndex] : initialImage;
			UpdateState();
		}

		/// <summary>
		/// Пытается найти изображение в ресурсах заданных при контексте. Сохраняет индекс.
		/// Если изображение не найдено в ресурсах, выбирает первое изображение по умолчанию.
		/// </summary>
		public void SelectImage(object imageSource)
		{
			_imageSourceIndex = FindImageIndex(imageSource);
			if (_imageSourceIndex == -1 && _imageSources.Count > 0)
			{
				_imageSourceIndex = 0;
			}
			SelectedImageSource = _imageSourceIndex >= 0 ? _imageSources[_imageSourceIndex] : imageSource;
			UpdateState();
		}

		public void Next()
		{
			if (!HasNext) return;

			_imageSourceIndex++;
			SelectedImageSource = _imageSources[_imageSourceIndex];
			UpdateState();
		}

		public void Prev()
		{
			if (!HasPrev) return;

			_imageSourceIndex--;
			SelectedImageSource = _imageSources[_imageSourceIndex];
			UpdateState();
		}


		private void UpdateState() 
        {
            OnPropertyChanged(null);
            StateChanged?.Invoke();
        }

		private int FindImageIndex(object target)
		{
			if (target is string strTarget)
			{
				return _imageSources.FindIndex(i => i is string s && s == strTarget);
			}

			if (target is byte[] byteArrayTarget)
			{
				IEqualityComparer comparer = StructuralComparisons.StructuralEqualityComparer;
				return _imageSources.FindIndex(i => i is byte[] arr && comparer.Equals(arr, byteArrayTarget));
			}

			if (target is IEnumerable<byte> bytesTarget)
			{
				return _imageSources.FindIndex(i => i is IEnumerable<byte> currentBytes && currentBytes.SequenceEqual(bytesTarget));
			}

			return _imageSources.FindIndex(i => object.Equals(i, target));
		}
	}
}
