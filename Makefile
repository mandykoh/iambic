csc = gmcs
nunit = nunit-console2

bindir = bin

sources = $(shell find main/cs -name *.cs)
tests = $(shell find test/cs -name *.cs)

assembly: init-bindir $(bindir)/Naucera.Iambic.dll

clean:
	rm -rf bin

clean-test:
	rm -rf bin/*.Test.dll bin/*.TestResult.xml

init-bindir:
	mkdir -p $(bindir)


$(bindir)/Naucera.Iambic.dll: $(sources)
	$(csc) -doc:$(bindir)/Naucera.Iambic.doc.xml -nowarn:1591 -debug -recurse:main/cs/*.cs -lib:$(bindir) -target:library -out:$(bindir)/Naucera.Iambic.dll

$(bindir)/Naucera.Iambic.Test.dll: $(tests) $(bindir)/Naucera.Iambic.dll
	$(csc) -debug -recurse:test/cs/*.cs -lib:$(bindir) -r:nunit.framework.dll -r:Naucera.Iambic.dll -target:library -out:$(bindir)/Naucera.Iambic.Test.dll

test: init-bindir $(bindir)/Naucera.Iambic.Test.dll
	$(nunit) $(bindir)/Naucera.Iambic.Test.dll -xml=$(bindir)/Naucera.Iambic.TestResult.xml

doc: assembly
	monodocer -pretty -importslashdoc:$(bindir)/Naucera.Iambic.doc.xml -assembly:$(bindir)/Naucera.Iambic.dll -path:$(bindir)/doc/monodoc
	mdoc export-html -o $(bindir)/doc/html $(bindir)/doc/monodoc
