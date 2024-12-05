using FangJia.Cores.Base;
using FangJia.Cores.Utils.Commands;
using FangJia.Models.DataModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FangJia.ViewModels.PageViewModels;

public class DataFormulas : ViewModelBase
{
    public DataFormulas()
    {
        Categories =
        [
            new Category
            {
                Name = "解表剂",
                SubCategories =
                [
                    new Category
                    {
                        Name = "辛温解表",
                        Formulas =
                        [
                            new Formula { Name = "葛根汤", Medicines = "葛根、麻黄", Purpose = "解表散寒" },
                            new Formula { Name = "麻黄汤", Medicines = "麻黄、桂枝", Purpose = "解表散寒" }
                        ]
                    },

                    new Category
                    {
                        Name = "辛凉解表",
                        Formulas = [new Formula { Name = "白虎汤", Medicines = "知母、石膏", Purpose = "清热生津" }]
                    }
                ]
            }
        ];
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