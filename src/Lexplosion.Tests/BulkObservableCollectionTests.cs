using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Lexplosion.UI.WPF.Core;
using NUnit.Framework;

namespace Lexplosion.Tests
{
    [TestFixture]
    public sealed class BulkObservableCollectionTests
    {
        [Test]
        public void ReplaceAllPublishesOneResetRegardlessOfItemCount()
        {
            var collection = new BulkObservableCollection<int>();
            var events = new List<NotifyCollectionChangedAction>();
            var properties = new List<string>();
            collection.CollectionChanged += (sender, args) => events.Add(args.Action);
            ((INotifyPropertyChanged)collection).PropertyChanged +=
                (sender, args) => properties.Add(args.PropertyName);

            collection.ReplaceAll(Enumerable.Range(0, 100));

            Assert.That(collection.Count, Is.EqualTo(100));
            Assert.That(collection[99], Is.EqualTo(99));
            Assert.That(events, Is.EqualTo(new[] { NotifyCollectionChangedAction.Reset }));
            Assert.That(properties, Is.EquivalentTo(new[] { "Count", "Item[]" }));
        }

        [Test]
        public void ReplaceAllCanReuseCurrentItemsAndClearWithOneReset()
        {
            var collection = new BulkObservableCollection<string>();
            collection.ReplaceAll(new[] { "a", "b" });
            int resets = 0;
            collection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Reset) resets++;
            };

            collection.ReplaceAll(collection);
            collection.ReplaceAll(Array.Empty<string>());

            Assert.That(resets, Is.EqualTo(2));
            Assert.That(collection, Is.Empty);
        }
    }
}
