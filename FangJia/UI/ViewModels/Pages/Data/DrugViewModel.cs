using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FangJia.BusinessLogic.Interfaces;
using FangJia.BusinessLogic.Models.Data;
using FangJia.BusinessLogic.Services;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using FangJia.BusinessLogic.Models;
using Unity;

namespace FangJia.UI.ViewModels.Pages.Data;

[SuppressMessage("ReSharper", "HeapView.ObjectAllocation")]
[SuppressMessage("ReSharper", "HeapView.ObjectAllocation.Possible")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class DrugViewModel(
	[Dependency("DrugCrawler")] ICrawler<Drug> drugCrawler,
	DataService                                dataService
) : ObservableObject
{
	/// <summary>
	/// 初始化数据任务
	/// </summary>
	/// <remarks>
	/// 步骤：
	/// 1. 从数据服务中获取药品列表
	/// 2. 将获取到的药品列表赋值给 _drugList
	/// 3. 将 _drugList 赋值给 ShowingDrugs 属性
	/// </remarks>
	public async Task InitDataTask()
	{
		_drugList = [.. await dataService.GetDrugs()];

		ShowingDrugs = _drugList;
	}

	/// <summary>
	/// 添加药品命令
	/// </summary>
	/// <remarks>
	/// 步骤：
	/// 1. 创建一个新的 Drug 对象，并将其 Id 设置为 -1
	/// 2. 将新创建的 Drug 对象赋值给 SelectedDrug 属性
	/// </remarks>
	[RelayCommand]
	private void AddDrug()
	{
		SelectedDrug = new Drug { Id = -1 };
	}

	/// <summary>
	/// 保存药品命令
	/// </summary>
	/// <remarks>
	/// 步骤：
	/// 1. 检查 SelectedDrug 的 Id 是否大于 0
	/// 2. 如果 Id 大于 0，调用数据服务的 UpdateDrug 方法更新药品
	/// 3. 如果 Id 不大于 0，调用数据服务的 InsertDrug 方法插入药品
	/// 4. 调用 InitDataTask 方法重新初始化数据
	/// </remarks>
	[RelayCommand]
	private async Task SaveDrug()
	{
		if (SelectedDrug!.Id > 0)
			await dataService.UpdateDrug(SelectedDrug);
		else
			await dataService.InsertDrug(SelectedDrug);
		await InitDataTask();
	}

	/// <summary>
	/// 从 Zyfj 获取药品命令
	/// </summary>
	/// <remarks>
	/// 步骤：
	/// 1. 调用 drugCrawler 的 GetListAsync 方法获取药品列表
	/// 2. 遍历获取到的药品列表
	/// 3. 在本地 _drugList 中查找是否存在相同名称的 Drug
	/// 4. 如果存在，将 Id 赋值给 drug.Id 并更新数据库
	/// 5. 如果不存在，插入新记录
	/// 6. 调用 InitDataTask 方法重新初始化数据
	/// </remarks>
	[RelayCommand]
	private async Task GetDrugsFromZyfj()
	{
		var progress = new Progress<CrawlerProgress>(p => Progress = p);
		var drugList = await drugCrawler.GetListAsync(progress);

		Progress = Progress.Reset();
		Progress = Progress with { TotalLength = drugList.Count };
		Progress = Progress with { IsRunning = true };
		Progress = Progress.AddLog("正在保存数据...");
		foreach (var drug in drugList)
		{
			// 在本地 _drugList 中查找是否存在相同名称的 Drug
			var existingDrug = _drugList!.FirstOrDefault(d => d.Name == drug.Name);

			if (existingDrug != null)
			{
				// 如果存在，将 Id 赋值给 drug.Id 并更新数据库
				drug.Id = existingDrug.Id;

				await dataService.UpdateDrug(drug);
			}
			else
			{
				// 如果不存在，插入新记录
				await dataService.InsertDrug(drug);
			}

			Progress = Progress.UpdateProgress(Progress.CurrentProgress + 1);
			await Task.Delay(1);
		}

		Progress = Progress.AddLog("数据保存完成。");
		Progress = Progress.AddLog("正在初始化数据...");

		await InitDataTask();
		Progress = Progress.Reset();
	}

	/// <summary>
	/// 药品列表
	/// </summary>
	private ObservableCollection<Drug>? _drugList;

	/// <summary>
	/// 显示的药品列表
	/// </summary>
	[ObservableProperty] private ObservableCollection<Drug>? _showingDrugs;

	/// <summary>
	/// 选中的药品
	/// </summary>
	[ObservableProperty] private Drug? _selectedDrug;

	/// <summary>
	/// 爬虫进度
	/// </summary>
	[ObservableProperty] private CrawlerProgress _progress = new(0, 0, false);
}
