using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using TerrariaApi.Server;
using TShockAPI;

[ApiVersion(2, 1)]
public class ProjSpawner : TerrariaPlugin
{

    public override string Author => "Cai";

    public override string Description => "生成弹幕在玩家头上!";

    public override string Name => "ProjSpawner";

    public override Version Version => new Version(1, 0, 0, 0);

    public ProjSpawner(Main game)
        : base(game)
    {
    }

    public override void Initialize()
    {
        Commands.ChatCommands.Add(new Command(new List<string> { "danmu.use" }, ProjSpawn, "弹幕生成","projspawn"));

    }

    private void ProjSpawn(CommandArgs args)
    {
        try
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendInfoMessage("[i:50][i:1359]/弹幕生成 <弹幕ID> <弹幕伤害>-----在自己头上生成指定伤害的指定弹幕!");
            }
            else
            {
                int num = int.Parse(args.Parameters[0]);
                int num2 = int.Parse(args.Parameters[1]);
                Vector2 vector = new Vector2(args.Player.TPlayer.Center.X, args.Player.TPlayer.Center.Y - 200f);
                Vector2 velocity = args.Player.TPlayer.velocity;
                Projectile.NewProjectile(null, vector, velocity, num, num2, 0f, 255, 0f, 0f);


            }
        }
        catch
        {
            args.Player.SendErrorMessage("[i:50][i:1359]请正确输入 /弹幕生成 <弹幕ID> <弹幕伤害>");
        }
    }


}
