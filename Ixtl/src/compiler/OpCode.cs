
namespace Ixtl;

public enum OpCode : byte {
  // Literals:
  LITERAL,        // Push a literal value from the chunk
  TRUE,           // Push true to the stack
  FALSE,          // Push false to the stack
  TYPE_FLOAT,     // TypeId of float
  TYPE_FUNCTION,  // TypeId of Function
  TYPE_STRING,    // TypeId of String
                  // Stack and variable manipulation
  POP,            // Pop 1 value from the stack
  DEFINE_GLOBAL_VAR,   // Define a global variable
  DEFINE_GLOBAL_CONST, // Define a global variable as const
  GET_GLOBAL,     // Push the value of a global to the stack
  SET_GLOBAL,     // Set the value of a variable
  GET_LOCAL,
  SET_LOCAL,
  // Binary operators: take two values from the stack and push one:
  EQUAL,
  NOT_EQUAL,
  GREATER,
  GREATER_EQUAL,
  LESS,
  LESS_EQUAL,
  ADD,
  SUBTRACT,
  MULTIPLY,
  DIVIDE,
  // Unary operators: take one value, push one value:
  NEGATE,
  NOT,
  // Built-ins:
  PRINT,              // Pop 1 value, print it, Push nil
                      // Control flow:
  JUMP,               // Unconditionally jump forward by bytecode offset 
  LOOP,               // Unconditionally jump backwards by bytecode offset 
  JUMP_IF_TRUE,       // If top of stack is truthy, jump fwd by bytecode offset
  JUMP_IF_FALSE,      // If top of stack is falsy, jump fwd by bytecode offset
  JUMP_IF_ZERO,       // If top of stack is zero, jump fwd by bytecode offset
  CALL,               // call function
  RETURN,
}
