using Dapper;
using NLog;
using System.Data.SQLite;

namespace FangJia.DataAccess;

public class DbManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private string _connectionString = null!;

    public void SetConnection(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// 创建表，如果表不存在则创建
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnsDefinitions"></param>
    public void CreateTableIfNotExists(string tableName, params (string, string)[] columnsDefinitions)
    {
        try
        {
            Logger.Info($"检查表 {tableName} 是否存在，若不存在则创建");
            var columns = string.Join(", ", columnsDefinitions.Select(cd => $"{cd.Item1} {cd.Item2}"));
            var sql = $"CREATE TABLE IF NOT EXISTS {tableName} ({columns});";
            Execute(sql);
            Logger.Info($"表 {tableName} 已确保存在");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"创建表 {tableName} 时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 执行 SQL 语句
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="param"></param>
    public void Execute(string sql, object param = null!)
    {
        try
        {
            Logger.Debug($"执行 SQL: {sql}");
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            connection.Execute(sql, param);
            Logger.Info("SQL 执行成功");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "执行 SQL 时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 查询表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="whereClause"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public IEnumerable<T> Query<T>(string tableName, string whereClause = "", params object[] parameters)
    {
        try
        {
            Logger.Debug($"查询表 {tableName}");
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            var sql = $"SELECT * FROM {tableName} {whereClause}";
            var dynamicParameters = BuildDynamicParameters(parameters);
            Logger.Debug($"执行查询 SQL: {sql}");
            return connection.Query<T>(sql, dynamicParameters);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"查询表 {tableName} 时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 查询表，获取单个记录
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="whereClause"></param>
    /// <param name="columnNames"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public T QuerySingleOrDefault<T>(string tableName, string whereClause, string[] columnNames, object parameters)
    {
        try
        {
            Logger.Info($"查询表 {tableName}，获取单个记录");
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            var sanitizedColumnNames =
                string.Join(", ", columnNames.Select(cn => cn.Replace("\"", "\"\""))); // 防止 SQL 注入
            var sql = $"SELECT {sanitizedColumnNames} FROM {tableName} {whereClause}";
            Logger.Debug($"执行查询单个记录 SQL: {sql}");
            return connection.QuerySingleOrDefault<T>(sql, parameters)!;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"查询表 {tableName} 获取单个记录时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 插入数据
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnValues"></param>
    /// <returns></returns>
    public int Insert(string tableName, params (string, object)[] columnValues)
    {
        try
        {
            Logger.Info($"向表 {tableName} 插入数据");
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            var columns = string.Join(", ", columnValues.Select(cv => cv.Item1));
            var values = string.Join(", ", columnValues.Select(cv => "@" + cv.Item1));
            var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({values}); SELECT last_insert_rowid();";
            var parameters = new DynamicParameters();
            foreach (var (key, value) in columnValues) parameters.Add(key, value);
            Logger.Debug($"执行插入 SQL: {sql}");
            return connection.QuerySingle<int>(sql, parameters);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"向表 {tableName} 插入数据时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 更新数据
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="columnValues"></param>
    /// <param name="whereClause"></param>
    /// <param name="parameters"></param>
    public void Update(string tableName, (string, object)[] columnValues, string whereClause, params object[] parameters)
    {
        try
        {
            Logger.Info($"更新表 {tableName}");
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            var setClause = string.Join(", ", columnValues.Select(cv => $"{cv.Item1} = @{cv.Item1}"));
            var sql = $"UPDATE {tableName} SET {setClause} {whereClause}";
            var dynamicParameters = BuildDynamicParameters(parameters);
            foreach (var (key, value) in columnValues) dynamicParameters.Add(key, value);
            Logger.Debug($"执行更新 SQL: {sql}");
            connection.Execute(sql, dynamicParameters);
            Logger.Info("更新操作成功");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"更新表 {tableName} 时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 删除数据
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="whereClause"></param>
    /// <param name="parameters"></param>
    public void Delete(string tableName, string whereClause, params object[] parameters)
    {
        try
        {
            Logger.Info($"从表 {tableName} 删除数据");
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            var sql = $"DELETE FROM {tableName} {whereClause}";
            var dynamicParameters = BuildDynamicParameters(parameters);
            Logger.Debug($"执行删除 SQL: {sql}");
            connection.Execute(sql, dynamicParameters);
            Logger.Info("删除操作成功");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"从表 {tableName} 删除数据时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 构建动态参数
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    private static DynamicParameters BuildDynamicParameters(params object[] parameters)
    {
        var dynamicParameters = new DynamicParameters();
        for (var i = 0; i < parameters.Length; i += 2)
            if (i + 1 < parameters.Length)
                dynamicParameters.Add(parameters[i].ToString()!, parameters[i + 1]);
        return dynamicParameters;
    }
}