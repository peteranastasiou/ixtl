# ixtl
c-sharp scripting language with interop

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

# General Flow

lexerIInputStream
 -> lexer/Lexer
  -> compiler/Compiler
   -> values/Function
    -> vm/Vm
