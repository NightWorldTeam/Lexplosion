using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Core
{
    public class SwitchCaseCollection : ObservableCollection<SwitchCaseItem> 
    {
        
    }

    public class SwitchCaseItem : ContentControl 
    {
        public event Action KeyChanged;

        public static readonly DependencyProperty KeyProperty
            = DependencyProperty.Register(nameof(Key), typeof(object), typeof(SwitchCaseItem),
                new FrameworkPropertyMetadata(defaultValue: null, propertyChangedCallback: OnKeyPropertyChanged));

        public object Key 
        {
            get => (object)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        private static void OnKeyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = (d as SwitchCaseItem);
            _this.Key = e.NewValue;
            _this.KeyChanged?.Invoke();
        }

        private static void OnContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }
    }

    public class Switch : UserControl
    {
        private readonly Guid _id;

        #region Dependency Properties


        public static readonly DependencyProperty SelectedExpressionProperty
            = DependencyProperty.Register(nameof(SelectedExpression), typeof(object), typeof(Switch),
                new FrameworkPropertyMetadata(null, propertyChangedCallback: OnConditionPropertyChanged));

        public static readonly DependencyProperty CasesProperty
            = DependencyProperty.RegisterAttached(nameof(Cases), typeof(SwitchCaseCollection), typeof(Switch),
            new FrameworkPropertyMetadata(new SwitchCaseCollection(), propertyChangedCallback: OnConditionsPropertyChanged));


        #endregion Dependency Properties


        #region Properties


        public object SelectedExpression
        {
            get => (object)GetValue(SelectedExpressionProperty);
            set => SetValue(SelectedExpressionProperty, value);
        }

        public SwitchCaseCollection Cases
        {
            get => (SwitchCaseCollection)GetValue(CasesProperty);
            set => SetValue(CasesProperty, value);
        }


        #endregion Properties


        public Switch()
        {
            // Коллекция по умолчанию не пустая, обнуляем её чтобы всё работало как надо
            // Думаю в будущем, при добавлении в коллецию нужно проверять хеш-объекта, чтобы компонент не падал
            Cases = new SwitchCaseCollection();
            UpdateContent();
            Cases.CollectionChanged += Cases_CollectionChanged;
        }

        private void UpdateContent()
        {
            Content = Cases?.FirstOrDefault(c => IsEqualsKeys(c.Key, SelectedExpression)); 
        }


        private bool IsEqualsKeys(object key, object key2) 
        {
            if (key?.GetType() != key2?.GetType())
                return false;

            if (key.GetType().IsValueType && key2.GetType().IsValueType)
                return key.Equals(key2);

            if (key is string && key2 is string)
                return (key as string) == (key2 as string);

            return key == key2;
        }



        private static void OnContentDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = (Switch)d;
            _this.UpdateContent();
        }

        private static void OnConditionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = (Switch)d;
            var r = _this._id;
            _this.UpdateContent();
        }

        private static void OnConditionsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = (Switch)d;

            if (e.OldValue != null)
                (e.OldValue as SwitchCaseCollection).CollectionChanged -= _this.Cases_CollectionChanged;

            if (e.NewValue != null)
                (e.NewValue as SwitchCaseCollection).CollectionChanged += _this.Cases_CollectionChanged;

            _this.UpdateContent();
        }

        private void Cases_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        foreach (var sci in e.NewItems) 
                        {
                            (sci as SwitchCaseItem).KeyChanged += UpdateContent;
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    {
                        foreach (var sci in e.OldItems)
                        {
                            (sci as SwitchCaseItem).KeyChanged -= UpdateContent;
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    {
                        foreach (var sci in e.OldItems)
                        {
                            (sci as SwitchCaseItem).KeyChanged -= UpdateContent;
                        }

                        foreach (var sci in e.NewItems)
                        {
                            (sci as SwitchCaseItem).KeyChanged += UpdateContent;
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    { }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    {
                        foreach (var oldItem in e.OldItems)
                        {
                            foreach (var sci in e.OldItems)
                            {
                                (sci as SwitchCaseItem).KeyChanged -= UpdateContent;
                            }
                        }
                    }
                    break;
            }
        }
    }
}
