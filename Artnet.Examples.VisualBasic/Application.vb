Imports Artnet.Models
Imports System.Globalization

Module Program
    Sub Main()
        Dim dmx = UnfilteredDmxReader.CreateSource()

        Dim key As ConsoleKey
        Dim universes() As LittleEndianUniverse = {}
        Dim selectionNumber As Integer = -1

        Do
            universes = dmx.KnownUniverses.ToArray()
            For i As Integer = 0 To universes.Length - 1
                Console.WriteLine($"[{i}] {universes(i).LsbOctet} - {universes(i).MsbSeptet}")
            Next
            Integer.TryParse(Console.ReadLine(), NumberStyles.Integer, CultureInfo.InvariantCulture, selectionNumber)
        Loop While selectionNumber < 0 OrElse selectionNumber >= universes.Length

        Dim universeNumber As LittleEndianUniverse = universes(selectionNumber)
        Dim universe = dmx.SetUniverseSubscribed(universeNumber)

        Do
            For i As Integer = 0 To universe.Data.Length - 1
                If (i Mod 16) = 0 Then
                    Console.WriteLine($"{i}: ")
                End If
                Console.Write($"{universe.Data(i)} ")
            Next
        Loop While Console.ReadKey().Key <> ConsoleKey.Q
    End Sub
End Module
