%namespace CompilerO
%scannertype GplexScanner
%visibility public

%%

"class"     { return (int)Tokens.CLASS; }
"if"        { return (int)Tokens.IF; }
"else"      { return (int)Tokens.ELSE; }
"while"     { return (int)Tokens.WHILE; }
"return"    { return (int)Tokens.RETURN; }
"void"      { return (int)Tokens.VOID; }
"int"       { return (int)Tokens.INT; }
"float"     { return (int)Tokens.FLOAT; }
"read"      { return (int)Tokens.READ; }
"write"     { return (int)Tokens.WRITE; }
"true"      { return (int)Tokens.BOOL_LITERAL; }
"false"     { return (int)Tokens.BOOL_LITERAL; }
"null"      { return (int)Tokens.NULL_LITERAL; }

"=="        { return (int)Tokens.EQUALS; }
"!="        { return (int)Tokens.NOT_EQUALS; }
">"         { return (int)Tokens.GREATER_THAN; }
"<"         { return (int)Tokens.LESS_THAN; }
"="         { return (int)Tokens.ASSIGN; }
"+"         { return (int)Tokens.PLUS; }
"-"         { return (int)Tokens.MINUS; }
"*"         { return (int)Tokens.MULTIPLY; }
"/"         { return (int)Tokens.DIVIDE; }

"("         { return (int)Tokens.LEFT_PAREN; }
")"         { return (int)Tokens.RIGHT_PAREN; }
"{"         { return (int)Tokens.LEFT_BRACE; }
"}"         { return (int)Tokens.RIGHT_BRACE; }
";"         { return (int)Tokens.SEMICOLON; }
","         { return (int)Tokens.COMMA; }

[0-9]+"."[0-9]+    { yylval.String = yytext; return (int)Tokens.FLOAT_LITERAL; }
[0-9]+             { yylval.String = yytext; return (int)Tokens.INT_LITERAL; }
\"[^"]*\"          { yylval.String = yytext.Substring(1, yytext.Length - 2); return (int)Tokens.STRING_LITERAL; }

[a-zA-Z_][a-zA-Z0-9_]* { yylval.String = yytext; return (int)Tokens.IDENTIFIER; }

[ \t\r\n]   { /* skip whitespace */ }
"//"[^\n]*  { /* skip single-line comments */ }

.           { Console.WriteLine($"Invalid character: {yytext}"); }