﻿namespace Scheduling.Core.FJSP
{
    public class Job(int id)
    {
        public int Id { get; set; } = id;

        public long ReleaseDate { get; set; }

        public LinkedList<Operation> Operations { get; } = [];
    }
}
