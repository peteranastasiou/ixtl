# IXTL
Embeddable scripting language implemented in C#.

The language is intentionally simple and C-like.

Yes:
* Type Safety
* Curly Braces and Semi-Colons
* Systems programming inspired e.g. integer types and binary operators
* Garbage Collection (relying on C# GC)
* Inter-op with C# object methods (soon)
* Event subscription (soon)

No:
* Closures, Anonymous Functions, Lambdas
* Inheritance
* Pointers, References
* Shadowing Variables
* Type coersion - must cast

# How to use

## Setup

Install dotnet-sdk e.g.

`sudo apt install -y dotnet-sdk-8.0`

## Build

`dotnet build`

## Run

`dotnet run --project Ixtl`

## Test

`dotnet test`

# Example Script

TODO

# Notable differences from C

if, for, while are mostly the same. Except for first argument of for, these can only support expressions not variable assignment in the predicate.

Switch statements (TODO) have braces around the cases not the switch, and are available for any datatype:
```
switch(x)
case 0 {

} case 1 {

}
```

# EBNF
Note:
* This applies to tokens, after comments, whitespace and newlines are discarded.
* Type safety not encoded in below grammar
* Incomplete and possibly incorrect
```
Program ::= { TopLevelStatement }
TopLevelStatement ::= VarDef | FnDef
VarDef ::= Type IDENT '=' Expr ';'
FnDef ::= RetType IDENT '(' [Params] ')' Block
Params ::= Type IDENT { ',' Type IDENT }
Type ::= 'bool' | 'i8' | 'i16' | 'i32' | 'u8' | 'u16' | 'u32' | 'flt' | 'str' | 'fn'
RetType ::= Type | 'void'
Block ::= '{' {Statement} '}'
Statement ::= Call ';' | VarDef | Assignment | ReturnExpr | For | If | While | Switch
Assignment ::= IDENT  '=' Expr ';'
ReturnExpr ::= 'return' Expr ';'
Expr ::= Value | BinExpr | UnExpr | Group | Call
Group ::= '(' Expr ')'
Call ::= IDENT '(' [Args] ')'
Args ::= Expr { ',' Expr }
UnExpr ::= UnOp Expr
UnOp ::= '!' | '-' | '+' | Cast
Cast ::= '(' Type ')'
BinExpr ::= Expr BinOp Expr
BinOp ::= '+' | '-' | '&&' | '||' | '&' | '|' | '^' | '*' | '**' | '/' | '%'
For ::= 'for' '(' Type IDENT '=' Expr ';' Expr ';' Expr ')' Block
If ::= 'if' '(' Expr ')' Block { 'else' ( If | Block ) }
While ::= 'while' '(' Expr ')' Block
Switch ::= 'switch' '(' Expr ')' {Case}
Case ::= 'case' Expr Block 
Value ::= IDENT | INTEGER | FLOAT | STRING
```

IDENT is given by regex`[a-zA-Z]+[a-zA-Z0-9_]*`

INTEGER is given by regex `[0-9]+`

FLOAT is given by regex `[0-9]+\.[0-9]+`

STRING is content surrounded by '"'

# General Interpretter Flow

lexerIInputStream

-> lexer/Lexer

-> compiler/Compiler

-> compiler/Program (Note: can be saved to file)

-> vm/Vm

# Milestones
 [x] parse globals and main function
 [x] int, flt, strings
 [x] print
 [ ] math operations and casting
 [ ] call functions
 [ ] booleans & inequalities
 [ ] other integer types
 [ ] floats
 [ ] c-sharp objects
 [ ] if / for / while
 [ ] arrays
 [ ] switch
 [ ] panic