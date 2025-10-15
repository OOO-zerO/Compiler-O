using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        RunLexerTests();
        RunParserTests();
    }

    static void RunLexerTests()
    {
        Console.WriteLine("Lexer tests");
        string lexerPath = "./Tests/Lexer";
        if (!Directory.Exists(lexerPath))
        {
            Console.WriteLine($"Error: Directory '{lexerPath}' does not exist.");
            return;
        }

        foreach (string file in Directory.GetFiles(lexerPath, "*.txt"))
        {
            try
            {
                Console.WriteLine($"\nProcessing file: {Path.GetFileName(file)}");
                string content = File.ReadAllText(file);
                Console.WriteLine("Result of tokenization:");
                Console.WriteLine(new string('-', 50));

                var lexer = new global::Lexer(content);
                global::Token token;
                int tokenCount = 0;
                do
                {
                    token = lexer.GetNextToken();
                    Console.WriteLine(token);
                    tokenCount++;
                    if (token.Type == global::TokenType.ERROR)
                    {
                        Console.WriteLine("Error: error in analyse token");
                        break;
                    }
                    if (tokenCount > 1000)
                    {
                        Console.WriteLine("Warning: The limit of tokens reached");
                        break;
                    }
                } while (token.Type != global::TokenType.EOF);

                Console.WriteLine(new string('-', 50));
                Console.WriteLine($"Total tokens count: {tokenCount}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to process file {file}: {e.Message}");
            }
        }
    }

    static void RunParserTests()
    {
        Console.WriteLine();
        Console.WriteLine("Parser tests");
        string syntaxPath = "./Tests/Syntax";
        if (!Directory.Exists(syntaxPath))
        {
            Console.WriteLine($"Error: Directory '{syntaxPath}' does not exist.");
            return;
        }

        foreach (string file in Directory.GetFiles(syntaxPath, "*.txt"))
        {
            try
            {
                Console.WriteLine($"\nParsing file: {Path.GetFileName(file)}");
                string content = File.ReadAllText(file);
                var parser = new Parser(content);
                ProgramNode ast = parser.Parse();
                Console.WriteLine("AST:");
                Console.WriteLine(new string('-', 50));
                ASTPrinter.Print(ast);
                Console.WriteLine(new string('-', 50));
                Console.WriteLine("Parse result: Success");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to parse file {file}: {e.Message}");
            }
        }
    }
}