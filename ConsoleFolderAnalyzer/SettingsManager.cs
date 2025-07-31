using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleFolderAnalyzer
{
    /// <summary>
    /// Manages application settings, including display options and highlighting parameters.
    /// </summary>
    class SettingsManager
    {
        // Boolean display settings
        public bool ShowSize { get; set; } = true; // show size
        public bool ShowDateChange { get; set; } = false; // show the date of last change
        public bool ShowCreationDate { get; set; } = false; // show the creation date
        public bool Highlight {  get; set; } = true; // backlight
        public bool ShortenAbsolutePath { get; set; } = false; // Shorten the absolute path

        // Highlight size thresholds (MB)
        public int minSizeLight { get; set; } = 100; // Minimum file/folder size and highlighting of its name
        public int mediumSizeLight { get; set; } = 1000; // Maximum file/folder size and highlighting of its name
        public int aboveAverageSizeLight { get; set; } = 4000; // Above avarage size
        public int maxSizeLight { get; set; } = 5000; // maximum size


        private Dictionary<string, Func<bool>> boolSettingsGetters;
        private Dictionary<string, Action<bool>> boolSettingsSetters;

        private Dictionary<string, Func<int>> lightSettingsGetters;     
        private Dictionary<string, Action<int>> lightSettingsSetters;

        
        public SettingsManager()
        {
            // initialization of the dictionary with delegates that will provide the current value of properties
            boolSettingsGetters = new Dictionary<string, Func<bool>>()
            {              
                {"(1) Show size", () => ShowSize},
                {"(2) Show date of last change", () => ShowDateChange},
                {"(3) Show creation date", () => ShowCreationDate},
                {"(4) Backlight", () => Highlight},
                {"(5) Shorten the absolute path", () => ShortenAbsolutePath}

            };
            boolSettingsSetters = new Dictionary<string, Action<bool>>()
            {
                {"(1) Show size", v => ShowSize = v },
                {"(2) Show date of last change",  v => ShowDateChange = v },
                {"(3) Show creation date",  v => ShowCreationDate = v },
                {"(4) Backlight", v => Highlight = v },
                {"(5) Shorten the absolute path", v => ShortenAbsolutePath = v}
            };

            // initialization of the array for configuring highlighting
            lightSettingsGetters = new Dictionary<string, Func<int>>()
            {
                {"(6) minimum size for highlighting", () => minSizeLight },
                {"(7) medium size for highlighting", () => mediumSizeLight},
                {"(8) above average size for highlighting", () => aboveAverageSizeLight },
                {"(9) maximum size for highlighting", () => maxSizeLight}
            };
            lightSettingsSetters = new Dictionary<string, Action<int>>()
            {
                {"(6) minimum size for highlighting", v => minSizeLight = v },
                {"(7) medium size for highlighting", v => mediumSizeLight = v },
                {"(8) above average size for highlighting", v => aboveAverageSizeLight = v },
                {"(9) maximum size for highlighting", v => maxSizeLight = v}
            };
        }

        /// <summary>
        /// Displays the settings menu.
        /// </summary>
        public void ShowSettings()
        {
            Console.Clear();
            DrawSettingsMenu();

            while (true)
            {
                int option = GetSettingsOption();
                if (option == 0)
                    return;

                bool changed = false;
                if (HandleOption(option))
                {
                    changed = true;
                }
                else
                {
                    return;
                }
                // if the settings have been changed, redraw the menu
                if (changed)
                {
                    Console.Clear();
                    DrawSettingsMenu();
                }
            }
        }

        /// <summary>
        /// Renders the settings menu and populates it with current settings.
        /// </summary>
        private void DrawSettingsMenu()
        {
            int frameInnerWidth = 100;

            string header = "Settings";
            int padding = (frameInnerWidth - (header.Length + 4)) / 2;
            string top = "+" + new string('═', padding) + "[ " + header + " ]" + new string('═', frameInnerWidth - (padding + header.Length + 4)) + "+";

            Console.WriteLine(top);

            foreach (var item in boolSettingsGetters)
            {
                string key = item.Key;
                bool value = item.Value();

                string valueString = value ? "true" : "false";

                string lineStart = "║ " + key + ": ";
                int spacesCount = frameInnerWidth - lineStart.Length - valueString.Length - 1;
                if (spacesCount < 0) spacesCount = 0;

                string lineEnd = new string(' ', spacesCount) + "  ║";

                Console.Write(lineStart);

                Console.ForegroundColor = value ? ConsoleColor.Green : ConsoleColor.Red;
                Console.Write(valueString);

                Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine(lineEnd);
            }
            foreach (var item in lightSettingsGetters)
            {
                string key = item.Key;
                string value = item.Value().ToString() + "MB";

                string lineStart = "║ " + key + ": ";
                int spacesCount = frameInnerWidth - lineStart.Length - value.Length - 1;
                if (spacesCount < 0) spacesCount = 0;

                string lineEnd = new string(' ', spacesCount) + "  ║";

                Console.Write(lineStart);

                if (item.Value() <= minSizeLight) { Console.ForegroundColor = ConsoleColor.Green; }
                else if (item.Value() <= mediumSizeLight) { Console.ForegroundColor = ConsoleColor.Yellow; }
                else if (item.Value() < aboveAverageSizeLight) { Console.ForegroundColor = ConsoleColor.Red; }
                else if (item.Value() >= maxSizeLight) { Console.ForegroundColor = ConsoleColor.DarkRed; }

                Console.Write(value);
                Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine(lineEnd);
            }
            Console.WriteLine("║ " + "write back to exit to the menu" + new string(' ', 69) + "║");
            Console.WriteLine("+" + new string('═', 100) + "+");
        }

        /// <summary>
        /// Processes user input for selecting a settings option.
        /// </summary>
        int GetSettingsOption()
        {
            Console.WriteLine("Select an option:");
            while (true)
            {
                string input = Console.ReadLine();
                int totalOption = boolSettingsGetters.Count + lightSettingsGetters.Count;
                if(input.ToLower() == "back")
                {
                    return 0;
                }
                if (int.TryParse(input, out int option) && option > 0 && option <= totalOption)
                    return option;
                Console.WriteLine("Invalid option. Please try again:");
            }
        }

        /// <summary>
        /// Validates the selected option and proceeds to handle the setting change if valid.
        /// </summary>
        bool HandleOption(int option)
        {
            int maxOption = boolSettingsGetters.Count + lightSettingsGetters.Count;
            if (option < 1 || option > maxOption)
                return false; 

            bool changed = SettingsParams(option);
            return changed;
        }

        /// <summary>
        /// Displays the current value of the selected setting, prompts for a new value, and updates the setting.
        /// </summary>
        bool SettingsParams(int numberParam)
        {
            int total = 0;
            foreach (var item in boolSettingsGetters.Keys)
            {
                total++;
                if(total == numberParam)
                {
                    Console.WriteLine($"Current value for {item}: {boolSettingsGetters[item]()}");
                    Console.Write("Enter new value (true/false): ");
                    string input = Console.ReadLine();
                    if(bool.TryParse(input, out bool newVal))
                    {
                        boolSettingsSetters[item](newVal);
                        Console.WriteLine("Update! Press any key");
                        Console.ReadKey();
                        return true; 
                    }
                    else
                    {
                        Console.WriteLine("Invalid input! Press any key");
                        Console.ReadKey();
                        return true;
                    }
                    
                }
                
            }
            total = 0;
            foreach(var item in lightSettingsGetters.Keys)
            {
                total++;
                if (total + boolSettingsGetters.Count == numberParam)
                {
                    Console.WriteLine($"Current value for {item}: {lightSettingsGetters[item]()}");
                    Console.Write("Enter a new value (numerical value) ");
                    string input = Console.ReadLine();
                    if(int.TryParse(input, out int newVal))
                    {
                        lightSettingsSetters[item](newVal);
                        Console.WriteLine("Updated! Press any key");
                        Console.ReadKey();
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input! Press any key");
                        Console.ReadKey();
                        return true;
                    }
                }
            }
            return false;

        }

    }
}
