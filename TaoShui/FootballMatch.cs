using System;

namespace TaoShui
{
    public class FootballMatch
    {
        public FootballMatch(string name, DateTime startTime, int[] data)
        {
            Name = name;
            StartTime = startTime;
            Data = data;
        }

        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public int[] Data { get; set; }
    }
}