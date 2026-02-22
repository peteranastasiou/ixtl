namespace Ixtl.Tests;

using Xunit;

public class ValueTests {
  [Fact]
  public void ValuesEquality() {
    Value a = new Value.Str("hello");
    Value b = new Value.I32(3);
    Value c = new Value.Str("bye");
    Value d = new Value.Str("hell" + "o");
    Assert.NotEqual(a.GetType(), b.GetType());
    Assert.Equal(a.GetType(), c.GetType());
    Assert.Equal(d, a);
  }

  [Fact]
  public void ValueSwitching() {
    AssertValueType(new Value.Bool(true), ValueType.BOOL);
    AssertValueType(new Value.I32(1), ValueType.I32);
    AssertValueType(new Value.U32(1), ValueType.U32);
    AssertValueType(new Value.Flt(1.0), ValueType.FLT);
    AssertValueType(new Value.Str(""), ValueType.STR);
  }

  static void AssertValueType(Value v, ValueType t) {
    Assert.Equal(v.Type, t);
  }
}
