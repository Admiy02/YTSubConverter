﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Arc.YTSubConverter.Ass;

namespace Arc.YTSubConverter
{
    internal abstract class SubtitleDocument
    {
        public static readonly DateTime TimeBase = new DateTime(2000, 1, 1);

        protected SubtitleDocument()
        {
        }

        protected SubtitleDocument(SubtitleDocument doc)
        {
            VideoDimensions = doc.VideoDimensions;
            Lines.AddRange(doc.Lines);
        }

        public Size VideoDimensions
        {
            get;
            set;
        }

        public List<Line> Lines { get; } = new List<Line>();

        public static SubtitleDocument Load(string filePath)
        {
            switch (Path.GetExtension(filePath)?.ToLower())
            {
                case ".ass":
                    return new AssDocument(filePath);

                case ".sbv":
                    return new SbvDocument(filePath);

                case ".srt":
                    return new SrtDocument(filePath);

                case ".ytt":
                    return new YttDocument(filePath);

                default:
                    throw new NotSupportedException();
            }
        }

        public void CloseGaps()
        {
            for (int i = 0; i < Lines.Count - 1; i++)
            {
                if (Math.Abs((Lines[i + 1].Start - Lines[i].End).TotalMilliseconds) < 50)
                    Lines[i].End = Lines[i + 1].Start;
            }
        }

        public void Shift(TimeSpan offset)
        {
            foreach (Line line in Lines)
            {
                line.Start = Shift(line.Start, offset);
                line.End = Shift(line.End, offset);
            }
        }

        private static DateTime Shift(DateTime time, TimeSpan offset)
        {
            time += offset;
            return time < TimeBase ? TimeBase : time;
        }

        public virtual void Save(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
