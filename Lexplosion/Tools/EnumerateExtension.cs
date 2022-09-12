using Lexplosion.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Lexplosion.Tools
{
    public sealed class EnumerateExtension : MarkupExtension
    {
        public Type Type { get; set; }

        public EnumerateExtension(Type type)
        {
            this.Type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            string[] names = Enum.GetNames(Type);
            string[] values = new string[names.Length];

            for (int i = 0; i < names.Length; i++)
            { 
                values[i] = Resources.ResourceManager.GetString(names[i]); 
            }

            return values;
        }
    }
}
