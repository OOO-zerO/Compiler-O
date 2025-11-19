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
        
        Console.WriteLine($"Generated MSIL saved to: {ilFilePath}");
        
        // Try to compile using ilasm
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
                    Console.WriteLine("✅ MSIL successfully compiled to executable");
                    Console.WriteLine($"📁 Output: {outputPath}");
                }
                else
                {
                    Console.WriteLine("❌ MSIL compilation failed");
                    Console.WriteLine($"Error: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Could not run ilasm: {ex.Message}");
            Console.WriteLine("💡 Make sure .NET Framework SDK is installed");
        }
    }
    
    public static void ShowMSILCode(string msilCode)
    {
        Console.WriteLine("Generated MSIL code:");
        Console.WriteLine(new string('=', 50));
        Console.WriteLine(msilCode);
        Console.WriteLine(new string('=', 50));
    }
}