using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MusicPlayer.Music;
using TShockAPI;

namespace MusicPlayer
{
    class SongPlayer
    {
        public bool Listening { get; set; }
        public TSPlayer Player { get; set; }
        public Song currentSong = null;

        public SongPlayer(TSPlayer ply)
        {
            Player = ply;
            Listening = false;
        }

        public void StartSong(Song s)
        {
            if (currentSong != null)
            {
                currentSong.EndSong();
            }

            currentSong = s;
            currentSong.StartSong();
        }

        public void EndSong()
        {
            if (currentSong != null)
            {
                currentSong.EndSong();
            }

            currentSong = null;
        }
    }
}
