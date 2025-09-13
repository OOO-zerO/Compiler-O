using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Lexer tests");

        string LexerPath = "./Tests/Lexer";

        string[] files = Directory.GetFiles(LexerPath, "*.txt");
        foreach (string file in files)
        {
            try
            {
                string content = File.ReadAllText(file);
                Console.WriteLine("\nResult of tokenization:");
                Console.WriteLine(new string('-', 50));

                ILexer lexer = new Lexer(content);
                Token token;
                int tokenCount = 0;

                do
                {
                    token = lexer.GetNextToken();
                    Console.WriteLine(token);
                    tokenCount++;

                    if (token.Type == TokenType.ERROR)
                    {
                        Console.WriteLine("Error: error in analyse token");
                        break;
                    }

                    if (tokenCount > 1000)
                    {
                        Console.WriteLine("Warning: The limit of tokens reached");
                        break;
                    }

                } while (token.Type != TokenType.EOF);

                Console.WriteLine(new string('-', 50));
                Console.WriteLine($"Total tokens count: {tokenCount}");
            } catch (Exception e)
            {
                Console.WriteLine("Unable to run code " + e.Message);
            }
        }
    }
}