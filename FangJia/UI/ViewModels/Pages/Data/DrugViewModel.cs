using FangJia.BusinessLogic.Interfaces;
using FangJia.BusinessLogic.Models.Data;
using FangJia.BusinessLogic.Models.Utils;
using FangJia.BusinessLogic.Services;
using FangJia.UI.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Unity;

namespace FangJia.UI.ViewModels.Pages.Data;

public class DrugViewModel : ViewModelBase
{
    private readonly DataService _dataService;
    private readonly ICrawler<Drug> _crawler;
    public DrugViewModel(DataService dataService, [Dependency("DrugCrawler")] ICrawler<Drug> drugCrawler)
    {
        _dataService = dataService;
        _drugList = new ObservableCollection<Drug>(_dataService.GetDrugs());
        _crawler = drugCrawler;
        ShowingDrugs = _drugList;
        SaveDrugCommand = new RelayCommand(_ =>
        {
            if (SelectedDrug!.Id > 0)
            {
                _dataService.UpdateDrug(SelectedDrug);
            }
            else
            {
                _dataService.InsertDrug(SelectedDrug);
            }
            _drugList = new ObservableCollection<Drug>(_dataService.GetDrugs());
            ShowingDrugs = _drugList;
        });
        AddDrugCommand = new RelayCommand(_ =>
        {
            SelectedDrug = new Drug()
            {
                Id = -1,
            };
        });
        GetDrugsFromZyfjCommand = new RelayCommand(GetDrugsFromZyfj);


    }

    private async void GetDrugsFromZyfj(object _)
    {
        var drugList = await _crawler.GetListAsync();

        foreach (var drug in drugList)
        {
            // 在本地 _drugList 中查找是否存在相同名称的 Drug
            var existingDrug = _drugList!.FirstOrDefault(d => d.Name == drug.Name);

            if (existingDrug != null)
            {
                // 如果存在，将 Id 赋值给 drug.Id 并更新数据库
                drug.Id = existingDrug.Id;
                _dataService.UpdateDrug(drug);
            }
            else
            {
                // 如果不存在，插入新记录
                _dataService.InsertDrug(drug);
            }
        }
        _drugList = new ObservableCollection<Drug>(_dataService.GetDrugs());
        ShowingDrugs = _drugList;
    }

    private ObservableCollection<Drug>? _drugList;
    private ObservableCollection<Drug>? _showingDrugs;

    public ObservableCollection<Drug>? ShowingDrugs
    {
        get => _showingDrugs;
        set => SetProperty(ref _showingDrugs, value);
    }

    private Drug? _selectedDrug;

    public Drug? SelectedDrug
    {
        get => _selectedDrug;
        set => SetProperty(ref _selectedDrug, value);
    }

    public ICommand SaveDrugCommand { get; set; }
    public ICommand AddDrugCommand { get; set; }
    public ICommand GetDrugsFromZyfjCommand { get; private set; }
}