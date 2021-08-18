using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

namespace FoxThorne
{
    public class ConsoleReader : MonoBehaviour
    {
        [Header("Settings")]
        public bool displayVerbose = false;

        public bool displayLog = true;
        public bool displayWarning = true;
        public bool displayError = true;

        [Header("References")]
        public Text consoleDisplay;



        private void Update()
        {
            string displayString = "";
            foreach (string s in GameConsole.lines)
            {
                string nextLine = s;
                // We must check tags in reverse order that they're added.
                // First we check for warnings, then errors, then verbose
                // warnings:
                if (nextLine.EndsWith(GameConsole.WarningTag))
                {
                    if (displayWarning) // if we need to display it, trim the line
                        nextLine = nextLine.Substring(0, nextLine.Length - GameConsole.WarningTag.Length);
                    else // otherwise continue to the next line
                        continue;
                }
                // errors (a line can never be both a warning and an error, so we can use an else if here):
                else if (nextLine.EndsWith(GameConsole.ErrorTag))
                {
                    if (displayError) // if we need to display it, trim the line
                        nextLine = nextLine.Substring(0, nextLine.Length - GameConsole.ErrorTag.Length);
                    else // otherwise continue to the next line
                        continue;
                }
                // finally, verbose:
                if (nextLine.EndsWith(GameConsole.VerboseTag))
                {
                    if (displayVerbose) // if we need to display it
                        nextLine = nextLine.Substring(0, nextLine.Length - GameConsole.VerboseTag.Length);
                    else // otherwise, next line
                        continue;
                }

                displayString += nextLine + "\n";
            }

            consoleDisplay.text = displayString;
        }
    }
}