using FangJia.BusinessLogic.Models.Data;
using FangJia.BusinessLogic.Models.Utils;
using FangJia.BusinessLogic.Services;
using FangJia.UI.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FangJia.UI.ViewModels.Pages.Data;

public class FormulasViewModel : ViewModelBase
{
    private static DataService? _dataService;
    public FormulasViewModel(DataService dataService)
    {
        _dataService = dataService;
        Categories = new ObservableCollection<Category>(_dataService.GetCategories());

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
            Formulas = new ObservableCollection<Formulation>(_dataService!.GetFormulationsByName(value.Name));
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
    private readonly RelayCommand _saveFormulaCommand = new(SaveFormula);
    public ICommand SaveFormulaCommand => _saveFormulaCommand;

    private static void SaveFormula(object commandParameter)
    {
    }

    private readonly RelayCommand _cancelEditFormulaCommand = new(CancelEditFormula);
    public ICommand CancelEditFormulaCommand => _cancelEditFormulaCommand;

    private static void CancelEditFormula(object commandParameter)
    {
    }
}