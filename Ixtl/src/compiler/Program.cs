namespace Ixtl;

public class Program {
  // List of literals, looked up by index at run time
  public List<Value> Literals = [];

  // Code to initialise top level variables, runs first
  public Chunk Init = new();

  // Map of functions
  public Dictionary<string, Function> Functions = [];
}
