using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Controls
{
    public sealed class InstanceFormOverviewData
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public byte[] Avatar { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public IProjectCategory Tags { get; set; }
    }

    [TemplatePart(Name = PART_TAGS_PANEL, Type = typeof(InstanceForm))]
    public sealed class InstanceForm : Control
    {
        private const string PART_TAGS_PANEL = "PART_TagsPanel";  


        private InstanceClient _instanceClient;

        private ItemsControl _tagsPanel;

        #region Properties


        public InstanceFormOverviewData OverviewData { get; private set; }


        #endregion Properties


        #region Dependency Properties


        public static readonly DependencyProperty InstanceClientProperty
                = DependencyProperty.Register("InstanceClient", typeof(InstanceClient), typeof(InstanceForm),
                    new FrameworkPropertyMetadata(null));

        public InstanceClient InstanceClient
        {
            get => (InstanceClient)GetValue(InstanceClientProperty);
            set => SetValue(InstanceClientProperty, value);
        }


        #endregion Dependency Properties


        #region Constructors


        static InstanceForm()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InstanceForm), new FrameworkPropertyMetadata(typeof(InstanceForm)));
        }


        #endregion Constructors


        #region Public & Protected Methods


        public override void OnApplyTemplate()
        {
            _tagsPanel = Template.FindName(PART_TAGS_PANEL, this) as ItemsControl;

            _tagsPanel.ItemsSource = new List<String>()
            {
                "1.12.1", "Tech", "Magic", "Extra Large", "Quests", "Adventure & RPG"
            };

            base.OnApplyTemplate();
        }


        #endregion Public & Protected Methods
    }
}
