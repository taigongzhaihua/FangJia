using Dapper;
using FangJia.BusinessLogic.Models.Data;
using FangJia.DataAccess;
using NLog;
using System.Collections.ObjectModel;
using Drug = FangJia.BusinessLogic.Models.Data.Drug;

namespace FangJia.BusinessLogic.Services;

public class DataService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly DbManager _dbManager;

    private const string
        ConnectionString = "Data Source=|DataDirectory|\\\\data.db;Version=3;"; // 直接在类内部写定 connectionString

    // 构造函数，依赖注入 DbManager 对象
    public DataService(DbManager dbManager)
    {
        _dbManager = dbManager;
        _dbManager.SetConnection(ConnectionString);
        CreateTables();
    }

    // 创建所有表，包括外键约束和级联删除
    public void CreateTables()
    {
        // 分类表
        _dbManager.CreateTableIfNotExists("Category",
            ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"),
            ("FirstCategory", "TEXT"),
            ("SecondCategory", "TEXT"));

        // 方剂表，设置外键约束和级联删除
        _dbManager.CreateTableIfNotExists("Formulation",
            ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"),
            ("Name", "TEXT"),
            ("CategoryId", "INTEGER"),
            ("Usage", "TEXT"),
            ("Effect", "TEXT"),
            ("Indication", "TEXT"),
            ("Disease", "TEXT"),
            ("Application", "TEXT"),
            ("Supplement", "TEXT"),
            ("Song", "TEXT"),
            ("Notes", "TEXT"),
            ("Source", "TEXT"),
            ("FOREIGN KEY(CategoryId)", " REFERENCES Category(Id) ON DELETE CASCADE"));

        // 方剂图片表，设置外键约束和级联删除
        _dbManager.CreateTableIfNotExists("FormulationImage",
            ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"),
            ("FormulationId", "INTEGER"),
            ("Image", "BLOB"),
            ("FOREIGN KEY(FormulationId)", "REFERENCES Formulation(Id) ON DELETE CASCADE"));

        // 方剂组成表，设置外键约束和级联删除
        _dbManager.CreateTableIfNotExists("FormulationComposition",
            ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"),
            ("FormulationId", "INTEGER"),
            ("DrugId", "INTEGER"),
            ("DrugName", "TEXT"),
            ("Effect", "TEXT"),
            ("Position", "TEXT"),
            ("Notes", "TEXT"),
            ("FOREIGN KEY(FormulationId)", "REFERENCES Formulation(Id) ON DELETE CASCADE"),
            ("FOREIGN KEY(DrugId)", "REFERENCES Drug(Id) ON DELETE CASCADE"));

        // 药物表
        _dbManager.CreateTableIfNotExists("Drug",
            ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"),
            ("Name", "TEXT"),
            ("EnglishName", "TEXT"),
            ("LatinName", "TEXT"),
            ("Category", "TEXT"),
            ("Origin", "TEXT"),
            ("Properties", "TEXT"),
            ("Quality", "TEXT"),
            ("Taste", "TEXT"),
            ("Meridian", "TEXT"),
            ("Effect", "TEXT"),
            ("Notes", "TEXT"),
            ("Processed", "TEXT"),
            ("Source", "TEXT"));

        // 药物图片表，设置外键约束和级联删除
        _dbManager.CreateTableIfNotExists("DrugImage",
            ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"),
            ("DrugId", "INTEGER"),
            ("Image", "BLOB"),
            ("FOREIGN KEY(DrugId)", "REFERENCES Drug(Id) ON DELETE CASCADE"));
    }

    // 插入分类
    public int InsertCategory(string firstCategory, string secondCategory)
    {
        return _dbManager.Insert("Category",
            ("FirstCategory", firstCategory),
            ("SecondCategory", secondCategory));
    }

    // 获取所有分类
    public IEnumerable<Category> GetCategories()
    {
        try
        {
            // 1. 查询所有一级分类名称
            var firstCategories = _dbManager.Query<string>(
                "Category",
                ["FirstCategory"],
                "WHERE FirstCategory IS NOT NULL GROUP BY FirstCategory");

            // 2. 创建一级分类的列表
            var categories = new List<Category>();

            // 3. 遍历所有一级分类，查询对应的二级分类
            foreach (var firstCategoryName in firstCategories)
            {
                // 创建一级分类对象
                var category = new Category
                {
                    Name = firstCategoryName,
                    SubCategories = []
                };

                // 查询当前一级分类下的所有二级分类
                var parameters = new DynamicParameters();
                parameters.Add("FirstCategory", firstCategoryName);

                var secondCategories = _dbManager.Query<string>(
                    "Category",
                    ["SecondCategory"],
                    "WHERE FirstCategory = @FirstCategory AND SecondCategory IS NOT NULL GROUP BY SecondCategory",
                    parameters);

                // 将查询到的二级分类添加到当前一级分类的子分类集合中
                foreach (var secondCategoryName in secondCategories)
                    category.SubCategories.Add(new Category { Name = secondCategoryName });

                // 将一级分类添加到列表中
                categories.Add(category);
            }

            // 4. 返回所有一级分类及其子分类
            return categories;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "获取分类时发生错误");
            throw;
        }
    }

    public int GetCategoryIdByName(string name)
    {
        return _dbManager.QuerySingleOrDefault<int>("Category",
            "WHERE FirstCategory = @CategoryName OR SecondCategory = @CategoryName",
            ["Id"],
            new { CategoryName = name });
    }

    public IEnumerable<Formulation> GetFormulationsByCategoryName(string? categoryName)
    {
        try
        {
            // 1. 查询分类表，获取所有符合条件的 CategoryID 集合
            var categoryIds = _dbManager.Query<int>(
                "Category",
                ["Id"],
                "WHERE FirstCategory = @CategoryName OR SecondCategory = @CategoryName",
                new { CategoryName = categoryName }).ToList();

            // 2. 如果找不到对应的分类，返回空集合
            if (categoryIds.Count == 0) return [];

            // 3. 根据 CategoryIDs 查询方剂表
            var formulations = _dbManager.Query<Formulation>(
                "Formulation",
                ["*"],
                "WHERE CategoryId IN @CategoryIds",
                new { CategoryIds = categoryIds }).ToList();

            foreach (var formulation in formulations)
            {
                // 查询方剂的组成
                formulation.Compositions =
                    new ObservableCollection<FormulationComposition>(GetFormulationCompositions(formulation.Id));
                formulation.FormulationImage =
                    _dbManager.QuerySingleOrDefault<FormulationImage>("FormulationImage",
                        "WHERE FormulationId = @FormulationId",
                        ["*"],
                        new { FormulationId = formulation.Id });
            }

            // 4. 返回查询结果
            return formulations;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"根据分类名称 {categoryName} 获取方剂时发生错误");
            throw;
        }
    }

    public IEnumerable<Drug> GetDrugs()
    {
        var drugList = _dbManager.Query<Drug>("Drug", ["*"]).ToList();
        foreach (var drug in drugList)
            drug.DrugImage =
                _dbManager.QuerySingleOrDefault<DrugImage>("DrugImage",
                    "WHERE DrugId = @DrugId",
                    ["*"],
                    new { DrugId = drug.Id });

        return drugList;
    }

    // 获取所有方剂
    public IEnumerable<Formulation> GetFormulations()
    {
        var list = _dbManager.Query<Formulation>("Formulation", ["*"]);
        var formulations = list.ToList();
        foreach (var formulation in formulations)
            formulation.Compositions =
                new ObservableCollection<FormulationComposition>(GetFormulationCompositions(formulation.Id));
        return formulations;
    }

    // 获取方剂的组成
    public IEnumerable<FormulationComposition> GetFormulationCompositions(int formulationId)
    {
        return _dbManager.Query<FormulationComposition>("FormulationComposition", ["*"],
            "WHERE FormulationId = @FormulationId", new { FormulationId = formulationId });
    }

    public void InsertFormulation(Formulation formulation)
    {
        _dbManager.Insert("Formulation",
            ("Name", formulation.Name)!,
            ("CategoryId", formulation.CategoryId),
            ("Usage", formulation.Usage)!,
            ("Effect", formulation.Effect)!,
            ("Indication", formulation.Indication)!,
            ("Disease", formulation.Disease)!,
            ("Application", formulation.Application)!,
            ("Supplement", formulation.Supplement)!,
            ("Song", formulation.Song)!,
            ("Notes", formulation.Notes)!,
            ("Source", formulation.Source)!);
        // 插入方剂组成和图片
        foreach (var composition in formulation.Compositions!)
        {
            composition.FormulationId = formulation.Id;
            InsertFormulationComposition(composition);
        }

        InsertFormulationImage(GetFormulationIdByName(formulation.Name!), formulation.FormulationImage.Image!);
    }

    // 更新方剂
    public void UpdateFormulation(Formulation formulation)
    {
        _dbManager.Update("Formulation",
            [
                ("Name", formulation.Name)!,
                ("CategoryId", formulation.CategoryId),
                ("Usage", formulation.Usage)!,
                ("Effect", formulation.Effect)!,
                ("Indication", formulation.Indication)!,
                ("Disease", formulation.Disease)!,
                ("Application", formulation.Application)!,
                ("Supplement", formulation.Supplement)!,
                ("Song", formulation.Song)!,
                ("Notes", formulation.Notes)!,
                ("Source", formulation.Source)!
            ],
            "WHERE Id = @Id", "Id", formulation.Id);

        // 更新方剂组成和图片
        var oldCompositions = GetFormulationCompositions(formulation.Id).ToList();
        foreach (var oldComposition in oldCompositions.Where(oldComposition =>
                     formulation.Compositions!.All(c => c.Id != oldComposition.Id)))
            DeleteFormulation(oldComposition.Id);
        foreach (var composition in formulation.Compositions!)
            if (composition.Id == 0)
            {
                composition.FormulationId = formulation.Id;
                InsertFormulationComposition(composition);
            }
            else
            {
                _dbManager.Update("FormulationComposition",
                    [
                        ("DrugId", composition.DrugId),
                        ("DrugName", composition.DrugName)!,
                        ("Effect", composition.Effect)!,
                        ("Position", composition.Position)!,
                        ("Notes", composition.Notes)!
                    ],
                    "WHERE Id = @Id",
                    "Id", composition.Id);
            }

        if (_dbManager.Query<int>("FormulationImage", ["Id"], "WHERE FormulationId = @FormulationId",
                new { FormulationId = formulation.Id }).Any())
            _dbManager.Update("FormulationImage",
                [
                    ("Image", formulation.FormulationImage.Image)!
                ], "WHERE FormulationId = @FormulationId",
                "FormulationId", formulation.Id);
        else
            InsertFormulationImage(formulation.Id, formulation.FormulationImage.Image!);
    }

    // 删除方剂
    public void DeleteFormulation(int id)
    {
        _dbManager.Delete("FormulationComposition", "WHERE Id = @Id", "Id", id);
        // 级联删除将会自动处理 FormulationComposition 和 FormulationImage 中的相关数据
    }

    // 插入方剂组成
    public int InsertFormulationComposition(FormulationComposition formulationComposition)
    {
        var drugId = -1;
        if (!string.IsNullOrEmpty(formulationComposition.DrugName))
            drugId = GetDrugIdByName(formulationComposition.DrugName!);
        return _dbManager.Insert("FormulationComposition",
            ("FormulationId", formulationComposition.FormulationId),
            ("DrugId", drugId == -1 ? null : drugId)!,
            ("DrugName", formulationComposition.DrugName)!,
            ("Effect", formulationComposition.Effect)!,
            ("Position", formulationComposition.Position)!,
            ("Notes", formulationComposition.Notes)!);
    }

    // 插入方剂图片
    public int InsertFormulationImage(int formulationId, byte[] image)
    {
        return _dbManager.Insert("FormulationImage",
            ("FormulationID", formulationId),
            ("Image", image));
    }

    public int GetDrugIdByName(string drugName)
    {
        return _dbManager.QuerySingleOrDefault<int>("Drug", "WHERE Name = @Name", ["Id"], new { Name = drugName });
    }

    public int GetFormulationIdByName(string formulationName)
    {
        return _dbManager.QuerySingleOrDefault<int>("Formulation", "WHERE Name = @Name", ["Id"],
            new { Name = formulationName });
    }

    public void InsertDrug(Drug? drug)
    {
        _dbManager.Insert("Drug",
            ("Name", drug!.Name)!,
            ("EnglishName", drug.EnglishName)!,
            ("LatinName", drug.LatinName)!,
            ("Category", drug.Category)!,
            ("Origin", drug.Origin)!,
            ("Properties", drug.Properties)!,
            ("Quality", drug.Quality)!,
            ("Taste", drug.Taste)!,
            ("Meridian", drug.Meridian)!,
            ("Effect", drug.Effect)!,
            ("Notes", drug.Notes)!,
            ("Processed", drug.Processed)!,
            ("Source", drug.Source)!);

        _dbManager.Insert("DrugImage",
            ("DrugId", GetDrugIdByName(drug.Name!)),
            ("Image", drug.DrugImage.Image)!);
    }

    public void UpdateDrug(Drug? drug)
    {
        _dbManager.Update("Drug",
            [
                ("Name", drug!.Name)!,
                ("EnglishName", drug.EnglishName)!,
                ("LatinName", drug.LatinName)!,
                ("Category", drug.Category)!,
                ("Origin", drug.Origin)!,
                ("Properties", drug.Properties)!,
                ("Quality", drug.Quality)!,
                ("Taste", drug.Taste)!,
                ("Meridian", drug.Meridian)!,
                ("Effect", drug.Effect)!,
                ("Notes", drug.Notes)!,
                ("Processed", drug.Processed)!,
                ("Source", drug.Source)!
            ], "WHERE Id = @Id",
            "Id", drug.Id);
        if (_dbManager.Query<int>("DrugImage", ["Id"], "WHERE DrugId = @DrugId", new { DrugId = drug.Id }).Any())
            _dbManager.Update("DrugImage",
                [
                    ("Image", drug.DrugImage.Image)!
                ], "WHERE DrugId = @DrugId",
                "DrugId", drug.Id);
        else
            _dbManager.Insert("DrugImage",
                ("DrugId", GetDrugIdByName(drug.Name!)),
                ("Image", drug.DrugImage.Image)!);
    }
}