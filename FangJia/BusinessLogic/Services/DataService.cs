using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using FangJia.BusinessLogic.Models.Data;
using FangJia.DataAccess;
using NLog;
using Drug = FangJia.BusinessLogic.Models.Data.Drug;

namespace FangJia.BusinessLogic.Services;

/// <summary>
/// 数据服务类，负责与数据库交互，提供数据的增删改查功能。
/// </summary>
[SuppressMessage("ReSharper", "HeapView.ObjectAllocation")]
public class DataService
{
	private static readonly Logger    Logger = LogManager.GetCurrentClassLogger();
	private readonly        DbManager _dbManager;

	/// <summary>
	/// 数据库连接字符串，直接在类内部定义。
	/// </summary>
	private const string
		ConnectionString = "Data Source=|DataDirectory|\\\\data.db;Version=3;"; // 直接在类内部写定 connectionString

	/// <summary>
	/// 构造函数，初始化数据库管理器并设置连接字符串。
	/// </summary>
	/// <param name="dbManager">数据库管理器实例</param>
	public DataService(DbManager dbManager)
	{
		_dbManager = dbManager;
		_dbManager.SetConnection(ConnectionString);
		_ = CreateTablesAsync(); // 异步创建表
	}

	/// <summary>
	/// 异步创建数据库表。如果表不存在，则创建相应的表。
	/// </summary>
	/// <remarks>
	/// 实现步骤：
	/// 1. 检查并创建 "Category" 表，包含主键和两个分类字段。
	/// 2. 检查并创建 "Formulation" 表，包含主键、分类外键和其他相关字段，设置外键约束。
	/// 3. 检查并创建 "FormulationImage" 表，包含主键、方剂外键和图片字段，设置外键约束。
	/// 4. 检查并创建 "FormulationComposition" 表，包含主键、方剂外键、药物外键和其他相关字段，设置外键约束。
	/// 5. 检查并创建 "Drug" 表，包含主键和其他药物相关字段。
	/// 6. 检查并创建 "DrugImage" 表，包含主键、药物外键和图片字段，设置外键约束。
	/// </remarks>
	private async Task CreateTablesAsync()
	{
		// 创建 "Category" 表
		await _dbManager.CreateTableIfNotExistsAsync
			("Category",
			 ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"),
			 ("FirstCategory", "TEXT"),
			 ("SecondCategory", "TEXT"));

		// 创建 "Formulation" 表，并设置外键约束
		await _dbManager.CreateTableIfNotExistsAsync
			("Formulation",
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

		// 创建 "FormulationImage" 表，并设置外键约束
		await _dbManager.CreateTableIfNotExistsAsync
			("FormulationImage",
			 ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"),
			 ("FormulationId", "INTEGER"),
			 ("Image", "BLOB"),
			 ("FOREIGN KEY(FormulationId)",
			  "REFERENCES Formulation(Id) ON DELETE CASCADE"));

		// 创建 "FormulationComposition" 表，并设置外键约束
		await _dbManager.CreateTableIfNotExistsAsync
			("FormulationComposition",
			 ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"),
			 ("FormulationId", "INTEGER"),
			 ("DrugId", "INTEGER"),
			 ("DrugName", "TEXT"),
			 ("Effect", "TEXT"),
			 ("Position", "TEXT"),
			 ("Notes", "TEXT"),
			 ("FOREIGN KEY(FormulationId)",
			  "REFERENCES Formulation(Id) ON DELETE CASCADE"),
			 ("FOREIGN KEY(DrugId)", "REFERENCES Drug(Id) ON DELETE CASCADE")
			);

		// 创建 "Drug" 表
		await _dbManager.CreateTableIfNotExistsAsync
			("Drug",
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

		// 创建 "DrugImage" 表，并设置外键约束
		await _dbManager.CreateTableIfNotExistsAsync
			("DrugImage",
			 ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"),
			 ("DrugId", "INTEGER"),
			 ("Image", "BLOB"),
			 ("FOREIGN KEY(DrugId)", "REFERENCES Drug(Id) ON DELETE CASCADE"));
	}

	/// <summary>
	/// 异步插入一个新的分类记录到 "Category" 表中。
	/// </summary>
	/// <param name="firstCategory">一级分类名称</param>
	/// <param name="secondCategory">二级分类名称</param>
	/// <returns>插入的分类记录的ID</returns>
	public async Task<int> InsertCategoryAsync(string firstCategory, string secondCategory)
	{
		return await _dbManager.InsertAsync
			       ("Category",
			        ("FirstCategory", firstCategory),
			        ("SecondCategory", secondCategory));
	}

	/// <summary>
	/// 异步获取所有分类及其子分类。
	/// </summary>
	/// <returns>包含所有分类及其子分类的集合</returns>
	/// <remarks>
	/// 实现步骤：
	/// 1. 查询所有一级分类。
	/// 2. 对于每个一级分类，查询其对应的二级分类。
	/// 3. 将查询结果组织成一个包含一级分类及其子分类的集合。
	/// </remarks>
	public async Task<IEnumerable<Category>> GetCategoriesAsync()
	{
		try
		{
			// 查询所有一级分类，并按一级分类分组
			var firstCategories = await _dbManager.QueryAsync<string>
				                      ("Category",
				                       ["FirstCategory"],
				                       "WHERE FirstCategory IS NOT NULL GROUP BY FirstCategory");

			// 初始化分类列表
			var categories = new List<Category>();

			// 遍历每个一级分类
			foreach (var firstCategoryName in firstCategories)
			{
				// 创建一个新的分类对象，并设置一级分类名称
				var category = new Category
				               {
					               Name          = firstCategoryName,
					               SubCategories = []
				               };

				// 查询与当前一级分类关联的二级分类
				var parameters = new { FirstCategory = firstCategoryName };
				var secondCategories = await _dbManager.QueryAsync<string>
					                       ("Category",
					                        ["SecondCategory"],
					                        "WHERE FirstCategory = @FirstCategory AND SecondCategory IS NOT NULL GROUP BY SecondCategory",
					                        parameters);

				// 遍历每个二级分类，并将其添加到当前一级分类的子分类列表中
				foreach (var secondCategoryName in secondCategories)
				{
					category.SubCategories.Add(new Category { Name = secondCategoryName });
				}

				// 将当前一级分类及其子分类添加到分类列表中
				categories.Add(category);
			}

			// 返回包含所有分类及其子分类的集合
			return categories;
		}
		catch (Exception ex)
		{
			// 记录错误日志，并重新抛出异常
			Logger.Error(ex, "获取分类时发生错误");
			throw;
		}
	}

	/// <summary>
	/// 异步获取分类ID通过分类名称。
	/// </summary>
	/// <param name="name">分类名称</param>
	/// <returns>分类ID</returns>
	/// <remarks>
	/// 实现步骤：
	/// 1. 查询 "Category" 表，查找与给定分类名称匹配的一级或二级分类。
	/// 2. 返回匹配的分类ID。
	/// </remarks>
	public async Task<int> GetCategoryIdByNameAsync(string name)
	{
		return await _dbManager.QuerySingleOrDefaultAsync<int>
			       ("Category",
			        "WHERE FirstCategory = @CategoryName OR SecondCategory = @CategoryName",
			        ["Id"],
			        new { CategoryName = name });
	}

	/// <summary>
	/// 异步获取方剂通过分类名称。
	/// </summary>
	/// <param name="categoryName">分类名称</param>
	/// <returns>包含方剂的集合</returns>
	/// <remarks>
	/// 实现步骤：
	/// 1. 查询 "Category" 表，查找与给定分类名称匹配的一级或二级分类的ID。
	/// 2. 如果找到分类ID，查询 "Formulation" 表，获取与这些分类ID关联的所有方剂。
	/// 3. 对于每个方剂，查询其组成和图片，并将其附加到方剂对象中。
	/// 4. 返回包含所有方剂的集合。
	/// </remarks>
public async Task<IEnumerable<Formulation>> GetFormulationsByCategoryNameAsync(string? categoryName)
{
    try
    {
        Logger.Info($"开始根据分类名称 {categoryName} 获取方剂");

        // Step 1: 查询 Category 表，获取与分类名称匹配的一级或二级分类的ID
        var categoryIds = (await _dbManager.QueryAsync<int>(
            "Category",
            ["Id"],
            "WHERE FirstCategory = @CategoryName OR SecondCategory = @CategoryName",
            new { CategoryName = categoryName }
        )).ToList();

        // 如果没有找到匹配的分类ID，返回空集合
        if (categoryIds.Count == 0)
        {
            Logger.Warn($"分类名称 {categoryName} 未找到对应的分类ID");
            return [];
        }

        // Step 2: 查询 Formulation 表，获取与这些分类ID关联的所有方剂
        var formulations = (await _dbManager.QueryAsync<Formulation>(
            "Formulation",
            ["*"],
            "WHERE CategoryId IN @CategoryIds",
            new { CategoryIds = categoryIds }
        )).ToList();

        if (formulations.Count == 0)
        {
            Logger.Warn($"分类名称 {categoryName} 未找到任何方剂");
            return formulations;
        }

        // 获取所有 FormulationId
        var formulationIds = formulations.Select(f => f.Id).ToArray();

        // Step 3: 批量查询所有组成
        var compositions = (await _dbManager.QueryAsync<FormulationComposition>(
            "FormulationComposition",
            ["*"],
            "WHERE FormulationId IN @FormulationIds",
            new { FormulationIds = formulationIds }
        )).ToLookup(c => c.FormulationId);

        // Step 4: 批量查询所有图片
        var formulationImages = (await _dbManager.QueryAsync<FormulationImage>(
            "FormulationImage",
            ["*"],
            "WHERE FormulationId IN @FormulationIds",
            new { FormulationIds = formulationIds }
        )).ToDictionary(img => img.FormulationId);

        // Step 5: 将组成和图片附加到对应的方剂对象
        foreach (var formulation in formulations)
        {
            formulation.Compositions     = new ObservableCollection<FormulationComposition>(compositions[formulation.Id].ToList());
            formulation.FormulationImage = formulationImages.GetValueOrDefault(formulation.Id)!;
        }

        Logger.Info($"成功根据分类名称 {categoryName} 获取 {formulations.Count} 个方剂");
        return formulations;
    }
    catch (Exception ex)
    {
        // 记录错误日志，并重新抛出异常
        Logger.Error(ex, $"根据分类名称 {categoryName} 获取方剂时发生错误");
        throw;
    }
}


	/// <summary>
	/// 异步获取所有药物及其相关图片。
	/// </summary>
	/// <returns>包含所有药物及其图片的集合</returns>
	/// <remarks>
	/// 实现步骤：
	/// 1. 查询 "Drug" 表，获取所有药物记录。
	/// 2. 对于每个药物，查询 "DrugImage" 表，获取与该药物关联的图片。
	/// 3. 将查询到的图片附加到对应的药物对象中。
	/// 4. 返回包含所有药物及其图片的集合。
	/// </remarks>
	public async Task<IEnumerable<Drug>> GetDrugs()
	{
		try
		{
			Logger.Info("开始获取药物列表及其图片信息");

			// Step 1: 查询 Drug 表获取所有药物记录
			var drugList = (await _dbManager.QueryAsync<Drug>("Drug", ["*"])).ToList();

			if (!drugList.Any())
			{
				Logger.Warn("未查询到任何药物数据");
				return drugList;
			}

			// Step 2: 构建一个查询所有图片的 DrugId 条件
			var drugIds             = drugList.Select(d => d.Id).ToArray();
			var drugIdsPlaceholders = string.Join(", ", drugIds.Select((_, index) => $"@DrugId{index}"));
			var whereClause         = $"WHERE DrugId IN ({drugIdsPlaceholders})";

			// Step 3: 构建参数对象
			var parameters = drugIds
			                 .Select((id, index) => new KeyValuePair<string, object>($"DrugId{index}", id))
			                 .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			// Step 4: 一次性查询所有药物图片
			var drugImages =
				(await _dbManager.QueryAsync<DrugImage>("DrugImage", ["*"], whereClause, parameters))
				.ToDictionary(img => img.DrugId);

			// Step 5: 将图片附加到对应的药物对象
			foreach (var drug in drugList)
			{
				drug.DrugImage = drugImages.GetValueOrDefault(drug.Id)!;
			}

			Logger.Info("成功获取所有药物及其图片信息");
			return drugList;
		}
		catch (Exception ex)
		{
			Logger.Error(ex, "获取药物列表时发生错误");
			throw;
		}
	}


	/// <summary>
	/// 异步获取所有方剂及其组成。
	/// </summary>
	/// <returns>包含所有方剂及其组成的集合</returns>
	/// <remarks>
	/// 实现步骤：
	/// 1. 查询 "Formulation" 表，获取所有方剂记录。
	/// 2. 对于每个方剂，查询 "FormulationComposition" 表，获取与该方剂关联的组成信息。
	/// 3. 将查询到的组成信息附加到对应的方剂对象中。
	/// 4. 返回包含所有方剂及其组成的集合。
	/// </remarks>
	public async Task<IEnumerable<Formulation>> GetFormulations()
	{
		// 查询 "Formulation" 表，获取所有方剂记录
		var list = await _dbManager.QueryAsync<Formulation>(
		                                                    "Formulation",
		                                                    [@"*"]
		                                                   );
		var formulations = list.ToList();

		// 遍历每个方剂，查询其关联的组成信息，并将其附加到方剂对象中
		foreach (var formulation in formulations)
		{
			formulation.Compositions =
				[.. await GetFormulationCompositions(formulation.Id)];
		}

		// 返回包含所有方剂及其组成的集合
		return formulations;
	}

	/// <summary>
	/// 异步获取指定方剂的组成信息。
	/// </summary>
	/// <param name="formulationId">方剂ID</param>
	/// <returns>包含方剂组成信息的集合</returns>
	/// <remarks>
	/// 实现步骤：
	/// 1. 查询 "FormulationComposition" 表，获取与指定方剂ID关联的所有组成记录。
	/// 2. 返回包含方剂组成信息的集合。
	/// </remarks>
	public async Task<IEnumerable<FormulationComposition>> GetFormulationCompositions(int formulationId)
	{
		// 查询 "FormulationComposition" 表，获取与指定方剂ID关联的所有组成记录
		return await _dbManager.QueryAsync<FormulationComposition>
			       ("FormulationComposition", ["*"],
			        "WHERE FormulationId = @FormulationId",
			        new { FormulationId = formulationId });
	}

	/// <summary>
	/// 异步插入一个新的方剂及其组成和图片。
	/// </summary>
	/// <param name="formulation">方剂对象</param>
	/// <remarks>
	/// 实现步骤：
	/// 1. 插入方剂记录到 "Formulation" 表中。
	/// 2. 对于方剂的每个组成，插入组成记录到 "FormulationComposition" 表中。
	/// 3. 插入方剂图片记录到 "FormulationImage" 表中。
	/// </remarks>
	public async Task InsertFormulation(Formulation formulation)
	{
		// 插入方剂记录到 "Formulation" 表中
		await _dbManager.InsertAsync
			("Formulation",
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

		await InsertFormulationImage(await GetFormulationIdByName(formulation.Name!),
		                             formulation.FormulationImage.Image!);
	}

	/// <summary>
	/// 异步更新方剂及其组成和图片。
	/// </summary>
	/// <param name="formulation">方剂对象</param>
	/// <remarks>
	/// 实现步骤：
	/// 1. 更新 "Formulation" 表中的方剂记录。
	/// 2. 获取当前方剂的所有组成记录。
	/// 3. 对于每个旧的组成记录，如果新的组成记录中不存在，则删除该旧组成记录。
	/// 4. 对于每个新的组成记录，如果其ID为0，则插入新的组成记录；否则，更新该组成记录。
	/// 5. 更新或插入方剂图片记录。
	/// </remarks>
	public async Task UpdateFormulation(Formulation formulation)
	{
		// 更新 "Formulation" 表中的方剂记录
		await _dbManager.UpdateAsync
			("Formulation",
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

		// 获取当前方剂的所有组成记录
		var oldCompositions = await GetFormulationCompositions(formulation.Id);

		// 对于每个旧的组成记录，如果新的组成记录中不存在，则删除该旧组成记录
		foreach (var oldComposition in oldCompositions.Where(oldComposition =>
			                                                     formulation.Compositions!
			                                                                .All(c => c.Id != oldComposition.Id)))
		{
			await DeleteFormulationComposition(oldComposition.Id);
		}

		// 对于每个新的组成记录，如果其ID为0，则插入新的组成记录；否则，更新该组成记录
		foreach (var composition in formulation.Compositions!)
		{
			if (composition.Id == 0)
			{
				composition.FormulationId = formulation.Id;
				await InsertFormulationComposition(composition);
			}
			else
			{
				await _dbManager.UpdateAsync
					("FormulationComposition",
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
		}

		// 更新或插入方剂图片记录
		var formulationIds = await _dbManager.QueryAsync<int>("FormulationImage", ["Id"],
		                                                      "WHERE FormulationId = @FormulationId",
		                                                      new { FormulationId = formulation.Id });
		if (formulationIds.Any())
		{
			await _dbManager.UpdateAsync("FormulationImage",
			                             [
				                             ("Image", formulation.FormulationImage.Image)!
			                             ], "WHERE FormulationId = @FormulationId",
			                             "FormulationId", formulation.Id);
		}
		else
		{
			await InsertFormulationImage(formulation.Id, formulation.FormulationImage.Image!);
		}
	}

	/// <summary>
	/// 异步删除方剂组成记录。
	/// </summary>
	/// <param name="id">方剂组成记录的ID</param>
	/// <remarks>
	/// 实现步骤：
	/// 1. 删除 "FormulationComposition" 表中指定ID的记录。
	/// </remarks>
	public async Task DeleteFormulationComposition(int id)
	{
		await _dbManager.DeleteAsync("FormulationComposition", "WHERE Id = @Id", "Id", id);
	}

	/// <summary>
	/// 异步插入新的方剂组成记录。
	/// </summary>
	/// <param name="formulationComposition">方剂组成对象</param>
	/// <returns>插入的方剂组成记录的ID</returns>
	/// <remarks>
	/// 实现步骤：
	/// 1. 获取药物的ID。
	/// 2. 插入新的方剂组成记录到 "FormulationComposition" 表中。
	/// </remarks>
	public async Task<int> InsertFormulationComposition(FormulationComposition formulationComposition)
	{
		var drugId = -1;
		if (!string.IsNullOrEmpty(formulationComposition.DrugName))
			drugId = await GetDrugIdByName(formulationComposition.DrugName!);
		return await _dbManager.InsertAsync
			       ("FormulationComposition",
			        ("FormulationId", formulationComposition.FormulationId),
			        ("DrugId", drugId == -1 ? null : drugId)!,
			        ("DrugName", formulationComposition.DrugName)!,
			        ("Effect", formulationComposition.Effect)!,
			        ("Position", formulationComposition.Position)!,
			        ("Notes", formulationComposition.Notes)!);
	}

	/// <summary>
	/// 异步插入新的方剂图片记录。
	/// </summary>
	/// <param name="formulationId">方剂ID</param>
	/// <param name="image">图片数据</param>
	/// <returns>插入的方剂图片记录的ID</returns>
	/// <remarks>
	/// 实现步骤：
	/// 1. 插入新的方剂图片记录到 "FormulationImage" 表中。
	/// </remarks>
	public async Task<int> InsertFormulationImage(int formulationId, byte[] image)
	{
		return await _dbManager.InsertAsync
			       ("FormulationImage",
			        ("FormulationID", formulationId),
			        ("Image", image));
	}

	/// <summary>
	/// 异步获取药物ID通过药物名称。
	/// </summary>
	/// <param name="drugName">药物名称</param>
	/// <returns>药物ID</returns>
	/// <remarks>
	/// 实现步骤：
	/// 1. 查询 "Drug" 表，查找与给定药物名称匹配的记录。
	/// 2. 返回匹配的药物ID。
	/// </remarks>
	public async Task<int> GetDrugIdByName(string drugName)
	{
		return await _dbManager.QuerySingleOrDefaultAsync<int>
			       ("Drug",
			        "WHERE Name = @Name",
			        ["Id"],
			        new { Name = drugName });
	}

	/// <summary>
	/// 异步获取方剂ID通过方剂名称。
	/// </summary>
	/// <param name="formulationName">方剂名称</param>
	/// <returns>方剂ID</returns>
	/// <remarks>
	/// 实现步骤：
	/// 1. 查询 "Formulation" 表，查找与给定方剂名称匹配的记录。
	/// 2. 返回匹配的方剂ID。
	/// </remarks>
	public async Task<int> GetFormulationIdByName(string formulationName)
	{
		return await _dbManager.QuerySingleOrDefaultAsync<int>
			       ("Formulation",
			        "WHERE Name = @Name",
			        ["Id"],
			        new { Name = formulationName });
	}

	/// <summary>
	/// 异步插入新的药物及其图片。
	/// </summary>
	/// <param name="drug">药物对象</param>
	/// <remarks>
	/// 实现步骤：
	/// 1. 插入药物记录到 "Drug" 表中。
	/// 2. 插入药物图片记录到 "DrugImage" 表中，使用插入后的药物ID。
	/// </remarks>
	public async Task InsertDrug(Drug? drug)
	{
		// 插入药物记录到 "Drug" 表中
		await _dbManager.InsertAsync
			("Drug",
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

		// 插入药物图片记录到 "DrugImage" 表中，使用插入后的药物ID
		await _dbManager.InsertAsync
			("DrugImage",
			 ("DrugId", await GetDrugIdByName(drug.Name!)),
			 ("Image", drug.DrugImage.Image)!);
	}

	/// <summary>
	/// 异步更新药物及其图片。
	/// </summary>
	/// <param name="drug">药物对象</param>
	/// <remarks>
	/// 实现步骤：
	/// 1. 更新 "Drug" 表中的药物记录。
	/// 2. 查询 "DrugImage" 表，检查是否存在与该药物ID关联的图片记录。
	/// 3. 如果存在图片记录，则更新图片记录；否则，插入新的图片记录。
	/// </remarks>
	public async Task UpdateDrug(Drug? drug)
	{
		// 更新 "Drug" 表中的药物记录
		await _dbManager.UpdateAsync
			("Drug",
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

		// 查询 "DrugImage" 表，检查是否存在与该药物ID关联的图片记录
		var ids = await _dbManager.QueryAsync<int>
			          ("DrugImage",
			           ["Id"],
			           "WHERE DrugId = @DrugId",
			           new { DrugId = drug.Id });

		// 如果存在图片记录，则更新图片记录；否则，插入新的图片记录
		if (ids.Any())
			await _dbManager.UpdateAsync
				("DrugImage",
				 [("Image", drug.DrugImage.Image)!],
				 "WHERE DrugId = @DrugId",
				 "DrugId", drug.Id);
		else
			await _dbManager.InsertAsync
				("DrugImage",
				 ("DrugId", await GetDrugIdByName(drug.Name!)),
				 ("Image", drug.DrugImage.Image)!);
	}
}
