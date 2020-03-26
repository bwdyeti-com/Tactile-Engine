using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FEXNA_Library
{
    public class TextCommand
    {
        public const char MARKER = (char)0x01;
        const char DELIMITER = '|';
        const string DELIMITER_REGEX = @"\|";
        public readonly static Dictionary<string, TextCommand> COMMANDS = new Dictionary<string, TextCommand>
        {
            // \F: Change a face sprite graphic
            { "F",      new TextCommand { Identifier = (char)0x02, Tag = @"\\[Ff]", RegexStr = @"[0-9]\|([\w\s']+\-\w+|[\w\s']+)", StartupCommand = true } },
            // \S: Change the active speaker
            //{ "S",      new TextCommand { Identifier = (char)0x04, Tag = @"\\[Ss]", RegexStr = @"[-]{0,1}\d+", StartupCommand = true, NewlineAfterFirst = true } }, //Debug
            { "S",      new TextCommand { Identifier = (char)0x04, Tag = @"\\[Ss]", RegexStr = @"[-]{0,1}\w+", StartupCommand = true, ExecutionNotCertain = true, NewlineAfterFirst = true } },
            // \E: Change a face sprite's expression
            { "E",      new TextCommand { Identifier = (char)0x06, Tag = @"\\[Ee]", RegexStr = @"[0-9]\|[0-9]+", StartupCommand = true } },
            // \B: Change a face sprite's blink mode
            { "B",      new TextCommand { Identifier = (char)0x0e, Tag = @"\\[Bb]", RegexStr = @"[0-9]\|[0-9]+", StartupCommand = true } },
            // \G: Change background
            { "G",      new TextCommand { Identifier = (char)0x0f, Tag = @"\\[Gg]", RegexStr = @"[\w\s.,\$!\(\)-/\\\?;:#&""'\+]+", StartupCommand = true } },
            // \R: Reverse face
            { "R",      new TextCommand { Identifier = (char)0x13, Tag = @"\\[Rr]", RegexStr = @"[0-9]+", StartupCommand = true, ExecutionNotCertain = true } },
            // \FC: Recolor generic face; this only allows using single words of letters as colors //Yeti
            { "FC",     new TextCommand { Identifier = (char)0x15, Tag = @"\\[Ff][Cc]", RegexStr = @"[0-9]\|\w+", StartupCommand = true } },
            // \music: Change music
            { "Music",  new TextCommand { Identifier = (char)0x16, Tag = @"\\[Mm][Uu][Ss][Ii][Cc]", RegexStr = @"[\w\s.,\$!\(\)-/\\\?;:#&""'\+]+", StartupCommand = true } },
            // \name: Change name
            { "Name",   new TextCommand { Identifier = (char)0x18, Tag = @"\\[Nn][Aa][Mm][Ee]", RegexStr = @"[-]{0,1}\d+\|([\w\s.,\$!\(\)-/\\\?;:#&""'\+]+)", StartupCommand = true } },
            // \O: Face order
            { "O",      new TextCommand { Identifier = (char)0x1f, Tag = @"\\[Oo]", RegexStr = @"[0-9]+", StartupCommand = true } },

            // \C: Change text color
            { "C",      new TextCommand { Identifier = (char)0x01, Tag = @"\\[Cc]", RegexStr = @"[\w\s:]*" } },
            // \M: Move a face sprite
            { "M",      new TextCommand { Identifier = (char)0x03, Tag = @"\\[Mm]", RegexStr = @"[0-9]\|[0-9]+", ExecutionNotCertain = true } },
            // \W: Quick wait
            { "W",      new TextCommand { Identifier = (char)0x05, Tag = @"\\[Ww]", RegexStr = @"[0-9]+" } },
            // \wait: Force wait
            { "Wait",   new TextCommand { Identifier = (char)0x14, Tag = @"\\[Ww][Aa][Ii][Tt]", RegexStr = @"[0-9]+" } },
            // \X: Pitch change
            { "X",      new TextCommand { Identifier = (char)0x10, Tag = @"\\[Xx]", RegexStr = @"[\w\s.,\$!\(\)-/\\\?;:#&""'\+]+" } },
            // \scroll: Force scroll
            { "Scroll", new TextCommand { Identifier = (char)0x11, Tag = @"\\[Ss][Cc][Rr][Oo][Ll][Ll]", RegexStr = null } },
            // \loc: Location display
            { "Loc",    new TextCommand { Identifier = (char)0x17, Tag = @"\\[Ll][Oo][Cc]", RegexStr = @"[\w\s.,\$!\(\)-/\\\?;:#&""'\+]+" } },
            // \sound: Play sound
            { "Sound",  new TextCommand { Identifier = (char)0x19, Tag = @"\\[Ss][Oo][Uu][Nn][Dd]", RegexStr = @"[\w\s.,\$!\(\)-/\\\?;:#&""'\+]+" } },
            // \event: Continue event processing
            { "Event",  new TextCommand { Identifier = (char)0x1a, Tag = @"\\[Ee][Vv][Ee][Nn][Tt]", RegexStr = null } },
            // \I: Insert face name
            { "I",      new TextCommand { Identifier = (char)0x1b, Tag = @"\\[Ii]", RegexStr = @"[-]{0,1}\d+" } },
            // \Quick: Skip through text without speech sound
            { "Quick",  new TextCommand { Identifier = (char)0x1c, Tag = @"\\[Qq][Uu][Ii][Cc][Kk]", RegexStr = @"[\w]+" } },
        };

        public char Identifier { get; private set; }
        public string Tag { get; private set; }
        private string RegexStr;
        private bool NewlineAfterFirst = false;
        public bool StartupCommand { get; private set; }
        public bool ExecutionNotCertain { get; private set; }

        public bool null_replacement { get { return string.IsNullOrEmpty(RegexStr); } }
        public string regex_string { get { return null_replacement ? "" : string.Format(@"\[({0})\]", RegexStr); } }

        public override string ToString()
        {
            return string.Format("Command {0}: {1} '{2}'", Tag, (int)Identifier, Identifier);
        }

        public Regex regex()
        {
            return new Regex(regex_string, RegexOptions.IgnoreCase);
        }
        public Regex tag_regex()
        {
            return new Regex(Tag + regex_string, RegexOptions.IgnoreCase);
        }

        public void replace_raw_tags(ref string str)
        {
            string replacement = null_replacement ? "" : "[$1]";
            replacement = string.Concat(marker_identifier, replacement);
            Regex r = tag_regex();
            if (NewlineAfterFirst)
            {
                str = r.Replace(str, replacement, 1);
                str = r.Replace(str, replacement + "\n");
            }
            else
                str = r.Replace(str, replacement);
        }
        
        public static string remove_tags(string str, params string[] exceptions)
        {
            return remove_tags(str, (IEnumerable<string>)exceptions);
        }
        public static string remove_tags(string str, IEnumerable<string> exceptions = null)
        {
            foreach (var command in COMMANDS
                    .Where(x => exceptions == null || !exceptions.Contains(x.Key)))
                str = command.Value.tag_regex().Replace(str, "");
            return str;
        }

        public static string key_from_identifier(char identifier)
        {
            if (!COMMANDS.Any(x => x.Value.Identifier == identifier))
                return null;
            return COMMANDS.Single(x => x.Value.Identifier == identifier).Key;
        }

        public string marker_identifier { get { return string.Concat(MARKER, Identifier); } }
    }
}
