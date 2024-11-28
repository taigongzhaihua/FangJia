using FangJia.Models.ConfigModels;
using NLog;
using System.Collections.ObjectModel;

namespace FangJia.Views.Pages
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public Setting()
        {
            InitializeComponent();
            var x = ItemsControl.ItemsSource as ObservableCollection<Group>;
            Logger.Debug(x?[0].Title);
            var y = ItemsControl.Items.CurrentItem as Group;
            Logger.Debug(y?.Title);
        }
    }
}
