namespace Ixtl;

/**
 * This is the compiler's concept of a function, at run time all we need is the chunk
 */
public class Function {
  public string Name;
  public Chunk Chunk = new();

  // Local variable tracking
  readonly Stack<Local> _locals = [];
  int _scopeDepth = 0;

  public Function(string name) {
    Name = name;

    // Reserve a space for the stack pointer:
    AddLocal("");
  }

  public void WriteByteCode(byte b) {
    Chunk.Code.Add(b);
  }

  /**
   * If the local var exists in scope, return what its stack position will be
   * returns -1 if not a defined local
   */
  public int ResolveLocalToStackPosition(string name) {
    int index = _locals.Count - 1;
    foreach(var local in _locals) {
      if (local.Name == name) {
        return index;
      }
      -- index;
    }
    return -1;
  }

  /**
   * Tracks another local, returns index aka predicted stack position
   */
  public int AddLocal(string name) {
    int idx = _locals.Count;
    _locals.Push(new(name, _scopeDepth));
    return idx;
  }

  public void BeginScope() {
    _scopeDepth ++;
  }

  /**
   * End 1 level of scoping, popping all locals
   * Returns number of locals to pop
   */
  public int EndScope() {
    _scopeDepth --;

    // Pop all locals from that scope depth
    int numLocalsToPop = 0;
    while (_locals.Count > 0 && _locals.Peek().Depth > _scopeDepth) {
      _locals.Pop();
      ++ numLocalsToPop;
    }
    return numLocalsToPop;
  }

}

public class Local(string name, int depth) {
  public string Name = name;
  public int Depth = depth;
}

