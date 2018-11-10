﻿using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Arc.YTSubConverter
{
    internal class SrtDocument : SubtitleDocument
    {
        public SrtDocument()
        {
        }

        public SrtDocument(SubtitleDocument doc)
            : base(doc)
        {
        }

        public SrtDocument(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                while (true)
                {
                    string lineNr = reader.ReadLine();
                    if (lineNr == null)
                        break;

                    if (string.IsNullOrWhiteSpace(lineNr))
                        continue;

                    string timestamps = reader.ReadLine();
                    if (timestamps == null)
                        break;

                    string content = null;
                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                            break;

                        if (content != null)
                            content += "\r\n";

                        content += line;
                    }

                    (DateTime start, DateTime end) = ParseTimestamps(timestamps);
                    Lines.Add(new Line(start, end, content));
                }
            }
        }

        public override void Save(string filePath)
        {
            int index = 1;
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (Line line in Lines)
                {
                    writer.WriteLine(index.ToString());
                    writer.WriteLine(FormatTimestamps(line.Start, line.End));
                    writer.WriteLine(line.Text);
                    writer.WriteLine();

                    index++;
                }
            }
        }

        private static (DateTime, DateTime) ParseTimestamps(string timestamps)
        {
            Match match = Regex.Match(timestamps, @"^(\d+):(\d+):(\d+),(\d+) --> (\d+):(\d+):(\d+),(\d+)");
            return (
                new DateTime(
                    2000,
                    1,
                    1,
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value),
                    int.Parse(match.Groups[4].Value)
                ),
                new DateTime(
                    2000,
                    1,
                    1,
                    int.Parse(match.Groups[5].Value),
                    int.Parse(match.Groups[6].Value),
                    int.Parse(match.Groups[7].Value),
                    int.Parse(match.Groups[8].Value)
                )
            );
        }

        private static string FormatTimestamps(DateTime startTime, DateTime endTime)
        {
            return $"{startTime.Hour:00}:{startTime.Minute:00}:{startTime.Second:00},{startTime.Millisecond:000} --> " +
                   $"{endTime.Hour:00}:{endTime.Minute:00}:{endTime.Second:00},{endTime.Millisecond:000}";
        }
    }
}
