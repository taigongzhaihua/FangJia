using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FangJia.UI.ViewModels.Pages;

public partial class MemorizationViewModel: ObservableObject
{
	[ObservableProperty] private string? _memorizationText = "解表蠲饮小青龙，[麻]{麻黄}[桂]{桂枝}[姜]{生姜}[辛]{细辛}[夏]{半夏}[草]{甘草}从。[芍药]{芍药}[五味]{五味子}敛气阴，表寒内饮最有功。";
}
