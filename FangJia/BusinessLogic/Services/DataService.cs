using Dapper;
using FangJia.BusinessLogic.Models.Data;
using FangJia.DataAccess;
using NLog;

namespace FangJia.BusinessLogic.Services
{
    public class DataService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly DbManager _dbManager;
        private const string ConnectionString = "Data Source=|DataDirectory|\\\\data.db;Version=3;";  // 直接在类内部写定 connectionString

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
                ("ID", "INTEGER PRIMARY KEY AUTOINCREMENT"),
                ("FirstCategory", "TEXT"),
                ("SecondCategory", "TEXT"));

            // 方剂表，设置外键约束和级联删除
            _dbManager.CreateTableIfNotExists("Formulation",
                ("ID", "INTEGER PRIMARY KEY AUTOINCREMENT"),
                ("Name", "TEXT"),
                ("CategoryID", "INTEGER"),
                ("Usage", "TEXT"),
                ("Effect", "TEXT"),
                ("Indication", "TEXT"),
                ("Disease", "TEXT"),
                ("Application", "TEXT"),
                ("Supplement", "TEXT"),
                ("Song", "TEXT"),
                ("Notes", "TEXT"),
                ("Source", "TEXT"),
                ("FOREIGN KEY(CategoryID)", " REFERENCES Category(ID) ON DELETE CASCADE"));

            // 方剂图片表，设置外键约束和级联删除
            _dbManager.CreateTableIfNotExists("FormulationImage",
                ("ID", "INTEGER PRIMARY KEY AUTOINCREMENT"),
                ("FormulationID", "INTEGER"),
                ("Image", "BLOB"),
                ("FOREIGN KEY(FormulationID)", "REFERENCES Formulation(ID) ON DELETE CASCADE"));

            // 方剂组成表，设置外键约束和级联删除
            _dbManager.CreateTableIfNotExists("FormulationComposition",
                ("ID", "INTEGER PRIMARY KEY AUTOINCREMENT"),
                ("FormulationID", "INTEGER"),
                ("DrugID", "INTEGER"),
                ("DrugName", "TEXT"),
                ("Effect", "TEXT"),
                ("Position", "TEXT"),
                ("Notes", "TEXT"),
                ("FOREIGN KEY(FormulationID)", "REFERENCES Formulation(ID) ON DELETE CASCADE"));

            // 药物表
            _dbManager.CreateTableIfNotExists("Drug",
                ("ID", "INTEGER PRIMARY KEY AUTOINCREMENT"),
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
                ("ID", "INTEGER PRIMARY KEY AUTOINCREMENT"),
                ("DrugID", "INTEGER"),
                ("Image", "BLOB"),
                ("FOREIGN KEY(DrugID)", "REFERENCES Drug(ID) ON DELETE CASCADE"));
        }

        // 插入分类
        public int InsertCategory(string firstCategory, string secondCategory)
        {
            return _dbManager.Insert("Category",
                ("FirstCategory", firstCategory),
                ("SecondCategory", secondCategory));
        }

        // 插入方剂
        public int InsertFormulation(string name, int categoryId, string usage, string effect, string indication, string disease, string application, string supplement, string song, string notes, string source)
        {
            var formulationId = _dbManager.Insert("Formulation",
                ("Name", name),
                ("CategoryID", categoryId),
                ("Usage", usage),
                ("Effect", effect),
                ("Indication", indication),
                ("Disease", disease),
                ("Application", application),
                ("Supplement", supplement),
                ("Song", song),
                ("Notes", notes),
                ("Source", source));

            // 同步插入相关的组成和图片
            // 例如：
            // InsertFormulationComposition(formulationId, ...);
            // InsertFormulationImage(formulationId, ...);

            return formulationId;
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
                    {
                        category.SubCategories.Add(new Category { Name = secondCategoryName });
                    }

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

        public IEnumerable<Formulation> GetFormulationsByName(string? categoryName)
        {
            try
            {
                // 1. 查询分类表，获取所有符合条件的 CategoryID 集合
                var categoryIds = _dbManager.Query<int>(
                    "Category",
                    ["ID"],
                    "WHERE FirstCategory = @CategoryName OR SecondCategory = @CategoryName",
                    new { CategoryName = categoryName }).ToList();

                // 2. 如果找不到对应的分类，返回空集合
                if (categoryIds.Count == 0)
                {
                    return [];
                }

                // 3. 根据 CategoryIDs 查询方剂表
                var formulations = _dbManager.Query<Formulation>(
                    "Formulation",
                    ["*"],
                    "WHERE CategoryID IN @CategoryIds",
                    new { CategoryIds = categoryIds });

                // 4. 返回查询结果
                return formulations;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"根据分类名称 {categoryName} 获取方剂时发生错误");
                throw;
            }
        }

        // 获取所有方剂
        public IEnumerable<Formulation> GetFormulations()
        {
            return _dbManager.Query<Formulation>("Formulation", ["*"]);
        }

        // 获取指定分类的方剂
        public IEnumerable<Formulation> GetFormulationsByCategoryId(int categoryId)
        {
            return _dbManager.Query<Formulation>("Formulation", ["*"], "WHERE CategoryID = @CategoryID", new { CategoryID = categoryId });
        }

        // 获取方剂的组成
        public IEnumerable<FormulationComposition> GetFormulationCompositions(int formulationId)
        {
            return _dbManager.Query<FormulationComposition>("FormulationComposition", ["*"], "WHERE FormulationID = @FormulationID", new { FormulationID = formulationId });
        }

        // 更新方剂
        public void UpdateFormulation(int id, string name, int categoryId, string usage, string effect, string indication, string disease, string application, string supplement, string song, string notes, string source)
        {
            _dbManager.Update("Formulation",
                [("Name", name),
                ("CategoryID", categoryId),
                ("Usage", usage),
                ("Effect", effect),
                ("Indication", indication),
                ("Disease", disease),
                ("Application", application),
                ("Supplement", supplement),
                ("Song", song),
                ("Notes", notes),
                ("Source", source)],
                "WHERE ID = @ID", new { ID = id });

            // 同步更新相关的组成和图片
            // 这里需要根据实际情况，更新 `FormulationComposition` 和 `FormulationImage` 数据
        }

        // 删除方剂
        public void DeleteFormulation(int id)
        {
            _dbManager.Delete("Formulation", "WHERE ID = @ID", new { ID = id });
            // 级联删除将会自动处理 FormulationComposition 和 FormulationImage 中的相关数据
        }

        // 插入方剂组成
        public int InsertFormulationComposition(int formulationId, int drugId, string drugName, string effect, string position, string notes)
        {
            return _dbManager.Insert("FormulationComposition",
                ("FormulationID", formulationId),
                ("DrugID", drugId),
                ("DrugName", drugName),
                ("Effect", effect),
                ("Position", position),
                ("Notes", notes));
        }

        // 插入方剂图片
        public int InsertFormulationImage(int formulationId, byte[] image)
        {
            return _dbManager.Insert("FormulationImage",
                ("FormulationID", formulationId),
                ("Image", image));
        }
    }
}
