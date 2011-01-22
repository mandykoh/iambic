Iambic
===

# Welcome!

Iambic is a parser library for .NET that is small, fast, and easy to embed while still having sophisticated features like automatic error recovery. This makes it suitable for implementing things like syntax highlighting editors (even with runtime-configurable grammars) and composable grammars as well as traditional things such as compilers and data format parsers.

Iambic is released under a Simplified BSD License.

For more information, please see the [wiki](http://wiki.github.com/naucera/iambic/).

# Building


## Windows

The simplest way to build on Windows is probably to use Visual Studio (the free [Express edition](http://www.microsoft.com/express/) will work). Just have the IDE build the contents of the `main` directory.


## Most Other People

You will need a recent-ish version of [Mono](http://mono-project.com) installed. Then, just do:

	make

to build the assembly. If building is successful, the assembly will be found in `bin/`.

To build and run tests, use:

	make test
