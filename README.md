Iambic
===
*"Easy to parse up." :)*


# Welcome!

Iambic is a parser library for the CLR that is small, fast, and easy to embed while still having sophisticated features like automatic error recovery. Use Iambic for adding expression support to your apps, building compilers for your own programming languages, code editors with syntax highlighting and error markup, or simply for parsing structured data formats.

Iambic is released under a Simplified BSD License.

For more information, please see the [wiki](http://wiki.github.com/naucera/iambic/).

# Binaries

For the latest precompiled binaries, see [Downloads](http://github.com/naucera/iambic/archives/master).

# Building From Sources


## Windows

The simplest way to build on Windows is probably to use Visual Studio (the free [Express edition](http://www.microsoft.com/express/) will work). Just have the IDE build the contents of the `main` directory.


## Most Other People

You will need a recent-ish version of [Mono](http://mono-project.com) installed. Then, just do:

	make

to build the assembly. If building is successful, the assembly will be found in `bin/`.

To build and run tests, use:

	make test

To generate the API documentation, use:

    make doc

which will result in HTML browseable documentation in `bin/doc/html`.
