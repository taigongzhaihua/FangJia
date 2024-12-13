using FangJia.BusinessLogic.Models.Data;
using FangJia.DataAccess;
using NLog;
using Drug = FangJia.BusinessLogic.Models.Data.Drug;

namespace FangJia.BusinessLogic.Services;

public class DataService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly DbManager _dbManager;

    private const string
        ConnectionString = "Data Source=|DataDirectory|\\\\data.db;Version=3;"; // 直接在类内部写定 connectionString

    public DataService(DbManager dbManager)
    {
        _dbManager = dbManager;
        _dbManager.SetConnection(ConnectionString);
        _ = CreateTablesAsync(); // 异步创建表
    }

    public async Task CreateTablesAsync()
    {
        await _dbManager.CreateTableIfNotExistsAsync("Category",
            ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"),
            ("FirstCategory", "TEXT"),
            ("SecondCategory", "TEXT"));

        await _dbManager.CreateTableIfNotExistsAsync("Formulation",
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

        await _dbManager.CreateTableIfNotExistsAsync("FormulationImage",
            ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"),
            ("FormulationId", "INTEGER"),
            ("Image", "BLOB"),
            ("FOREIGN KEY(FormulationId)", "REFERENCES Formulation(Id) ON DELETE CASCADE"));

        await _dbManager.CreateTableIfNotExistsAsync("FormulationComposition",
            ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"),
            ("FormulationId", "INTEGER"),
            ("DrugId", "INTEGER"),
            ("DrugName", "TEXT"),
            ("Effect", "TEXT"),
            ("Position", "TEXT"),
            ("Notes", "TEXT"),
            ("FOREIGN KEY(FormulationId)", "REFERENCES Formulation(Id) ON DELETE CASCADE"),
            ("FOREIGN KEY(DrugId)", "REFERENCES Drug(Id) ON DELETE CASCADE"));

        await _dbManager.CreateTableIfNotExistsAsync("Drug",
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

        await _dbManager.CreateTableIfNotExistsAsync("DrugImage",
            ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"),
            ("DrugId", "INTEGER"),
            ("Image", "BLOB"),
            ("FOREIGN KEY(DrugId)", "REFERENCES Drug(Id) ON DELETE CASCADE"));
    }
    public async Task<int> InsertCategoryAsync(string firstCategory, string secondCategory)
    {
        return await _dbManager.InsertAsync("Category",
            ("FirstCategory", firstCategory),
            ("SecondCategory", secondCategory));
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        try
        {
            var firstCategories = await _dbManager.QueryAsync<string>(
                "Category",
                ["FirstCategory"],
                "WHERE FirstCategory IS NOT NULL GROUP BY FirstCategory");

            var categories = new List<Category>();

            foreach (var firstCategoryName in firstCategories)
            {
                var category = new Category
                {
                    Name = firstCategoryName,
                    SubCategories = []
                };

                var parameters = new { FirstCategory = firstCategoryName };
                var secondCategories = await _dbManager.QueryAsync<string>(
                    "Category",
                    ["SecondCategory"],
                    "WHERE FirstCategory = @FirstCategory AND SecondCategory IS NOT NULL GROUP BY SecondCategory",
                    parameters);

                foreach (var secondCategoryName in secondCategories)
                {
                    category.SubCategories.Add(new Category { Name = secondCategoryName });
                }

                categories.Add(category);
            }

            return categories;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "获取分类时发生错误");
            throw;
        }
    }


    public async Task<int> GetCategoryIdByNameAsync(string name)
    {
        return await _dbManager.QuerySingleOrDefaultAsync<int>("Category",
            "WHERE FirstCategory = @CategoryName OR SecondCategory = @CategoryName",
            ["Id"],
            new { CategoryName = name });
    }

    public async Task<IEnumerable<Formulation>> GetFormulationsByCategoryNameAsync(string? categoryName)
    {
        try
        {
            var categoryIds = (await _dbManager.QueryAsync<int>(
                "Category",
                ["Id"],
                "WHERE FirstCategory = @CategoryName OR SecondCategory = @CategoryName",
                new { CategoryName = categoryName })).ToList();

            if (categoryIds.Count == 0) return [];

            var formulations = (await _dbManager.QueryAsync<Formulation>(
                "Formulation",
                ["*"],
                "WHERE CategoryId IN @CategoryIds",
                new { CategoryIds = categoryIds })).ToList();

            foreach (var formulation in formulations)
            {
                formulation.Compositions = [.. await GetFormulationCompositions(formulation.Id)];
                formulation.FormulationImage = (await _dbManager.QuerySingleOrDefaultAsync<FormulationImage>(
                    "FormulationImage",
                    "WHERE FormulationId = @FormulationId",
                    ["*"],
                    new { FormulationId = formulation.Id }))!;
            }

            return formulations;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"根据分类名称 {categoryName} 获取方剂时发生错误");
            throw;
        }
    }

    public async Task<IEnumerable<Drug>> GetDrugs()
    {
        var drugList = await _dbManager.QueryAsync<Drug>("Drug", ["*"]);
        var drugs = drugList.ToList();
        foreach (var drug in drugs)
            drug.DrugImage = (await _dbManager.QuerySingleOrDefaultAsync<DrugImage>("DrugImage",
                "WHERE DrugId = @DrugId",
                ["*"],
                new { DrugId = drug.Id }))!;

        return drugs;
    }

    // 获取所有方剂
    public async Task<IEnumerable<Formulation>> GetFormulations()
    {
        var list = await _dbManager.QueryAsync<Formulation>("Formulation", ["*"]);
        var formulations = list.ToList();
        foreach (var formulation in formulations)
            formulation.Compositions =
                [.. await GetFormulationCompositions(formulation.Id)];
        return formulations;
    }

    // 获取方剂的组成
    public async Task<IEnumerable<FormulationComposition>> GetFormulationCompositions(int formulationId)
    {
        return await _dbManager.QueryAsync<FormulationComposition>("FormulationComposition", ["*"],
            "WHERE FormulationId = @FormulationId", new { FormulationId = formulationId });
    }

    public async Task InsertFormulation(Formulation formulation)
    {
        await _dbManager.InsertAsync("Formulation",
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
            await InsertFormulationComposition(composition);
        }

        await InsertFormulationImage(await GetFormulationIdByName(formulation.Name!), formulation.FormulationImage.Image!);
    }

    // 更新方剂
    public async Task UpdateFormulation(Formulation formulation)
    {
        await _dbManager.UpdateAsync("Formulation",
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
        var oldCompositions = await GetFormulationCompositions(formulation.Id);
        foreach (var oldComposition in oldCompositions.Where(oldComposition =>
                     formulation.Compositions!.All(c => c.Id != oldComposition.Id)))
            await DeleteFormulation(oldComposition.Id);
        foreach (var composition in formulation.Compositions!)
            if (composition.Id == 0)
            {
                composition.FormulationId = formulation.Id;
                await InsertFormulationComposition(composition);
            }
            else
            {
                await _dbManager.UpdateAsync("FormulationComposition",
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

        var formulationIds = await _dbManager.QueryAsync<int>("FormulationImage", ["Id"],
            "WHERE FormulationId = @FormulationId",
            new { FormulationId = formulation.Id });
        if (formulationIds.Any())
            await _dbManager.UpdateAsync("FormulationImage",
                [
                    ("Image", formulation.FormulationImage.Image)!
                ], "WHERE FormulationId = @FormulationId",
                "FormulationId", formulation.Id);
        else
            await InsertFormulationImage(formulation.Id, formulation.FormulationImage.Image!);
    }

    // 删除方剂
    public async Task DeleteFormulation(int id)
    {
        await _dbManager.DeleteAsync("FormulationComposition", "WHERE Id = @Id", "Id", id);
        // 级联删除将会自动处理 FormulationComposition 和 FormulationImage 中的相关数据
    }

    // 插入方剂组成
    public async Task<int> InsertFormulationComposition(FormulationComposition formulationComposition)
    {
        var drugId = -1;
        if (!string.IsNullOrEmpty(formulationComposition.DrugName))
            drugId = await GetDrugIdByName(formulationComposition.DrugName!);
        return await _dbManager.InsertAsync("FormulationComposition",
            ("FormulationId", formulationComposition.FormulationId),
            ("DrugId", drugId == -1 ? null : drugId)!,
            ("DrugName", formulationComposition.DrugName)!,
            ("Effect", formulationComposition.Effect)!,
            ("Position", formulationComposition.Position)!,
            ("Notes", formulationComposition.Notes)!);
    }

    // 插入方剂图片
    public async Task<int> InsertFormulationImage(int formulationId, byte[] image)
    {
        return await _dbManager.InsertAsync("FormulationImage",
            ("FormulationID", formulationId),
            ("Image", image));
    }

    public async Task<int> GetDrugIdByName(string drugName)
    {
        return await _dbManager.QuerySingleOrDefaultAsync<int>("Drug", "WHERE Name = @Name", ["Id"], new { Name = drugName });
    }

    public async Task<int> GetFormulationIdByName(string formulationName)
    {
        return await _dbManager.QuerySingleOrDefaultAsync<int>("Formulation", "WHERE Name = @Name", ["Id"],
            new { Name = formulationName });
    }

    public async Task InsertDrug(Drug? drug)
    {
        await _dbManager.InsertAsync("Drug",
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

        await _dbManager.InsertAsync("DrugImage",
            ("DrugId", GetDrugIdByName(drug.Name!)),
            ("Image", drug.DrugImage.Image)!);
    }

    public async Task UpdateDrug(Drug? drug)
    {
        await _dbManager.UpdateAsync("Drug",
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
        var ids = await _dbManager.QueryAsync<int>("DrugImage", ["Id"], "WHERE DrugId = @DrugId", new { DrugId = drug.Id });
        if (ids.Any())
            await _dbManager.UpdateAsync("DrugImage",
                [
                    ("Image", drug.DrugImage.Image)!
                ], "WHERE DrugId = @DrugId",
                "DrugId", drug.Id);
        else
            await _dbManager.InsertAsync("DrugImage",
                ("DrugId", GetDrugIdByName(drug.Name!)),
                ("Image", drug.DrugImage.Image)!);
    }
}