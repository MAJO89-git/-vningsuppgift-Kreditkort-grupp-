using Microsoft.Data.Sqlite;

while (true)
{
    using var connection = new SqliteConnection("Data Source=cards.db");
    connection.Open();

    Console.Clear();
    Console.WriteLine("What do you want to do?");
    Console.WriteLine("1. Generate mock data");
    Console.WriteLine("2. List people");
    Console.WriteLine("3. List Cards");
    Console.WriteLine("4. List Transactions");

    Console.Write("Choice: ");
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            MenuActions.GenerateData(connection);
            break;

        case "2":
            MenuActions.ListPeople();
            break;

        case "3":
            MenuActions.ListCards();
            break;

        case "4":
            MenuActions.ListTransactions();
            break;

        default:
            Console.WriteLine("Invalid option!");
            Console.ReadKey();
            break;
    }
}
