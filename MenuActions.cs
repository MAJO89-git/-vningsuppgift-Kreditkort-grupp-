using System.Diagnostics;
using Microsoft.Data.Sqlite;

internal class MenuActions
{
    internal static void GenerateData(SqliteConnection conn)
    {
        Console.Clear();
        Console.Write("Number of people to generate: ");
        var choice = Console.ReadLine();
        Console.WriteLine();

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        try
        {
            var numPeople = string.IsNullOrWhiteSpace(choice) ? 1_000_000 : int.Parse(choice);
            GeneratePeople(conn, numPeople);
        }
        catch
        {
            Console.WriteLine("Lämna tomt eller ange ett giltigt nummer");
            Console.ReadKey();
            return;
        }

        GenerateCards(conn);

        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;

        Console.WriteLine($"Done after {ts.TotalSeconds:F3} seconds");
        Console.WriteLine("Done! Press any key to return to main menu...");
        Console.ReadKey(true);
    }

    private static void GenerateCards(SqliteConnection conn) { }

    private static void GeneratePeople(SqliteConnection conn, int n)
    {
        using var tableCommand = conn.CreateCommand();
        tableCommand.CommandText =
            "CREATE TABLE IF NOT EXISTS People (Id INTEGER PRIMARY KEY, name TEXT);";
        tableCommand.ExecuteNonQuery();

        var lines = File.ReadAllLines("MOCK_DATA.csv");
        var rand = new Random();

        using (var transaction = conn.BeginTransaction())
        {
            var command = conn.CreateCommand();
            command.CommandText =
                @"
        INSERT INTO People(name)
        VALUES ($name)
    ";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "$name";
            command.Parameters.Add(parameter);

            var random = new Random();
            Console.WriteLine("Generating data...");
            Console.WriteLine();
            for (var i = 0; i < n; i++)
            {
                var firstName = lines[rand.Next(1, lines.Length)].Split(",")[0];
                var lastName = lines[rand.Next(1, lines.Length)].Split(",")[1];

                parameter.Value = $"{firstName} {lastName}";
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }

    internal static void ListCards()
    {
        throw new NotImplementedException();
    }

    internal static void ListPeople()
    {
        throw new NotImplementedException();
    }

    internal static void ListTransactions()
    {
        throw new NotImplementedException();
    }
}
