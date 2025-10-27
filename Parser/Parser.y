%namespace CompilerO
%parsertype Parser
%visibility public

%union {
    string String;
    int Integer;
}

%token <String> IDENTIFIER INT_LITERAL FLOAT_LITERAL STRING_LITERAL BOOL_LITERAL NULL_LITERAL
%token CLASS IF ELSE WHILE RETURN VOID INT FLOAT READ WRITE
%token ASSIGN EQUALS NOT_EQUALS GREATER_THAN LESS_THAN
%token PLUS MINUS MULTIPLY DIVIDE
%token LEFT_PAREN RIGHT_PAREN LEFT_BRACE RIGHT_BRACE SEMICOLON COMMA

%%

Program: ClassDeclaration
        ;

ClassDeclaration: CLASS IDENTIFIER LEFT_BRACE ClassMembers RIGHT_BRACE
                {
                    System.Console.WriteLine("Class declared: " + $2);
                }
                ;

ClassMembers: /* empty */
            | ClassMembers MemberDeclaration
            ;

MemberDeclaration: VariableDeclaration
                 | MethodDeclaration
                 ;

VariableDeclaration: Type IDENTIFIER SEMICOLON
                   {
                       System.Console.WriteLine("Variable: " + $2 + " of type " + $1);
                   }
                   ;

MethodDeclaration: Type IDENTIFIER LEFT_PAREN Parameters RIGHT_PAREN LEFT_BRACE Statements RIGHT_BRACE
                 {
                     System.Console.WriteLine("Method: " + $2 + " returns " + $1);
                 }
                 ;

Parameters: /* empty */
          | ParameterList
          ;

ParameterList: Parameter
             | ParameterList COMMA Parameter
             ;

Parameter: Type IDENTIFIER
         ;

Type: INT     { $$ = "int"; }
    | FLOAT   { $$ = "float"; }
    | VOID    { $$ = "void"; }
    ;

Statements: /* empty */
          | Statements Statement
          ;

Statement: VariableDeclaration
         | AssignmentStatement
         | IfStatement
         | WhileStatement
         | ReturnStatement
         | WriteStatement
         ;

AssignmentStatement: IDENTIFIER ASSIGN Expression SEMICOLON
                   {
                       System.Console.WriteLine("Assignment: " + $1 + " = " + $3);
                   }
                   ;

IfStatement: IF LEFT_PAREN Expression RIGHT_PAREN LEFT_BRACE Statements RIGHT_BRACE
           | IF LEFT_PAREN Expression RIGHT_PAREN LEFT_BRACE Statements RIGHT_BRACE ELSE LEFT_BRACE Statements RIGHT_BRACE
           ;

WhileStatement: WHILE LEFT_PAREN Expression RIGHT_PAREN LEFT_BRACE Statements RIGHT_BRACE
               ;

ReturnStatement: RETURN Expression SEMICOLON
               {
                   System.Console.WriteLine("Return: " + $2);
               }
               ;

WriteStatement: WRITE LEFT_PAREN Expression RIGHT_PAREN SEMICOLON
               {
                   System.Console.WriteLine("Write: " + $3);
               }
               ;

Expression: IDENTIFIER          { $$ = $1; }
          | INT_LITERAL         { $$ = $1; }
          | FLOAT_LITERAL       { $$ = $1; }
          | STRING_LITERAL      { $$ = "\"" + $1 + "\""; }
          | BOOL_LITERAL        { $$ = $1; }
          | Expression PLUS Expression   { $$ = $1 + " + " + $3; }
          | Expression MINUS Expression  { $$ = $1 + " - " + $3; }
          | Expression MULTIPLY Expression { $$ = $1 + " * " + $3; }
          | Expression DIVIDE Expression { $$ = $1 + " / " + $3; }
          | Expression EQUALS Expression { $$ = $1 + " == " + $3; }
          | Expression NOT_EQUALS Expression { $$ = $1 + " != " + $3; }
          | Expression GREATER_THAN Expression { $$ = $1 + " > " + $3; }
          | Expression LESS_THAN Expression { $$ = $1 + " < " + $3; }
          | LEFT_PAREN Expression RIGHT_PAREN { $$ = "(" + $2 + ")"; }
          ;

%%