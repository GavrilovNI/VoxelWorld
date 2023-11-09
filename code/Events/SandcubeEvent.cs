using Sandbox;
using Sandcube.Worlds.Blocks;

namespace Sandcube.Events;

public static class SandcubeEvent
{
    public static class Game
    {
        public static string Start = $"{SandcubeGame.ModName}.Start";
        public class StartAttribute : EventAttribute
        {
            public StartAttribute() : base(Start)
            {
            }
        }

        public static string Stop = $"{SandcubeGame.ModName}.Stop";
        public class StopAttribute : EventAttribute
        {
            public StopAttribute() : base(Stop)
            {
            }
        }
    }
}
