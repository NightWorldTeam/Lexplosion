using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Windows;

namespace Lexplosion.WPF.NewInterface.Core.Resources.Theme
{
    class ThemeLoader : IResourceLoader
    {
        #region Properties


        /// <summary>
        /// Название темы.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Информация, о цветах, прозрачности, кистях.
        /// </summary>
        public List<ThemeItem> Content { get; set; } = [];


        #endregion Properties


        #region Constructors


        public ThemeLoader(string themePath) : this(XElement.Load(themePath))
        {

        }


        public ThemeLoader(XElement xElement)
        {
            if (xElement.Name.LocalName.ToLower() != "theme")
            {
                throw new ArgumentException("wrong format");
            }

            LoadTheme(xElement);
        }


        #endregion Constructors


        public ResourceDictionary ToResourceDictionary()
        {
            var resourceDictionary = new ResourceDictionary();

            resourceDictionary.Add("__Name", Name);
            resourceDictionary.Add("__Type", "_ColorTheme");

            foreach (var item in Content)
            {
                resourceDictionary.Add($"{item.Name}Color", item.Color);
                resourceDictionary.Add($"{item.Name}SolidColorBrush", item.Brush);
            }

            return resourceDictionary;
        }


        /// <summary>
        /// Загружает тему из формата XML.
        /// </summary>
        /// <param name="xElement"></param>
        private void LoadTheme(XElement xElement)
        {
            // firstNode - первый элемент
            // Содержит свойство nextNode - следующий элемент
            // lastNode - последний элемент
            Name = xElement.Attributes().FirstOrDefault(a => a.Name.LocalName == "name").Value;

            var currentNode = xElement.FirstNode;

            while (currentNode.NextNode != null)
            {
                var element = (currentNode as XElement);

                if (element.Name.LocalName.ToLower() == "color")
                {
                    var key = element.Attributes().FirstOrDefault(a => a.Name.LocalName == "key").Value;
                    var value = element.Value;

                    Content.Add(new(key, value));
                }

                currentNode = currentNode.NextNode;
            }
        }
    }
}
