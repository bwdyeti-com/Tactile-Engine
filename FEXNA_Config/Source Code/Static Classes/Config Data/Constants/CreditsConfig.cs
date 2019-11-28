using System.Collections.Generic;

namespace FEXNA.Constants
{
    public class Credits
    {
        public const string FULL_CREDITS_LINK = "";
        //public const string FULL_CREDITS_LINK = "put.yoursite.herethough/yourgame/credits.html";

        public static List<CreditsEntry> CREDITS = new List<CreditsEntry>
        {
            new CreditsEntry("[your game name]", "[your name]"),

            new CreditsEntry("FEXNA Engine", "BwdYeti"),

            new CreditsEntry("Technologies",
                "Microsoft XNA",
                "NVorbis",
                "MonoGame"),

            /*
            //@Debug: Example credits entries from 7x:

            new CreditsEntry("Lead Developer", "BwdYeti"),
            new CreditsEntry("Lead Designer", "Michael \"Myke\" Cameron"),

            new CreditsEntry("Character Art",
                "Justin \"Vamp\" Ilijevski",
                "Nicholas \"Merc\" Freeman",
                "illuthra",
                "The Blind Archer",
                "SqrtOfPi",
                "Mewiyev",
                "Luminescent Blade",
                "Char",
                "Nih"),
            new CreditsEntry("Digital Art",
                "Israel Terenzi",
                "AthenaWyrm"),
            new CreditsEntry("Animation",
                "Kitsu",
                "Captain Supreme"),
            new CreditsEntry("Map Design",
                "Feaw",
                "Skitty"),
            new CreditsEntry("Music",
                "Justin Boyd",
                "Alex Dragonetti"),
            new CreditsEntry("Graphic Design",
                "monkeybard",
                "SgtSmilies",
                "Black Mage"),

            new CreditsEntry(""),

            new CreditsEntry("Technologies",
                "Microsoft XNA",
                "NVorbis",
                "MonoGame"),

            new CreditsEntry("Special Thanks",
                "Arch",
                "Team Overtroll",
                "Skylessia Chat"),

            new CreditsEntry(""),

            new CreditsEntry("Supporters",
                "Alexander Bartnik",
                "Alexandre Girard",
                "Alexis Berkowitz",
                "Andrew Lesniak",
                "Arinchai Charoenrak",
                "Amaury Houzelot",
                "Brivio Sergio",
                "Charles Rainey",
                "Corin Winnington",
                "Daniel Steritz",
                "Deranger",
                "Grant Shope",
                "Isaac Stevens",
                "Jason Gordon",
                "Jeffrey Taft",
                "Jermy Jose",
                "Jesse Thivierge",
                "Josh Davis",
                "Justin Pickney",
                "Keenan Prendergast",
                "Khang Nguyen",
                "Lester Ong",
                "Link Xter",
                "Lucas Pastori",
                "Lucien Tan",
                "Luminescent Blade",
                "Marc Goldstein",
                "Matthew Lyons",
                "Naphat Pattanapeeradej",
                "NickT",
                "Noah Rademacher",
                "Oscar Martinez",
                "Rohan Ambolkar"),*/
        };
    }

    public struct CreditsEntry
    {
        private List<string> _Names;

        public string Role { get; private set; }
        public List<string> Names
        {
            get { return new List<string>(_Names); }
            private set { _Names = value; }
        }

        public CreditsEntry(string role, List<string> names) : this()
        {
            Role = role;
            _Names = new List<string>(names);
        }
        public CreditsEntry(string role, params string[] names) : this()
        {
            Role = role;
            _Names = new List<string>(names);
        }
    }
}
