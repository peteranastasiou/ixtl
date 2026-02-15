
namespace Ixtl;
public class StringInputStream : IInputStream {
  readonly string str;
  int idx;

  public StringInputStream(string str) {
    this.str = str;
    idx = 0;
  }

  public char Peek() {
    if (idx >= str.Length) {
      return '\0';
    }
    else {
      return this.str[idx];
    }
  }

  public char Next() {
    char c = Peek();
    idx++;
    return c;
  }

  public void Close() {
  }
}
