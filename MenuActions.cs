using Microsoft.Data.Sqlite;

internal class MenuActions
{
    internal static void GenerateData(SqliteConnection conn)
    {
        Console.Clear();
        Console.Write("Number of people to generate: ");
        var choice = Console.ReadLine();

        int n = 0;
        try
        {
            n = string.IsNullOrWhiteSpace(choice) ? 1_000_000 : int.Parse(choice);
        }
        catch
        {
            Console.WriteLine("Lämna tomt eller ange ett giltigt nummer");
            Console.ReadKey();
            return;
        }

        using var tableCommand = conn.CreateCommand();
        tableCommand.CommandText =
            "CREATE TABLE IF NOT EXISTS People (Id INTEGER PRIMARY KEY, name TEXT);";
        tableCommand.ExecuteNonQuery();

        var lines = File.ReadAllLines("MOCK_DATA.csv");
        var rand = new Random();

        Console.WriteLine("\nGenerating data...\n");
        for (int i = 0; i < n; i++)
        {
            var firstName = lines[rand.Next(1, lines.Length)].Split(",")[0];
            var lastName = lines[rand.Next(1, lines.Length)].Split(",")[1];

            using var command = conn.CreateCommand();
            command.CommandText = "INSERT INTO People(name) VALUES (@name)";
            command.Parameters.AddWithValue("@name", $"{firstName} {lastName}");
            command.ExecuteNonQuery();
        }
        Console.WriteLine("Done! Press any key to return to main menu...");
        Console.ReadKey(true);
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

