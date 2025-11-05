public enum TokenType
{
    // Special Tokens
    EOF, ERROR,

    // Keywords
    CLASS, EXTENDS, IS, END, VAR, METHOD, THIS,
    IF, THEN, ELSE, WHILE, LOOP, RETURN,

    // Type Keywords (predefined classes)
    INTEGER, REAL, BOOLEAN, ARRAY, LIST, ANYVALUE, ANYREF,

    // Identifiers and Literals
    IDENTIFIER, INT_LITERAL, REAL_LITERAL, BOOL_LITERAL, STRING_LITERAL,

    // Operators and Punctuation
    ASSIGN, DOT, COLON, COMMA, SEMICOLON,
    LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
    LEFT_BRACKET, RIGHT_BRACKET,

    // Arithmetic operators
    PLUS, MINUS, STAR, SLASH,

    // Comparison operators
    GT, LT, GE, LE, EQEQ, NEQ,

    // Special symbols
    ARROW,
    EXTENDS_SYMBOL
}