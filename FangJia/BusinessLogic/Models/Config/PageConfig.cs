﻿using System.Diagnostics.CodeAnalysis;

namespace FangJia.BusinessLogic.Models.Config;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class PageConfig
{
	public string? Name { get; set; } // 页面名称，用于标识页面
	public string? Uri  { get; set; } // 页面 URI，用于导航时加载页面
}
