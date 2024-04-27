using TShockAPI;

namespace SwitchCommands
{
    public class CommandInfo
    {
        public SwitchPos point = null;
        public List<string> commandList = new List<string>();
        public float cooldown = 0;
        public bool ignorePerms = true;

    }

    public class SwitchPos
    {
        public int X = 0, Y = 0;

        public SwitchPos()
        {
            X = 0;
            Y = 0;
        }

        public SwitchPos(int x, int y)
        {
            X = x;
            Y = y;
        }


        public SwitchPos(string str)
        {
            X = int.Parse(str.Split(',')[0]);
            Y = int.Parse(str.Split(',')[1]);
        }
        public string ToSqlString()
        {
            return "{0},{1}".SFormat(X, Y);
        }
        public override string ToString()
        {
            return "{0},{1}".SFormat(X, Y);
        }

        public override bool Equals(object obj)
        {
            var check = obj as SwitchPos;

            if (check == null) return false;

            return check.X == X && check.Y == Y;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
