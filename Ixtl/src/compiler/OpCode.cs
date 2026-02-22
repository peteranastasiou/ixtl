
namespace Ixtl;

public enum OpCode : byte {

  // ------------- Literals -------------

  /**
   * Push Literal to stack
   * Operand: index of literal
   * Pushes: value of literal
   */
  LITERAL,

  /**
   * Push true to the stack
   * Pushes: true
   */
  TRUE,

  /**
   * Push false to the stack
   * Pushes: false
   */
  FALSE,

  // ----- Stack and variables ----------

  /**
   * Pop 1 value from the stack
   * Pops: 1 value
   */
  POP,

  /**
   * Add a new global variable from stack value
   * Operand: index of string literal for name of global
   * Operand: ValueType of the global
   * Pops: 1 value from stack
   */
  DEFINE_GLOBAL_VAR,

  /**
   * Look up a global by index
   * Operand: index of global
   * Pushes: 1 value (the global value)
   */
  GET_GLOBAL,

  /**
   * Look up a global by index
   * Operand: index of global
   * Pops: none (top of stack is peeked)
   */
  SET_GLOBAL,

  // GET_LOCAL,
  // SET_LOCAL,

  // ------ Operations ------------------

  /**
   * Binary Operators
   * Pops: 2 values (operands)
   * Pushes: 1 value (the result)
   */
  ADD,
  SUBTRACT,
  MULTIPLY,
  DIVIDE,
  // EQUAL,
  // NOT_EQUAL,
  // GREATER,
  // GREATER_EQUAL,
  // LESS,
  // LESS_EQUAL,

  /**
   * Unary Operators
   * Pops: 1 value (operand)
   * Pushes: 1 value (the result)
   */
  // NEGATE,
  // NOT,

  // ------ Built-ins ------------------
  PRINT,              // Pop 1 value, print it, Push nil

  // ------ Flow Control ---------------
  // JUMP,               // Unconditionally jump forward by bytecode offset 
  // LOOP,               // Unconditionally jump backwards by bytecode offset 
  // JUMP_IF_TRUE,       // If top of stack is truthy, jump fwd by bytecode offset
  // JUMP_IF_FALSE,      // If top of stack is falsy, jump fwd by bytecode offset
  // JUMP_IF_ZERO,       // If top of stack is zero, jump fwd by bytecode offset
  // CALL,               // call function
  RETURN,
}
