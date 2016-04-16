﻿namespace NineWorldsDeep.Studio
{
    public class Note
    {
        private string noteName;
        private int positionalValue;

        public Note(string name)
        {
            this.noteName = name;
            this.positionalValue = ParseNameToAbsVal(noteName);
        }

        public int PosVal
        {
            get
            {
                return positionalValue;
            }
        }

        public static int ParseNameToAbsVal(string name)
        {
            switch (name)
            {
                case "C":
                    return 0;
                case "C#/Db":
                    return 1;
                case "D":
                    return 2;
                case "D#/Eb":
                    return 3;
                case "E":
                    return 4;
                case "F":
                    return 5;
                case "F#/Gb":
                    return 6;
                case "G":
                    return 7;
                case "G#/Ab":
                    return 8;
                case "A":
                    return 9;
                case "A#/Bb":
                    return 10;
                case "B":
                    return 11;
                default:
                    throw new InvalidNoteParseException();
            }
        }

        public override string ToString()
        {
            return noteName;
        }
    }
}