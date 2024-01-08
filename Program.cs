using System.Text;
using System.Text.RegularExpressions;

namespace UnpackEBM
{
    class Program
    {
        public static string path = Environment.CurrentDirectory;
        private static List<customTable> charactersName = new List<customTable>(); 
        public static string file = "";
        public static bool forced = false;

        public static bool noLimit = false;

        public static string game = "AtelierRyza3";
        static void Main(string[] args)
        {

            if(args.Length == 0) // If testing (dotnet run), the local file will be used
            {
                file = "event_message_xf01_010.ebm";
            }
            else // Else, the first args is the path, and the second if the file
            {
                path = args[1];
                file = args[0];
            }
            
            if (!File.Exists(path + "\\" + file))
            {
                Console.WriteLine("File " + path + "\\" + file + " not found. (0x001)");
                Environment.Exit(-1);
                return;
            } 

            // NOT SURE AT 100% !
            customTable.AddAllcustomTable(charactersName); // Characters ID (NameTable.cs)

            forced = args.Contains("-forced");

            if(args.Length >= 3)
                game = args[2];

            if(file.EndsWith(".txt"))
            {
                Insert();
                return;
            }
            else
            {
                Extract();
                return;
            }
        }

        static void Extract(){
            if(!forced && File.Exists(path + "\\" + file.Replace(".ebm", ".txt")))
            {
                Console.WriteLine("The file " + file.Replace(".ebm", ".txt") + " already exist, overwrite canceled." + Environment.NewLine + "Use the command -forced to overwrite. (0x005)");
                Environment.Exit(0);
                return;
            }

            if(forced && File.Exists(path + "\\" + file.Replace(".ebm", ".txt")))
            {
                Console.WriteLine("The file " + file.Replace(".ebm", ".txt") + " already exist, but the command -forced was found ! Beware !!");
            }
            Console.WriteLine("Extract " + path + "\\" + file);
            FileStream fileStream = new FileStream(path + "\\" + file, FileMode.Open);
            fileStream.Seek(0, SeekOrigin.Begin);

            long nbMessage = 0;

            int beginHeader = 0; // Where First header is
            int offsetBeforeID = 0; // Where Text length is, after beginHeader
            int offesetBeforeText = 0; // Where Text is, offsetBeforeID
            int offsetNextHeader = 0; // Where the next header is, after the text

            // TO-DO : Support more games
            switch(game)
            {
                case "AtelierRyza3":
                    beginHeader = 0x8; 
                    offsetBeforeID = 0x36;
                    offesetBeforeText = 0x2;
                    offsetNextHeader = 0xD;
                    nbMessage = ReadBytes(fileStream, 1);
                break;
                case "BlueReflectionSL":
                    beginHeader = 0x10; 
                    offsetBeforeID = 0x12;
                    offesetBeforeText = 0x2;
                    offsetNextHeader = 0x11;
                    nbMessage = 0xFFFF - ReadBytes(fileStream, 2) + 1;
                break;
            }
            
            int length = 0;
            string resultat = "";

            fileStream.Seek(beginHeader, SeekOrigin.Begin);

            Console.WriteLine("The file contains " + nbMessage + " lines.");

            try
            {
                while(nbMessage > 0){
                                string tmp = "";

                                int i = ReadBytes(fileStream, 2); // We get the character's ID
                                var characterNameFound = charactersName.Find(characterName => characterName.Id.Equals(i.ToString("X")));
                                if(characterNameFound.Name == null)
                                    tmp += "[0x" + i.ToString("X") + "] ";
                                else
                                    tmp += "[" + characterNameFound.Name + "] ";

                                fileStream.Seek(offsetBeforeID, SeekOrigin.Current); // This is where the text length is

                                length = ReadBytes(fileStream, 2) - 1; // -1 letter, since the last s 0x0

                                fileStream.Seek(offesetBeforeText, SeekOrigin.Current); // This is where the text is

                                string text = ReadMessage(fileStream, length);

                                Regex regex = new Regex(@"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]"); // We ignore the line with "incorrect" characters, like japanese-characters or not-translating character

                                bool containsJapanese = regex.IsMatch(text);

                                if(text == "" || text == "ꆁꆁꆁ" || containsJapanese){
                                    nbMessage--;
                                    continue;
                                }

                                tmp += text;
                                if(nbMessage == 1)
                                    resultat += tmp;
                                else
                                    resultat += tmp + Environment.NewLine;

                                fileStream.Seek(offsetNextHeader, SeekOrigin.Current); // This is where the next header start, and loop!

                                if(file == "event_message_cm09_140.ebm" && nbMessage == 23 && game == "AtelierRyza3") // Quick fix, this file AND this position is weird for THIS game
                                {
                                    nbMessage--;
                                    fileStream.Seek(0x49, SeekOrigin.Current);
                                }
                                nbMessage--;
                            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if(resultat == ""){
                Console.WriteLine("The file contained no usable dialogs. File passed.");
                return;
            }

            string[] lignes = resultat.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string texteSansLignesVides = string.Join(Environment.NewLine, lignes);

            using(StreamWriter writetext = new StreamWriter(path + "\\" + file.Replace(".ebm", ".txt")))
            {
                writetext.WriteLine(texteSansLignesVides);
            }

        }

        static void Insert(){
            if (!File.Exists(path + "\\" + file.Replace(".txt", ".ebm"))) // Precautionary measure
            {
                Console.WriteLine("File EBM not found. (0x003)");
                Environment.Exit(-1);
                return;
            } 

            Console.WriteLine("Insert " + path + "\\" + file);
            string[] lines = File.ReadAllLines(path + "\\" + file.Replace(".ebm", ".txt")); // We load the .txt file

            FileStream fileStream = new FileStream(path + "\\" + file.Replace(".txt", ".ebm"), FileMode.Open); // Open the original .ebm file, which must have the same name as the .txt file
            MemoryStream memoire = new MemoryStream(); // We create a Memory Stream, faster and necessary
            fileStream.Seek(0, SeekOrigin.Begin);

            long nbMessage = -1;
            int headerSize = 0;
            int finalHeaderSize = 0;
            byte[] firstHeader;

            switch(game)
            {
                case "AtelierRyza3":
                    nbMessage = ReadBytes(fileStream, 1);
                    fileStream.Position = 0;
                    firstHeader = new byte[64];
                    fileStream.Read(firstHeader, 0, 64);
                    memoire.Write(firstHeader);
                    headerSize = 69;
                    finalHeaderSize = 65;
                break;
                case "BlueReflectionSL":
                    nbMessage = 0xFFFF - ReadBytes(fileStream, 2) + 1;
                    fileStream.Position = 0;
                    firstHeader = new byte[36];
                    fileStream.Read(firstHeader, 0, 36);
                    memoire.Write(firstHeader);
                    headerSize = 37;
                    finalHeaderSize = 33;
                break;
            }

            if(file == "event_message_cm09_140.txt" && game == "AtelierRyza3"){
                nbMessage--;
            }

            if(nbMessage != lines.Length) // To avoid line breaks by mistake, use a forced line break with <CR> in a .txt file (for the game)
            {
                Console.WriteLine("The number of lines in the .txt file (" + lines.Length + ") is different from the number of dialogs in the .ebm file (" + nbMessage + "). (0x004)");
                Environment.Exit(-1);
                return;
            }

            
            int a = 0;

            foreach(string s in lines){
                nbMessage--;
                if(s.Length == 0 || s == "") // The .txt file always contains an extra line break at the end
                    continue;
                a++;
                string message = s;

                while (message[message.Length - 1] == ' ')
                {
                    message = message.Substring(0, message.Length - 1);
                }

                if(message.StartsWith("[")){ // If the dialog begins with a character tag, we delete it
                    int b = message.IndexOf("] ")+2;
                    message = message.Substring(b);
                }

                int length = ReadBytes(fileStream, 2); // Load ORIGINAL text size
                byte[] ba = Encoding.Default.GetBytes(message); // The translated dialog is converted into bytes (é = 2 bytes, for example).
                memoire.Write(BitConverter.GetBytes(ba.Length+1)); // We rewrite the size of the translated text, size in bytes
                memoire.Write(ba); // We write the translated dialogue

                fileStream.Seek(length + 1, SeekOrigin.Current); // We move on to the next header

                if(file == "event_message_cm09_140.txt" && a == 4 && game == "AtelierRyza3"){
                    byte[] add = new byte[0x49];
                    fileStream.Read(add, 0, 0x49);
                    memoire.Write(add);
                }

                if(nbMessage == 0){ // If it's the last header, fewer bytes are written, as there is no "next dialog size" (0x4 bytes)
                    byte[] header = new byte[finalHeaderSize];
                    fileStream.Read(header, 0, finalHeaderSize);
                    memoire.Write(header);
                }else{
                    byte[] header = new byte[headerSize];
                    fileStream.Read(header, 0, headerSize);
                    memoire.Write(header);
                }
                
            }

            long rest = fileStream.Length - fileStream.Position;
            byte[] final = new byte[rest];

            fileStream.Read(final, 0, (int) rest);
            memoire.Write(final);

            fileStream.Close(); // Close the Stream of the original .ebm file to rewrite it

            using (var save = new FileStream(path + "\\" + file.Replace(".txt", ".ebm"), FileMode.Truncate))
            {
                memoire.WriteTo(save);
            }
        }

        static int ReadBytes(FileStream file, int size, long offset = 0)
        {
            byte[] bytes = new byte[size];
            file.Seek(0 + offset, SeekOrigin.Current);
            file.Read(bytes, 0, size);
            if (size == 4)
                return BitConverter.ToInt32(bytes, 0);
            if (size == 2)
                return BitConverter.ToUInt16(bytes, 0);
            else
                return bytes[0];
        }

        static string ReadMessage(FileStream stream, int size, long offset = 0)
        {
            byte[] bytes = new byte[size];
            stream.Seek(0 + offset, SeekOrigin.Current);
            stream.Read(bytes, 0, size);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}