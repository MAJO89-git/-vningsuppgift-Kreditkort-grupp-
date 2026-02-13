using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Reflection.PortableExecutable;

internal class MenuActions
{
    internal static void GenerateData(SqliteConnection conn)
    {
        Console.Clear();
        Console.Write("Number of people to generate: ");
        var choice = Console.ReadLine();
        Console.WriteLine();

        
        using var pragmaCmd = conn.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA journal_mode = WAL; PRAGMA synchronous = NORMAL;";
        pragmaCmd.ExecuteNonQuery();

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

    internal static void GenerateCards(SqliteConnection conn) {
        var rand = new Random();
        var personIds = new List<int>();

        using var getIds = conn.CreateCommand();
        getIds.CommandText =
                        @"SELECT Id FROM People";
        using var reader = getIds.ExecuteReader();
        while (reader.Read())
        { personIds.Add(reader.GetInt32(0)); }

        var shuffledList = personIds.OrderBy(x => rand.Next()).ToList();

        int totalIds = shuffledList.Count;

        int g70 = (int)(totalIds * 0.7);
        int g20 = (int)(totalIds * 0.2);

        var group70 = shuffledList.Take(g70).ToList();
        var group20 = shuffledList.Skip(g70).Take(g20).ToList();
        var group10 = shuffledList.Skip(g70 + g20).ToList();

        using var tableCommand = conn.CreateCommand();
        tableCommand.CommandText =
            @"CREATE TABLE IF NOT EXISTS Cards (
                      Id INTEGER PRIMARY KEY,
                      card_number INTEGER,
                      user_id INTEGER,
                      FOREIGN KEY(user_id) REFERENCES People(id))";
        tableCommand.ExecuteNonQuery();


        GenerateCard(group70, 1); // Anropar metoden och skapar kort
        GenerateCard(group20, 2);

        foreach (var id in group10)
        {
            int cardCount = rand.Next(3, 11);
            for (int i = 0; i < cardCount; i++)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                      INSERT INTO Cards    (user_id, card_number)
                      VALUES($user_id, $card_number)";
                cmd.Parameters.AddWithValue("$user_id", id);
                cmd.Parameters.AddWithValue("$card_number", rand.Next(1000, 9999)); //SKAPA ISTÄLLET RANDOM CARD NUMBER METOD (GenerateCardNumber)
                cmd.ExecuteNonQuery();
            }




        }
        void GenerateCard(List<int> ids, int cardsAmount)
        {

            foreach (var id in ids)
            {
                for (int i = 0; i < cardsAmount; i++)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                      INSERT INTO Cards    (user_id, card_number)
                      VALUES($user_id, $card_number)";
                    cmd.Parameters.AddWithValue("$user_id", id);
                    cmd.Parameters.AddWithValue("$card_number", rand.Next(1000, 9999));
                    cmd.ExecuteNonQuery();
                }
            }
        }
        Console.WriteLine("Done! Press any key to return to main menu...");
        Console.ReadKey(true);

    }

    private static void GeneratePeople(SqliteConnection conn, int n)
    {
        using var tableCommand = conn.CreateCommand();
        tableCommand.CommandText =
            "CREATE TABLE IF NOT EXISTS People (Id INTEGER PRIMARY KEY, name TEXT);";
        tableCommand.ExecuteNonQuery();

        
        var lines = File.ReadAllLines("MOCK_DATA.csv");
        var names = lines
            .Skip(1)
            .Select(l => l.Split(','))
            .ToArray();

        var rand = new Random();

        Console.WriteLine("Generating data...");
        Console.WriteLine();

        using (var transaction = conn.BeginTransaction())
        {
            var command = conn.CreateCommand();
            command.CommandText = "INSERT INTO People(name) VALUES ($name)";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "$name";
            command.Parameters.Add(parameter);

            for (var i = 0; i < n; i++)
            {
                var row = names[rand.Next(names.Length)];
                parameter.Value = $"{row[0]} {row[1]}";
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }

    internal static void ListCards(SqliteConnection conn)
    {
        //throw new NotImplementedException();
        Console.Clear();
        Console.Write("Name to list cards for: ");
        string input = Console.ReadLine()!;

        using var getCardsCommand = conn.CreateCommand();
        getCardsCommand.CommandText = @"
                SELECT card_number 
                FROM Cards
                    JOIN People ON People.id = Cards.user_id
                WHERE name = @name
                ";
        getCardsCommand.Parameters.AddWithValue("@name", input);


        using var reader = getCardsCommand.ExecuteReader();

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        if (!reader.HasRows) Console.WriteLine("no cards");
        else
        {
            while (reader.Read())
            {
                string cardNumber = reader.GetString(0);
                Console.WriteLine(cardNumber);
            }
        }
        
        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;
        Console.CursorVisible = false;
        Console.WriteLine($"Done after {ts.TotalSeconds:F3} seconds");
        Console.Write("Press any key to return to main menu...");
        Console.ReadKey(true);
        Console.CursorVisible = true;
    }

    internal static void ListPeople(SqliteConnection conn)
    {
        using var amountCommand = conn.CreateCommand();
        amountCommand.CommandText = "SELECT COUNT(*) FROM People";

        var count = Convert.ToInt32(amountCommand.ExecuteScalar());

        Console.Clear();
        Console.WriteLine($"Antal personer i databasen ({count})");
        Console.Write("Hur många vill du visa (standard 100): ");

        string? input = Console.ReadLine();
        int amount = 100;

        if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int parsed)) amount = parsed;
        if (amount > count) amount = count;

        using var peopleCommand = conn.CreateCommand();
        peopleCommand.CommandText = "SELECT Name FROM People LIMIT @amount";
        peopleCommand.Parameters.AddWithValue("@amount", amount);

        using var reader = peopleCommand.ExecuteReader();

        Console.Clear();
        Console.WriteLine($"Visar {amount} personer:\n");
        while (reader.Read())
        {
            string name = reader.GetString(0);
            Console.WriteLine(name);
        }

        Console.CursorVisible = false;
        Console.Write("\nTryck valfri tangent för att fortsätta...");
        Console.ReadKey();
        Console.CursorVisible = true;
    }


    internal static void ListTransactions()
    {
        throw new NotImplementedException();
    }
}
