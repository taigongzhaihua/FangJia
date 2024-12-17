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

/// <summary>
/// 这是一个抽象的部分类，用于表示配方视图模型。
/// 它依赖于 DataService 和两个不同类型的爬虫服务来处理配方数据。
/// </summary>
/// <param name="dataService">数据服务，用于处理与配方相关的数据库操作。</param>
/// <param name="formulationFormulaCrawler">配方爬虫服务，用于从外部源获取配方数据。</param>
/// <param name="fangjiCrawler">方剂爬虫服务，用于从外部源获取方剂数据。</param>
[SuppressMessage("ReSharper", "HeapView.ObjectAllocation.Possible")]
[SuppressMessage("ReSharper", "HeapView.ObjectAllocation")]
[SuppressMessage("ReSharper", "HeapView.ClosureAllocation")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class FormulasViewModel(
	[Dependency("FormulationCrawler")] ICrawler<Formulation>                           formulationFormulaCrawler,
	[Dependency("FangjiCrawler")]      ICrawler<(string Category, string FormulaName)> fangjiCrawler,
	DataService                                                                        dataService)
	: ObservableObject
{
	// 私有只读字段，用于存储数据服务实例
	private readonly DataService? _dataService = dataService;

	/// <summary>
	/// 初始化数据的异步任务。
	/// 该方法从数据服务中获取所有类别，并将其赋值给 Categories 属性。
	/// 如果获取到的类别列表不为空，则将第一个类别设置为当前选中的类别。
	/// </summary>
	public async Task InitDataTask()
	{
		// 从数据服务中异步获取所有类别，并将其赋值给 Categories 属性
		Categories = [.. await _dataService!.GetCategoriesAsync()];

		// 如果获取到的类别列表不为空，则将第一个类别设置为当前选中的类别
		if (Categories.Count > 0) SelectedCategory = Categories[0];
	}

	/// <summary>
	/// 添加一个新的成分到当前选中的配方中。
	/// 该方法会在 SelectedFormula 的 Compositions 集合中添加一个新的 FormulationComposition 对象，
	/// 并触发属性更改通知以更新 UI。
	/// </summary>
	[RelayCommand]
	private void AddComposition()
	{
		// 向当前选中的配方的成分集合中添加一个新的成分对象
		SelectedFormula.Compositions!.Add(new FormulationComposition { FormulationId = SelectedFormula.Id });
		// 触发属性更改通知以更新 UI
		OnPropertyChanged(nameof(SelectedFormula));
	}

	/// <summary>
	/// 保存当前选中的配方。
	/// 如果配方的 Id 为 -1，则表示这是一个新配方，需要插入到数据库中；
	/// 否则，更新数据库中的现有配方。
	/// </summary>
	[RelayCommand]
	private async Task SaveFormula()
	{
		// 如果配方的 Id 为 -1，表示这是一个新配方，插入到数据库中
		if (SelectedFormula.Id == -1)
			await _dataService!.InsertFormulation(SelectedFormula);
		else
			// 否则，更新数据库中的现有配方
			await _dataService!.UpdateFormulation(SelectedFormula);
	}

	/// <summary>
	/// 添加一个新的配方。
	/// 该方法会创建一个新的 Formulation 对象，并将其赋值给 SelectedFormula 属性。
	/// 新配方的 Id 设置为 -1，表示这是一个新配方，并且其 CategoryId 通过异步调用数据服务获取。
	/// </summary>
	[RelayCommand]
	private async Task AddNewFormula()
	{
		// 创建一个新的 Formulation 对象，并设置其 Id 为 -1，表示这是一个新配方
		// 通过数据服务异步获取当前选中类别的 CategoryId，并赋值给新配方的 CategoryId
		SelectedFormula = new Formulation
		                  {
			                  Id         = -1,
			                  CategoryId = await _dataService?.GetCategoryIdByNameAsync(SelectedCategory.Name!)!
		                  };
	}

	/// <summary>
	/// 从外部源获取配方数据并更新本地数据库。
	/// 该方法会从两个不同的爬虫服务中获取配方数据，并将其与本地数据库中的数据进行比较和更新。
	/// 如果本地数据库中存在相同名称的配方，则更新该配方的数据；否则，插入新配方。
	/// </summary>
	[RelayCommand]
	private async Task GetFormulaFromZyfj()
	{
		var progress = new Progress<CrawlerProgress>(p => Progress = p);
		// 从配方爬虫服务中异步获取配方列表
		var list = await formulationFormulaCrawler.GetListAsync(progress);
		// 从方剂爬虫服务中异步获取方剂列表
		var fangjiList = await fangjiCrawler.GetListAsync(progress);

		// 从数据服务中异步获取本地数据库中的所有配方
		var formulationList = await _dataService!.GetFormulations();
		var formulations    = formulationList.ToList();
		Progress.Reset();
		Progress = Progress.AddLog("开始保存数据...") // 添加日志
		                   .UpdateProgress(0)
		                   with{TotalLength = formulations.Count}; // 更新进度

		// 遍历从爬虫服务获取的配方列表
		foreach (var formulation in list)
		{
			// 在本地数据库中查找是否存在相同名称的配方
			var existingFormulation = formulations.FirstOrDefault(d => d.Name == formulation.Name);

			// 通过方剂列表获取当前配方的类别，并异步获取该类别的 CategoryId
			formulation.CategoryId =
				await _dataService.GetCategoryIdByNameAsync(fangjiList
				                                            .FirstOrDefault(d => d.FormulaName == formulation.Name)
				                                            .Category);

			if (existingFormulation != null)
			{
				// 如果本地数据库中存在相同名称的配方，则更新该配方的 Id 并更新数据库
				formulation.Id = existingFormulation.Id;
				await _dataService.UpdateFormulation(formulation);
			}
			else
			{
				// 如果本地数据库中不存在相同名称的配方，则插入新配方
				await _dataService.InsertFormulation(formulation);
			}

			Progress = Progress.UpdateProgress(Progress.CurrentProgress + 1)
			                   .AddLog($"已保存 {formulation.Name}");
		}
	}

	/// <summary>
	/// 定义一个可观察的集合，用于存储类别列表。
	/// 该集合会在数据发生变化时自动通知 UI 更新。
	/// </summary>
	[ObservableProperty] private ObservableCollection<Category> _categories = [];

	/// <summary>
	/// 定义一个可观察的属性，用于存储当前选中的类别。
	/// 当该属性发生变化时，会触发 OnSelectedCategoryChanged 方法。
	/// </summary>
	[ObservableProperty] private Category _selectedCategory = null!;

	/// <summary>
	/// 当 SelectedCategory 属性发生变化时触发的部分方法。
	/// 该方法会异步调用 SetFormulasTask 方法，根据新选中的类别设置配方列表。
	/// </summary>
	/// <param name="value">新选中的类别</param>
	partial void OnSelectedCategoryChanged(Category value)
	{
		_ = SetFormulasTask(value);
	}

	/// <summary>
	/// 根据指定的类别异步设置配方列表。
	/// 该方法会从数据服务中获取与指定类别名称匹配的配方列表，并将其赋值给 Formulas 属性。
	/// 如果获取到的配方列表不为空，则将第一个配方设置为当前选中的配方。
	/// </summary>
	/// <param name="value">指定的类别</param>
	private async Task SetFormulasTask(Category value)
	{
		// 从数据服务中异步获取与指定类别名称匹配的配方列表，并赋值给 Formulas 属性
		Formulas = [.. await _dataService!.GetFormulationsByCategoryNameAsync(value.Name)];

		// 如果获取到的配方列表不为空，则将第一个配方设置为当前选中的配方
		if (Formulas.Count <= 0) return;
		SelectedFormula = Formulas[0];
	}

	/// <summary>
	/// 定义一个可观察的集合，用于存储配方列表。
	/// 该集合会在数据发生变化时自动通知 UI 更新。
	/// </summary>
	[ObservableProperty] private ObservableCollection<Formulation> _formulas = [];

	/// <summary>
	/// 定义一个可观察的属性，用于存储当前选中的配方。
	/// 当该属性发生变化时，会触发相应的 UI 更新。
	/// </summary>
	[ObservableProperty] private Formulation _selectedFormula = null!;

	/// <summary>
	/// 定义一个进度对象，用于显示配方数据获取的进度信息。
	/// </summary>
	[ObservableProperty] private CrawlerProgress _progress = new(0,0,false);
}
