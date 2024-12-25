namespace AISmart.Agent.GEvents;

using System;

class Program
{
    public static void Main()
    {
        string guidString = "49eea4d1-ca5c-4b69-a58c-fca972e691a6";
        Guid guid = Guid.Parse(guidString);

        Console.WriteLine(guid);
    }
}