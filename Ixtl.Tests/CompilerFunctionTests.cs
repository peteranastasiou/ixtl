namespace Ixtl.Tests;

using Xunit;

public class CompilerFunctionTests {
  [Fact]
  public void TestLocals() {
    /*
      fn {
        a
        {
          b
          {
            c
          }
          d
          {
            c
            e
          }
        }
        e
      }
    */
    var f = new Function("f");
    f.BeginScope();
    Assert.Equal(1, f.AddLocal("a"));
    f.BeginScope();
    Assert.Equal(2, f.AddLocal("b"));
    f.BeginScope();
    Assert.Equal(3, f.AddLocal("c"));

    Assert.Equal(1, f.ResolveLocalToStackPosition("a"));
    Assert.Equal(2, f.ResolveLocalToStackPosition("b"));
    Assert.Equal(3, f.ResolveLocalToStackPosition("c"));

    Assert.Equal(1, f.EndScope());

    Assert.Equal(-1, f.ResolveLocalToStackPosition("c"));

    Assert.Equal(3, f.AddLocal("d"));
    f.BeginScope();
    Assert.Equal(4, f.AddLocal("c"));
    Assert.Equal(5, f.AddLocal("e"));

    Assert.Equal(1, f.ResolveLocalToStackPosition("a"));
    Assert.Equal(2, f.ResolveLocalToStackPosition("b"));
    Assert.Equal(4, f.ResolveLocalToStackPosition("c"));
    Assert.Equal(3, f.ResolveLocalToStackPosition("d"));
    Assert.Equal(5, f.ResolveLocalToStackPosition("e"));

    Assert.Equal(2, f.EndScope());

    Assert.Equal(-1, f.ResolveLocalToStackPosition("c"));
    Assert.Equal(-1, f.ResolveLocalToStackPosition("e"));

    Assert.Equal(2, f.EndScope());
    Assert.Equal(2, f.AddLocal("e"));
    Assert.Equal(2, f.EndScope());

    Assert.Equal(-1, f.ResolveLocalToStackPosition("a"));
    Assert.Equal(-1, f.ResolveLocalToStackPosition("b"));
    Assert.Equal(-1, f.ResolveLocalToStackPosition("c"));
    Assert.Equal(-1, f.ResolveLocalToStackPosition("d"));
    Assert.Equal(-1, f.ResolveLocalToStackPosition("e"));
  }
}