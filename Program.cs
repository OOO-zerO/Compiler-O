using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Input Code for lexer, or press enter for default code");

        string inputCode = "";
        string line;
        while ((line = Console.ReadLine()) != null && line != "")
        {
            inputCode += line + "\n";
        }

        if (string.IsNullOrEmpty(inputCode))
        {
            inputCode = @"class Example is
    method main is
        var x : Integer(42)
        var y : Real(3.14)
        
        if x.Greater(Integer(10)) then
            // In Project O, we need to use method calls instead of write()
            // Assuming we have some output mechanism, but it's not specified
            // For now, we'll just show the structure
        else
            // else branch
        end
        
        var flag : Boolean(true)
    end
end

class Hi extends Example is
    method double(x: Integer) : Integer is
        return x.Plus(x)
    end
end
";
            Console.WriteLine("\nUsing Default example of code:");
            Console.WriteLine(inputCode);
        }

        Console.WriteLine("\nResult of tokenization:");
        Console.WriteLine(new string('-', 50));

        ILexer lexer = new Lexer(inputCode);
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
    }
}