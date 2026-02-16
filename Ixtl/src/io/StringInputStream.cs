
namespace Ixtl;
public class StringInputStream : IInputStream {
  readonly string _str;
  int _idx;

  public StringInputStream(string str) {
    _str = str;
    _idx = 0;
  }

  public char Peek() {
    if (_idx >= _str.Length) {
      return '\0';
    }
    else {
      return _str[_idx];
    }
  }

  public char Next() {
    char c = Peek();
    _idx++;
    return c;
  }

  public void Close() {
  }
}
