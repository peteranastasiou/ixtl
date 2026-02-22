
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

  public string GetLine(int lineNum) {
    // find start of line:
    int i = 0;
    while(lineNum > 0) {
      if(_str[i] == '\n') {
        lineNum --;
      }
      if(i == _str.Length) {
        return "<line number out of range>";
      }
      i ++;
    }
    // find end of line:
    int end = _str.IndexOf('\n', i);
    if (end < 0) {
      return _str[i..];
    } else {
      return _str[i..end];
    }
  }

  public void Close() {
  }
}
