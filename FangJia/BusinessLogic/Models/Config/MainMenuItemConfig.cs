using System.Diagnostics.CodeAnalysis;

namespace FangJia.BusinessLogic.Models.Config
{
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public class MainMenuItemConfig
	{
		public string? Name     { get; set; }
		public string? Icon     { get; set; }
		public string? PageName { get; set; }
	}
}
