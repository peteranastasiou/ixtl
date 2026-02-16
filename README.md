# IXTL
Embeddable scripting language implemented in C#.

The language is intentionally simple and C-like.

Yes:
* Type Safety
* Curly Braces and Semi-Colons
* Systems programming inspired e.g. integer types and binary operators
* Inter-op with C# object methods
* Garbage Collection (relying on C# GC)

No:
* Closures / Anonymous Functions / Lambdas
* Inheritance

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

# EBNF
Note:
* This applies to tokens, after comments, whitespace and newlines are discarded.
* Type safety not encoded in below grammar
* Possibly incorrect
```
Program ::= { TopLevelStatement }
TopLevelStatement ::= VarDef | FnDef
VarDef ::= Type IDENT ‘=’ Expr ‘;’
FnDef ::= Type IDENT ‘(’ [Params] ‘)’ ’{’ {Statement} ‘}’
Params ::= Type IDENT { ‘,’ Type IDENT }
Type ::= ‘bool’ | ‘i8’ | ‘i16’ | ‘i32’ | ‘u8’ | ‘u16’ | ‘u32’ | ‘flt’ | ‘str’ | ‘fn’
Statement ::= Expr ‘;’ | VarDef | Assignment | ReturnExpr
Assignment ::= IDENT  ‘=’ Expr ‘;’
ReturnExpr ::= ‘return’ Expr ‘;’
Expr ::= Value | BinExpr | UnExpr | Group | Call
Group ::= ‘(’ Expr ‘)’
Call ::= IDENT ‘(’ [Args] ‘)’
Args ::= Expr { ‘,’ Expr }
UnExpr ::= UnOp Expr
UnOp ::= ‘!’ | ‘-’ | ‘+’ | Cast
Cast ::= ‘(’ Type ‘)’
BinExpr ::= Expr BinOp Expr
BinOp ::= ‘+’ | ‘-’ | ‘&&’ | ‘||’ | ‘&’ | ‘|’ | ‘^’ | ‘*’ | ‘**’ | ‘/’ | ‘%’
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
