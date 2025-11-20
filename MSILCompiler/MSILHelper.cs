using System;
using System.Diagnostics;
using System.IO;

public static class MSILHelper
{
    public static void CompileToExe(string msilCode, string outputPath)
    {
        // Save MSIL code to .il file
        string ilFilePath = Path.ChangeExtension(outputPath, ".il");
        File.WriteAllText(ilFilePath, msilCode);
        
        ColorConsole.WriteLine($"Generated MSIL saved to: {ilFilePath}");
        
        // Compile using ilasm
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "ilasm",
                Arguments = $"/exe /output=\"{outputPath}\" \"{ilFilePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
                
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                
                if (process.ExitCode == 0)
                {
                    ColorConsole.WriteSuccess("MSIL successfully compiled to executable");
                    ColorConsole.WriteLine($"Output: {outputPath}");
                }
                else
                {
                    ColorConsole.WriteError("MSIL compilation failed");
                    ColorConsole.WriteError($"Error: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            ColorConsole.WriteError($"Could not run ilasm: {ex.Message}");
            ColorConsole.WriteLine("Make sure .NET Framework SDK is installed");
        }
    }
    
    public static void ShowMSILCode(string msilCode)
    {
        ColorConsole.WriteLine("Generated MSIL code:");
        ColorConsole.WriteLine(new string('=', 50));
        ColorConsole.WriteLine(msilCode);
        ColorConsole.WriteLine(new string('=', 50));
    }
}

public static class ColorConsole
{
    public static void WriteLine(string message)
    {
        Console.WriteLine(message);
    }
    
    public static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    public static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    public static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    public static void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    public static void WriteFile(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    public static void WriteStage(string message)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
