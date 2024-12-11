using FangJia.BusinessLogic.Interfaces;
using FangJia.BusinessLogic.Models.Data;
using FangJia.BusinessLogic.Models.Utils;
using FangJia.BusinessLogic.Services;
using FangJia.UI.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Unity;

namespace FangJia.UI.ViewModels.Pages.Data;

public class FormulasViewModel : ViewModelBase
{
    private static DataService? _dataService;
    private readonly ICrawler<Formulation> _formulaCrawler;
    private readonly ICrawler<(string Category, string FormulaName)> _fangjiCrawler;
    public FormulasViewModel(DataService dataService,
        [Dependency("FormulationCrawler")] ICrawler<Formulation> formulationFormulaCrawler,
        [Dependency("FangjiCrawler")] ICrawler<(string Category, string FormulaName)> fangjiCrawler)
    {
        _dataService = dataService;
        _formulaCrawler = formulationFormulaCrawler;
        _fangjiCrawler = fangjiCrawler;
        Categories = new ObservableCollection<Category>(_dataService.GetCategories());
        if (Categories.Count > 0)
        {
            SelectedCategory = Categories[0];
        }

        SaveFormulaCommand = new RelayCommand(_ =>
        {
            if (SelectedFormula.Id == -1)
            {
                dataService.InsertFormulation(SelectedFormula);
            }
            else
            {
                _dataService.UpdateFormulation(SelectedFormula);
            }
        });
        CancelEditFormulaCommand = new RelayCommand(_ =>
        {
            SelectedFormula = new Formulation
            {
                Id = -1,
                CategoryId = _dataService.GetCategoryIdByName(SelectedCategory.Name!)
            };
        });

        AddCompositionCommand = new RelayCommand(_ =>
        {
            SelectedFormula.Compositions!.Add(new FormulationComposition { FormulationId = SelectedFormula.Id });
            OnPropertyChanged(nameof(SelectedFormula));
        });

        GetFormulaFromZyfjCommand = new RelayCommand(GetFormulaFromZyfj);
    }

    private async void GetFormulaFromZyfj(object _)
    {
        var list = await _formulaCrawler.GetListAsync();
        var formulationList = _dataService!.GetFormulations().ToList();
        var fangjiList = await _fangjiCrawler.GetListAsync();
        foreach (var formulation in list)
        {
            // 在本地 _drugList 中查找是否存在相同名称的 Drug
            var existingDrug = formulationList.FirstOrDefault(d => d.Name == formulation.Name);
            formulation.CategoryId =
                _dataService.GetCategoryIdByName(fangjiList.FirstOrDefault(d => d.FormulaName == formulation.Name)
                    .Category);

            if (existingDrug != null)
            {
                // 如果存在，将 Id 赋值给 drug.Id 并更新数据库
                formulation.Id = existingDrug.Id;
                _dataService.UpdateFormulation(formulation);
            }
            else
            {
                // 如果不存在，插入新记录
                _dataService.InsertFormulation(formulation);
            }
        }
    }

    private ObservableCollection<Category> _categories = [];

    public ObservableCollection<Category> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    private Category _selectedCategory = null!;

    public Category SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            SetProperty(ref _selectedCategory, value);
            Formulas = new ObservableCollection<Formulation>(_dataService!.GetFormulationsByCategoryName(value.Name));
            if (Formulas.Count <= 0) return;
            SelectedFormula = Formulas[0];
        }
    }
    private ObservableCollection<Formulation> _formulas = [];

    public ObservableCollection<Formulation> Formulas
    {
        get => _formulas;
        set => SetProperty(ref _formulas, value);
    }

    private Formulation _selectedFormula = null!;

    public Formulation SelectedFormula
    {
        get => _selectedFormula;
        set => SetProperty(ref _selectedFormula, value);
    }

    public ICommand SaveFormulaCommand { get; }
    public ICommand CancelEditFormulaCommand { get; }
    public ICommand AddCompositionCommand { get; }
    public ICommand GetFormulaFromZyfjCommand { get; }
}