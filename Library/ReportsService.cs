using Helper;
using Library.Data;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

public class ReportsService
{
    private static ReportsService _instance;
    private static readonly object _lock = new object();

    private string _connectionString;

    private ReportsService(string connStr)
    {
        _connectionString = connStr;
    }

    public static ReportsService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ReportsService(DbConfig.ConnectionString);
                    }
                }
            }
            return _instance;
        }
    }

    public string GetTopThreeBooksReport(DateTime startDate, DateTime endDate)
    {
        List<string> notices = GetBooks(startDate, endDate);

        if (notices.Count == 0)
        {
            return "За указанный период книги не выдавались.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Три самые популярные книги за период {startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}:");
        foreach (var line in notices)
        {
            sb.AppendLine(line);
        }

        return sb.ToString();
    }

    public List<string> GetBooks(DateTime startDate, DateTime endDate)
    {
        var notices = new List<string>();
        using (var con = new NpgsqlConnection(_connectionString))
        {
            con.Open();

            con.Notice += (sender, e) =>
            {

                notices.Add(e.Notice.MessageText);
            };

            using (var cmd = new NpgsqlCommand("CALL top_three_books(@sd, @ed);", con))
            {
                cmd.Parameters.Add(new NpgsqlParameter("sd", NpgsqlTypes.NpgsqlDbType.Date)).Value = startDate.Date;
                cmd.Parameters.Add(new NpgsqlParameter("ed", NpgsqlTypes.NpgsqlDbType.Date)).Value = endDate.Date;

                cmd.ExecuteNonQuery();
            }
        }

        return notices;
    }

    public string GetCalculateFinesReport(DateTime startDate, DateTime endDate)
    {
        using (var con = new NpgsqlConnection(_connectionString))
        {
            con.Open();
            using (var cmd = new NpgsqlCommand("CALL calculate_fines(@sd,@ed,@tf);", con))
            {
                cmd.Parameters.Add(new NpgsqlParameter("sd", NpgsqlTypes.NpgsqlDbType.Date)).Value = startDate.Date;
                cmd.Parameters.Add(new NpgsqlParameter("ed", NpgsqlTypes.NpgsqlDbType.Date)).Value = endDate.Date;

                var outParam = cmd.Parameters.AddWithValue("tf", DBNull.Value);
                outParam.Direction = System.Data.ParameterDirection.InputOutput;

                cmd.ExecuteNonQuery();

                var totalFine = (decimal)outParam.Value;
                return $"Расчет штрафов за период {startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}:\r\nОбщая сумма штрафов: {totalFine}";
            }
        }
    }
}
