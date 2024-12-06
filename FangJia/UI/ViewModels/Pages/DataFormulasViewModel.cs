using FangJia.BusinessLogic.Models.Data;
using FangJia.BusinessLogic.Models.Utils;
using FangJia.UI.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FangJia.UI.ViewModels.Pages;

public class DataFormulas : ViewModelBase
{
    public DataFormulas()
    {
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
            SelectedFormula = (value.Formulas.Count > 0 ? value.Formulas[0] : null)!;
        }
    }

    private Formula _selectedFormula = null!;

    public Formula SelectedFormula
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