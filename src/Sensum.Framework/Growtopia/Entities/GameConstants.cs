namespace Sensum.Framework.Growtopia.Entities;

public static class GameConstants
{
    public const int RED_GEIGER_SIGNAL = 0;
    public const int YELLOW_GEIGER_SIGNAL = 1065353216;
    public const int GREEN_GEIGER_SIGNAL = 1073741824;
    public const int GREEN_RAPID_GEIGER_SIGNAL = 1073741825;
    public const int GEIGER_FOUND = 1077936128;

    public const byte STORAGE_BOX_MAX_CAPACITY = 20;
    public const byte STORAGE_BOX_2_MAX_CAPACITY = 40;
    public const byte STORAGE_BOX_3_MAX_CAPACITY = 90;

    public static readonly string[] GUEST_NAMES =
    [
        "Gar", "Lite", "Rat", "Mouse", "Bucks", "Cry", "Board", "You", "Flash", "Banana", "Einst", "Azure", "Punch",
        "Laugh", "Solid", "Snake", "Duck", "Len", "Sickle", "Smile", "Bill", "Joy", "Shiny", "Watch", "Pie", "Dawn",
        "Brave", "Head", "Fairy", "Smell", "Dar", "Tiny", "Krazy", "Burp", "Tickle", "Wiggle", "Squish", "Fun", "Good",
        "Bad", "Fire", "Cake", "Tor"
    ];

    public static readonly string[] KLV_SALTS =
    [
        "e9fc40ec08f9ea6393f59c65e37f750aacddf68490c4f92d0d2523a5bc02ea63",
        "c85df9056ee603b849a93e1ebab5dd5f66e1fb8b2f4a8caef8d13b9f9e013fa4",
        "3ca373dffbf463bb337e0fd768a2f395b8e417475438916506c721551f32038d",
        "73eff5914c61a20a71ada81a6fc7780700fb1c0285659b4899bc172a24c14fc1"
    ];
}