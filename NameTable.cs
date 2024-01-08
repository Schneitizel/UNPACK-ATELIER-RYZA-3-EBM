using System.Collections.Generic;

namespace UnpackEBM
{
    public struct customTable
    {
        public customTable(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }

        public override string ToString() => $"({Id}, {Name})";

        public static void AddAllcustomTable(List<customTable> charactersName)
        {
            charactersName.Add(new customTable("0","Ryza"));
			charactersName.Add(new customTable("1","Klaudia"));
			charactersName.Add(new customTable("2","Lent"));
            charactersName.Add(new customTable("3","Tao"));
			charactersName.Add(new customTable("4","Patricia"));
			charactersName.Add(new customTable("5","Empel"));
			charactersName.Add(new customTable("6","Lila"));
            charactersName.Add(new customTable("7","Bos"));
			charactersName.Add(new customTable("8","Federica"));
			charactersName.Add(new customTable("9","Dian"));
			charactersName.Add(new customTable("a","Kala"));
        }
    }
}
