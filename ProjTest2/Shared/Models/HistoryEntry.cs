using System;

namespace ProjTest2.Shared.Models
{
    public class HistoryEntry
    {
        private HistoryEntry() //EFC-contructor
        {
        }
        public HistoryEntry(DateTime date, Content content, Learner learner)
        {
            Date = date;
            Content = content;
            Learner = learner;
        }
        public int Id {get; set; }
        public DateTime Date { get; set; }
        public Content Content { get; set; }
        public Learner Learner { get; set; }
    }
}