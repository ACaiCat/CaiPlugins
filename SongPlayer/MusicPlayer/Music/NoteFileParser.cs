using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;

namespace MusicPlayer.Music
{
    public enum Instrument
    {
        Guitar = 4057,
        Drums = 4673,
        Bell = 507,
        Axe = 1305,
        Harp
    }
    class NoteFileParser
    {
        public static int Tempo = 250;
        public static Instrument Instrument;

        public static List<List<Note>> Read(string path, out int tempo)
        {
            List<List<Note>> Notes = new List<List<Note>>();
            int t = Tempo;
            using (var reader = new StreamReader(path))
            {
                string line = "";
                bool readTempo = false;
                while ((line = reader.ReadLine()) != null)
                {
					if (!string.IsNullOrWhiteSpace(line) && (line.Trim())[0] == '#')
	                {
						//this line is a comment, skip
		                continue;
	                }

                    if (!readTempo)
                    {
                        int tryOut = 0;
                        if (int.TryParse(line, out tryOut))
                        {
                            t = tryOut;
                        }

                        readTempo = true;
                        continue;
                    }

                    List<Note> ret = new List<Note>();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        foreach (var note in line.Split(',').ToList())
                        {
                            try
                            {
                                ret.Add(new Note()
                                            {
                                                Value = NoteName.GetNoteByName(note)
                                            }
                                        );
                            }
                            catch (ArgumentException e)
                            {
                                TShock.Log.ConsoleError("Failed to read note: {0}", note );
                            }
                        }
                    }
                    {
                        ret.Add(new Note()
                                    {
                                        Value = -2   
                                    }
                                );
                    }
                    Notes.Add(ret);
                }
            }
            tempo = t;
            return Notes;
        }
    }
}
