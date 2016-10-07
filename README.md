Iambic
===
*"Easy to parse up." :)*


# Welcome!

Iambic is a modern parser library for .NET that is small, fast, and easy to embed while still having sophisticated features like automatic error recovery. Use Iambic for adding expression support to your apps, building compilers for your own programming languages, code editors with syntax highlighting and error markup, or simply for parsing structured data formats.

Iambic is released under a Simplified BSD License. See [license.txt](http://github.com/mandykoh/iambic/blob/master/LICENSE.txt) for the licensing terms.

For more information, please see the [documentation](http://wiki.github.com/mandykoh/iambic/).


# Building From Sources


## Windows

The simplest way to build on Windows is probably to use Visual Studio (the free [Community edition](https://www.visualstudio.com/vs/) with the [.NET Core tools](https://docs.microsoft.com/en-us/dotnet/articles/core/windows-prerequisites) will work). Just have the IDE build the `Naucera.Iambic` project in `main`.


## Most Other People

You will need [.NET Core](https://dotnet.github.io/) installed. Then, just do:

	dotnet restore
	dotnet build

in the `main/Naucera.Iambic` directory.

To build and run tests, use:

	dotnet test

in the `test/Naucera.Iambic.Test` directory.
