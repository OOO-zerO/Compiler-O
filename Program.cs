using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Compiler O - Interactive Mode");
        Console.WriteLine("=============================\n");

        if (args.Length > 0)
        {
            ProcessFile(args[0]);
        }
        else
        {
            RunInteractiveMode();
        }
    }

    static void RunInteractiveMode()
    {
        while (true)
        {
            Console.WriteLine("\nChoose action:");
            Console.WriteLine("1 - Process specific file");
            Console.WriteLine("2 - Run all tests");
            Console.WriteLine("3 - Compile to MSIL");
            Console.WriteLine("4 - Exit");
            Console.Write("Your choice: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ProcessSingleFile();
                    break;
                case "2":
                    RunAllTests();
                    break;
                case "3":
                    RunMSILCompilation();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    static void ProcessSingleFile()
    {
        Console.Write("\nEnter file path: ");
        string filePath = Console.ReadLine();

        if (string.IsNullOrEmpty(filePath))
        {
            Console.WriteLine("Path cannot be empty.");
            return;
        }

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        ProcessFile(filePath);
    }

    static void ProcessFile(string filePath)
    {
        try
        {
            Console.WriteLine($"\nProcessing file: {Path.GetFileName(filePath)}");
            Console.WriteLine(new string('=', 60));

            string content = File.ReadAllText(filePath);

            Console.WriteLine("Source code:");
            Console.WriteLine(content);
            Console.WriteLine(new string('-', 60));

            // Stage 1 - Lexical analysis
            Console.WriteLine("\nLEXICAL ANALYSIS:");
            Console.WriteLine(new string('-', 40));
            RunLexerStage(content);

            // Stage 2 - Syntax analysis
            Console.WriteLine("\nSYNTAX ANALYSIS (AST):");
            Console.WriteLine(new string('-', 40));
            var ast = RunParserStage(content);

            if (ast != null)
            {
                // Stage 3 - Semantic analysis
                Console.WriteLine("\nSEMANTIC ANALYSIS:");
                Console.WriteLine(new string('-', 40));
                RunSemanticStage(ast);

                // Stage 4 - MSIL Compilation (optional)
                Console.WriteLine("\nMSIL COMPILATION:");
                Console.WriteLine(new string('-', 40));
                CompileToMSIL(ast, Path.GetFileNameWithoutExtension(filePath));
            }

            Console.WriteLine(new string('=', 60));
            Console.WriteLine("Processing completed!");

        }
        catch (Exception e)
        {
            Console.WriteLine($"Error processing file: {e.Message}");
        }
    }

    static void CompileToMSIL(ProgramNode ast, string fileName)
    {
        try
        {
            var compiler = new MSILCompiler();
            string msilCode = compiler.Compile(ast);
            
            MSILHelper.ShowMSILCode(msilCode);
            
            // Save to Tests/Compilation folder
            string outputPath = Path.Combine("./Tests/Compilation", fileName + ".exe");
            MSILHelper.CompileToExe(msilCode, outputPath);
        }
        catch (Exception e)
        {
            Console.WriteLine($"MSIL compilation error: {e.Message}");
        }
    }

    static void RunLexerStage(string content)
    {
        try
        {
            var lexer = new Lexer(content);
            Token token;
            int tokenCount = 0;

            do
            {
                token = lexer.GetNextToken();
                Console.WriteLine($"  {token}");
                tokenCount++;

                if (token.Type == TokenType.ERROR)
                {
                    Console.WriteLine("  Error in token");
                    break;
                }

                if (tokenCount > 1000)
                {
                    Console.WriteLine("  Token limit reached");
                    break;
                }

            } while (token.Type != TokenType.EOF);

            Console.WriteLine($"\n  Total tokens: {tokenCount}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"  Lexer error: {e.Message}");
        }
    }

    static ProgramNode RunParserStage(string content)
    {
        try
        {
            var parser = new Parser(content);
            ProgramNode ast = parser.Parse();

            ASTPrinter.Print(ast);
            Console.WriteLine("  Syntax analysis successful");
            return ast;
        }
        catch (Exception e)
        {
            Console.WriteLine($"  Parser error: {e.Message}");
            return null;
        }
    }

    static void RunSemanticStage(ProgramNode ast)
    {
        try
        {
            var analyzer = new SemanticAnalyzer();
            var errors = analyzer.Analyze(ast);

            if (errors.Count == 0)
            {
                Console.WriteLine("  Semantic analysis successful");

                // Show optimized AST if changes were made
                Console.WriteLine("\n  Optimized AST:");
                ASTPrinter.Print(ast);
            }
            else
            {
                Console.WriteLine("  Semantic errors:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"    {error}");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"  Semantic analysis error: {e.Message}");
        }
    }

    static void RunMSILCompilation()
    {
        Console.WriteLine("\nMSIL COMPILATION");
        Console.WriteLine("================");
        
        string compilationPath = "./Tests/Compilation";
        if (!Directory.Exists(compilationPath))
        {
            Directory.CreateDirectory(compilationPath);
            Console.WriteLine($"Created directory: {compilationPath}");
            
            // Create sample test file
            CreateSampleCompilationTest();
            return;
        }

        foreach (string file in Directory.GetFiles(compilationPath, "*.txt"))
        {
            try
            {
                Console.WriteLine($"\nCompiling: {Path.GetFileName(file)}");
                string content = File.ReadAllText(file);
                
                // Parse
                var parser = new Parser(content);
                ProgramNode ast = parser.Parse();
                
                // Semantic analysis
                var analyzer = new SemanticAnalyzer();
                var errors = analyzer.Analyze(ast);
                
                if (errors.Count > 0)
                {
                    Console.WriteLine("Semantic errors found, skipping compilation:");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"  {error}");
                    }
                    continue;
                }
                
                // Generate MSIL
                var compiler = new MSILCompiler();
                string msilCode = compiler.Compile(ast);
                
                // Show generated code
                MSILHelper.ShowMSILCode(msilCode);
                
                // Compile to executable
                string exePath = Path.Combine(compilationPath, Path.GetFileNameWithoutExtension(file) + ".exe");
                MSILHelper.CompileToExe(msilCode, exePath);
                
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
    }

    static void CreateSampleCompilationTest()
    {
        string sampleCode = @"class Program is
    method main() is
        var x: 5
        var y: 3
        var result: x + y
        write(result)
    end
end";

        string filePath = Path.Combine("./Tests/Compilation", "sample_program.txt");
        File.WriteAllText(filePath, sampleCode);
        Console.WriteLine($"Created sample test: {filePath}");
    }

    static void RunAllTests()
    {
        Console.WriteLine("\nRUNNING ALL TESTS");
        Console.WriteLine(new string('=', 50));

        RunLexerTests();
        RunParserTests();
        RunSemanticTests();

        Console.WriteLine("\nAll tests completed!");
    }

    static void RunLexerTests()
    {
        Console.WriteLine("\nLEXER TESTS");
        string lexerPath = "./Tests/Lexer";
        if (!Directory.Exists(lexerPath))
        {
            Console.WriteLine($"Directory not found: {lexerPath}");
            return;
        }

        foreach (string file in Directory.GetFiles(lexerPath, "*.txt"))
        {
            try
            {
                Console.WriteLine($"\nProcessing: {Path.GetFileName(file)}");
                string content = File.ReadAllText(file);

                var lexer = new Lexer(content);
                Token token;
                int tokenCount = 0;

                do
                {
                    token = lexer.GetNextToken();
                    Console.WriteLine($"  {token}");
                    tokenCount++;

                    if (token.Type == TokenType.ERROR)
                    {
                        Console.WriteLine("  Error in token");
                        break;
                    }

                    if (tokenCount > 1000)
                    {
                        Console.WriteLine("  Token limit reached");
                        break;
                    }

                } while (token.Type != TokenType.EOF);

                Console.WriteLine($"  Total tokens: {tokenCount}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"  Error: {e.Message}");
            }
        }
    }

    static void RunParserTests()
    {
        Console.WriteLine("\nPARSER TESTS");
        string syntaxPath = "./Tests/Syntax";
        if (!Directory.Exists(syntaxPath))
        {
            Console.WriteLine($"Directory not found: {syntaxPath}");
            return;
        }

        foreach (string file in Directory.GetFiles(syntaxPath, "*.txt"))
        {
            try
            {
                Console.WriteLine($"\nParsing: {Path.GetFileName(file)}");
                string content = File.ReadAllText(file);
                var parser = new Parser(content);
                ProgramNode ast = parser.Parse();

                Console.WriteLine("AST:");
                ASTPrinter.Print(ast);
                Console.WriteLine("Success");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
    }

    static void RunSemanticTests()
    {
        Console.WriteLine("\nSEMANTIC ANALYSIS TESTS");
        string semanticPath = "./Tests/Semantic";
        if (!Directory.Exists(semanticPath))
        {
            Console.WriteLine($"Directory not found: {semanticPath}");
            return;
        }

        foreach (string file in Directory.GetFiles(semanticPath, "*.txt"))
        {
            try
            {
                Console.WriteLine($"\nAnalyzing: {Path.GetFileName(file)}");
                string content = File.ReadAllText(file);

                var parser = new Parser(content);
                ProgramNode ast = parser.Parse();

                var analyzer = new SemanticAnalyzer();
                var errors = analyzer.Analyze(ast);

                if (errors.Count == 0)
                {
                    Console.WriteLine("Semantic analysis successful");
                }
                else
                {
                    Console.WriteLine("Semantic errors:");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"  {error}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
    }
}