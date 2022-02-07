using UnityEngine;

namespace TutoTOONS.Utils.Debug.Console
{
    public class DebugConsoleCommand
    {
        public delegate void CommandCallback(string _full_command);

        public string command;
        public string description;
        public CommandCallback commandCallback;

        public DebugConsoleCommand(string _command, string _description, CommandCallback _callback)
        {
            command = _command;
            description = _description;
            commandCallback = _callback;
        }
    }
}