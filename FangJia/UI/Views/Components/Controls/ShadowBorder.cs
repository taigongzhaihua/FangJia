using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace FangJia.UI.Views.Components.Controls
{
	public class ShadowBorder : Border
	{
		// 默认的阴影效果，只有在必要时才创建
		private readonly DropShadowEffect _dropShadowEffect;

		public ShadowBorder()
		{
			// 只在需要时才初始化 DropShadowEffect
			_dropShadowEffect = CreateDefaultShadowEffect();
			Effect            = _dropShadowEffect;
			BorderThickness   = new Thickness(0);
		}

		// 可选：提供阴影相关的属性以便自定义
		public static readonly DependencyProperty ShadowColorProperty =
			DependencyProperty.Register(nameof(ShadowColor), typeof(Color), typeof(ShadowBorder),
			                            new PropertyMetadata(Colors.SteelBlue, OnShadowPropertyChanged));

		public static readonly DependencyProperty ShadowBlurRadiusProperty =
			DependencyProperty.Register(nameof(ShadowBlurRadius), typeof(double), typeof(ShadowBorder),
			                            new PropertyMetadata(15.0, OnShadowPropertyChanged));

		public static readonly DependencyProperty ShadowDepthProperty =
			DependencyProperty.Register(nameof(ShadowDepth), typeof(double), typeof(ShadowBorder),
			                            new PropertyMetadata(3.0, OnShadowPropertyChanged));

		public static readonly DependencyProperty ShadowOpacityProperty =
			DependencyProperty.Register(nameof(ShadowOpacity), typeof(double), typeof(ShadowBorder),
			                            new PropertyMetadata(0.5, OnShadowPropertyChanged));

		// Shadow properties with getters and setters
		public Color ShadowColor
		{
			get => (Color)GetValue(ShadowColorProperty);
			set => SetValue(ShadowColorProperty, value);
		}

		public double ShadowBlurRadius
		{
			get => (double)GetValue(ShadowBlurRadiusProperty);
			set => SetValue(ShadowBlurRadiusProperty, value);
		}

		public double ShadowDepth
		{
			get => (double)GetValue(ShadowDepthProperty);
			set => SetValue(ShadowDepthProperty, value);
		}

		public double ShadowOpacity
		{
			get => (double)GetValue(ShadowOpacityProperty);
			set => SetValue(ShadowOpacityProperty, value);
		}

		// 统一处理阴影属性更改
		private static void OnShadowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var shadowBorder = (ShadowBorder)d;

			// 只更新已更改的属性
			switch (e.Property.Name)
			{
				case nameof(ShadowColor):
					shadowBorder._dropShadowEffect.Color = shadowBorder.ShadowColor;
					break;
				case nameof(ShadowBlurRadius):
					shadowBorder._dropShadowEffect.BlurRadius = shadowBorder.ShadowBlurRadius;
					break;
				case nameof(ShadowDepth):
					shadowBorder._dropShadowEffect.ShadowDepth = shadowBorder.ShadowDepth;
					break;
				case nameof(ShadowOpacity):
					shadowBorder._dropShadowEffect.Opacity = shadowBorder.ShadowOpacity;
					break;
			}
		}

		// 创建默认的 DropShadowEffect，只在需要时调用
		private static DropShadowEffect CreateDefaultShadowEffect()
		{
			return new DropShadowEffect
			       {
				       Color       = (Color)Application.Current.Resources["ShadowColor"],
				       BlurRadius  = 8,
				       ShadowDepth = 0,
				       Opacity     = 0.2
			       };
		}
	}
}
