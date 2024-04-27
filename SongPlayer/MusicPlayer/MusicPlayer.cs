using System;
using System.IO;
using MusicPlayer.Music;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace MusicPlayer
{
    [ApiVersion(2,1)]
    public class MusicPlayer : TerrariaPlugin
    {
        public string songPath;
        private SongPlayer[] songPlayers = new SongPlayer[255];

        public override string Author
        {
            get { return "Olink and Ijwu"; }
        }

        public override string Description
        {
            get { return "Plays melodical songs."; }
        }

        public override string Name
        {
            get { return "Song Player"; }
        }

        public override Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        public MusicPlayer(Main game) : base(game)
        {
            songPath = Path.Combine(TShock.SavePath, "Songs");
            if (!Directory.Exists(songPath))
            {
                Directory.CreateDirectory(songPath);
            }
        }

        public override void Initialize()
        {
            TShockAPI.Commands.ChatCommands.Add(new Command("", PlaySong, "play"));
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
            }
            
            base.Dispose(disposing);
        }

        private void OnJoin(GreetPlayerEventArgs args)
        {
            int who = args.Who;
            songPlayers[who] = new SongPlayer(TShock.Players[who]);
        }

        private void OnLeave(LeaveEventArgs args)
        {
            int who = args.Who;
            songPlayers[who] = null;
        }

        public void PlaySong(CommandArgs args)
        {
            if (songPlayers[args.Player.Index] == null)
                return;

            if (args.Parameters.Count == 0 && !songPlayers[args.Player.Index].Listening)
            {
                args.Player.SendInfoMessage("Usage: /play \"Song name\"");
                args.Player.SendInfoMessage("use /play again to stop the song.");
                return;
            }
            
            if (args.Parameters.Count == 0)
            {
                songPlayers[args.Player.Index].EndSong();
                return;
            }

            String songName = string.Join(" ", args.Parameters);
            String path = Path.Combine(songPath, songName);
            if (!File.Exists(path))
            {
                args.Player.SendErrorMessage("Failed to load song: '{0}'", songName);
                return;
            }

            Song s = new Song(this, songName, path, songPlayers[args.Player.Index]);
            songPlayers[args.Player.Index].StartSong(s);
        }
    }
}
