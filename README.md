.Net Standard Implementation of ArtNet
======================================

An ArtNet library for C# and VB.Net developers. It's _very vaguely_ based on @mikeCodesDotNET's 10 year old project which in turn is based on the [Architecture for Control Networks (ACN)](http://acn.codeplex.com) project codebase of which at this point is probably nothing left anymore. I meant to do some *light* refactorings and tweak some stuff for my particular usecase. But that got out of hand.

It's implemented in .Net Standard so should theoretically work with old-world .Net and new-world .Net. It's cross platform and any platform peculiarities have been accounted for. 

I put good effort in keeping the allocations during runtime and/or GC hits to a minimum because I'm on the special bus like that. There are two regrettable `MemoryMarshal.Read<>` calls I don't super love which may allocate a bit here and there during the early life of the application, but this calms down b/c .Net figures out pretty quickly there's only going to be so many instances of that struct referenced at a time.

If you ended up here wondering why it's a bit of a bother trying to find suitable ArtNet libraries for .Net, I would recommend to do as I did and churn through the spec and implement your own. It's definitely the protocol of all time.

This library mostly caters towards making your .Net app behave like a fixture, but can theoretically transmit too. Check out the `DependencyInjectionExtensions` to figure out how to set up the services to suit your needs, or hit the ground running by doing:

```
var dmx = UnfilteredDmxReader.CreateSource(@"^192\.168\.112"); // replace your IP prefix
```

This will produce an object that keeps an eye on all ArtNet universes in your subnet. This will also dump some diagnostic info in your console (it'll be needed probably).
Note that we don't listen on `IpAddress.Any` but instead only to broadcasts on our local subnet. That's because my particular usecase may involve being a member of multiple different LANs of VLANs with distinct ArtNets I may have to differentiate. 

Now use this to figure out which universes exist:

```
dmx.KnownUniverses
```

Now to select a universe either for example 

```
// if you don't care and just want one that's broadcasting:
var anyUniverseNumber = dmx.KnownUniverses.ElementOrDefault(0); 

// if you know exactly which universe we're going to be listening in on:
var specificUniverseNumber = new LittleEndianUniverse() { LsbOctet = 0, MsbSeptet = 0 };
```

If you got your universe selected, you may use this to tell the datagram gargler you're going to need DMX updates:

```
var universe = dmx.SetUniverseSubscribed(universeNumber);
```

Note that `SetUniverseSubscribed` will also succeed for Universes that haven't been detected into `KnownUniverses` yet. But DMX values will always stay at 0 until that universe actually comes up.

To access DMX data then, use for example:

```
byte amountOfSmoke = universe.Data[SMOKE_MACHINE_ADDRESS];
```

`universe.Data` will be up to date for the lifetime of the application. You should probably dispose some stuff if you're done using it but I don't think I exposed that quite yet on the assumption any consumer of these services is going to be using DMX for the lifetime of the application.