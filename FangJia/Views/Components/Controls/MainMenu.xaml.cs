using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FangJia.Views.Components.Controls
{
    /// <summary>
    /// MainMenu.xaml 的交互逻辑
    /// </summary>
    public partial class MainMenu
    {
        public MainMenu()
        {
            InitializeComponent();
        }
        private void SidebarListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SidebarListBox.SelectedItem is not ListBoxItem selectedItem) return;
            // 获取选中项的索引
            var selectedIndex = SidebarListBox.Items.IndexOf(selectedItem);

            // 计算选中项在列表中的目标位置
            var targetTop = selectedIndex * selectedItem.ActualHeight;

            // 创建一个平滑移动的动画
            DoubleAnimation animation = new DoubleAnimation
            {
                To = targetTop,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            // 应用动画到 Rectangle 的 TranslateTransform 的 Y 属性上
            SelectionTransform.BeginAnimation(TranslateTransform.YProperty, animation);
        }
    }
}
