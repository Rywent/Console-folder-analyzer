using ConsoleFolderAnalyzer;
using System;
using System.Diagnostics;

/// <summary>
/// The main entry point of the ConsoleFolderAnalyzer application.
/// Displays a menu, processes user input, and delegates control to settings and data collection.
/// </summary>
class Program
{
    const int frameInnerWidth = 28;
    static readonly string[] menuItems = new string[]
    {
        "0 - git hub",
        "1 - settings",
        "2 - start scanning",
        "3 - exit"
    };

    /// <summary>
    /// Displays the main menu with available options.
    /// </summary>
    static void Main()
    {
        SettingsManager _settings = new SettingsManager();
        DataCollection _startDataCollection = new DataCollection(_settings);
        PrintInformation _printInfo = new PrintInformation(_settings);
        while (true)
        {
            ShowMenu();

            int option = GetMenuOption();

            if (!HandleOption(option, _settings, _startDataCollection))
                break; // exit from the program
        }
    }

    /// <summary>
    /// Displays the main menu with available options.
    /// </summary>
    public static void ShowMenu()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Blue;

        string[] CFA = new string[]
        {
            "\n",
            "  ░██████  ░██████████   ░███    ",
            " ░██   ░██ ░██          ░██░██   ",
            "░██        ░██         ░██  ░██  ",
            "░██        ░█████████ ░█████████ ",
            "░██        ░██        ░██    ░██ ",
            " ░██   ░██ ░██        ░██    ░██ ",
            "  ░██████  ░██        ░██    ░██ ",
            "\n"
        };
        foreach (string line in CFA)
            Console.WriteLine(line);
        Console.ResetColor();

        string header = "MENU";
        int padding = (frameInnerWidth - (header.Length + 4)) / 2;
        string top = "+" + new string('═', padding) + "[ " + header + " ]" +
                     new string('═', frameInnerWidth - (padding + header.Length + 4)) + "+";
        Console.WriteLine(top);

        foreach (string item in menuItems)
            Console.WriteLine("║ " + item.PadRight(frameInnerWidth - 1) + "║");

        Console.WriteLine("+" + new string('═', frameInnerWidth) + "+");
    }

    /// <summary>
    /// Reads and validates the user's menu selection.
    /// </summary>
    /// <returns>The integer index of the selected menu option.</returns>
    static int GetMenuOption()
    {
        Console.WriteLine("Select an option:");
        while (true)
        {
            string input = Console.ReadLine();
            if (int.TryParse(input, out int option) && option >= 0 && option < menuItems.Length)
                return option;
            Console.WriteLine("Invalid option. Please try again:");
        }
    }

    /// <summary>
    /// Handles the selected menu option by calling the appropriate methods.
    /// </summary>
    /// <param name="option">The selected menu option index.</param>
    /// <param name="settings">The SettingsManager instance.</param>
    /// <param name="startDataCollection">The DataCollection instance.</param>
    /// <returns>False if the program should exit; otherwise, true.</returns>
    static bool HandleOption(int option, SettingsManager settings, DataCollection startDataCollection)
    {
        switch (option)
        {
            case 0:
                OpenGitHub();
                return true;
            case 1:
                settings.ShowSettings();
                return true;
            case 2:
                startDataCollection.InputPaths();
                return true; 
            case 3:
                Console.WriteLine("Exiting.");
                return false;
            default:
                return true;
        }
    }

    /// <summary>
    /// Opens the project's GitHub repository in the default web browser.
    /// </summary>
    static void OpenGitHub()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "https://github.com/Rywent/ConsoleFolderAnalyzer",
            UseShellExecute = true
        };
        Process.Start(psi);
    }
}
