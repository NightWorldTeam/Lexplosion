using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.Resources.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Lexplosion.WPF.NewInterface.Core.Resources
{

    internal class LanguageLoader : IResourceLoader
    {
        #region Properties


        public string Id { get; private set; }
        public string Name { get; private set; }
        public List<LanguageItem> Content { get; set; }


        #endregion Properties



        public LanguageLoader(string themePath) : this(XElement.Load(themePath))
        {

        }


        public LanguageLoader(XElement xElement)
        {
            if (xElement.Name.LocalName.ToLower() != "language")
            {
                throw new ArgumentException("wrong format");
            }

            LoadLanguage(xElement);
        }

        void LoadLanguage(XElement xElement) 
        {
            // firstNode - первый элемент
            // Содержит свойство nextNode - следующий элемент
            // lastNode - последний элемент
            Id = xElement.Attributes().FirstOrDefault(a => a.Name.LocalName == "id").Value;
            Name = xElement.Attributes().FirstOrDefault(a => a.Name.LocalName == "name").Value;

            var currentNode = xElement.FirstNode;
            while (currentNode.NextNode != null)
            {
                var element = (currentNode as XElement);

                if (element.Name.LocalName.ToLower() == "phrase")
                {
                    var key = element.Attributes().FirstOrDefault(a => a.Name.LocalName == "key").Value;
                    var value = element.Value;

                    Content.Add(new(key, value));
                }
                else if (element.Name.LocalName.ToLower() == "languagename") 
                {
                    var key = element.Attributes().FirstOrDefault(a => a.Name.LocalName == "targetLanguage").Value;
                    var value = element.Value;

                    Content.Add(new(key, value));
                }

                currentNode = currentNode.NextNode;
            }
        }

        ResourceDictionary IResourceLoader.ToResourceDictionary()
        {
            var resourceDictionary = new ResourceDictionary();

            foreach (var item in Content) 
            {
                resourceDictionary.Add(item.Key, item.Value);
            }

            return resourceDictionary;
        }
    }
}
