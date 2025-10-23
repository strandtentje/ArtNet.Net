using Artnet;
using Artnet.Models;
using System.Globalization;

// This happens to be the subnet in the testing subnet in which the lighting controller lives
// It isn't directed at 10.x or 2.x in particular, instead yelling into the 255.255.255.255
// And hoping we're listening.
var dmx = UnfilteredDmxReader.CreateSource(@"^192\.168\.112");

ConsoleKey key;

LittleEndianUniverse[] universes = [];
int selectionNumber = -1;
int i = 0;
do
{
    universes = dmx.KnownUniverses.ToArray();
    for (i = 0; i < universes.Length; i++)
        Console.WriteLine($"[{i}] {universes[i].LsbOctet} - {universes[i].MsbSeptet}");
    if (i == 0)
        Console.WriteLine("No universes. Press enter to query again.");
    int.TryParse(Console.ReadLine(), CultureInfo.InvariantCulture, out selectionNumber);
} while (selectionNumber < 0 || selectionNumber >= universes.Length);

LittleEndianUniverse universeNumber = universes[selectionNumber];
var universe = dmx.SetUniverseSubscribed(universeNumber);

do
{
    for (i = 0; i < universe.Data.Length; i++)
    {
        if ((i % 16) == 0)
        {
            Console.WriteLine($"{i}: ");
        }
        Console.Write($"{universe.Data[i]} ");
    }
} while (Console.ReadKey().Key != ConsoleKey.Q);


