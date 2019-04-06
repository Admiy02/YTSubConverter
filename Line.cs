﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Arc.YTSubConverter
{
    internal class Line : ICloneable
    {
        public Line(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public Line(DateTime start, DateTime end, string content)
        {
            Start = start;
            End = end;
            Sections.Add(new Section(content));
        }

        public Line(DateTime start, DateTime end, IEnumerable<Section> sections)
        {
            Start = start;
            End = end;
            Sections.AddRange(sections);
        }

        public DateTime Start
        {
            get;
            set;
        }

        public DateTime End
        {
            get;
            set;
        }

        public List<Section> Sections { get; } = new List<Section>();

        public string Text
        {
            get { return string.Join("", Sections.Select(s => s.Text)); }
        }

        public AnchorPoint AnchorPoint
        {
            get;
            set;
        } = AnchorPoint.BottomCenter;

        public PointF? Position
        {
            get;
            set;
        }

        public VerticalTextType VerticalTextType
        {
            get;
            set;
        }

        public RubyPosition RubyPosition
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Text;
        }

        public virtual object Clone()
        {
            Line newLine = new Line(Start, End);
            newLine.Assign(this);
            return newLine;
        }

        protected virtual void Assign(Line line)
        {
            Start = line.Start;
            End = line.End;
            AnchorPoint = line.AnchorPoint;
            Position = line.Position;
            VerticalTextType = line.VerticalTextType;
            RubyPosition = line.RubyPosition;

            Sections.Clear();
            foreach (Section section in line.Sections)
            {
                Sections.Add((Section)section.Clone());
            }
        }
    }
}
