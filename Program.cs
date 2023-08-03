using System;
using System.IO;
using System.Text.RegularExpressions;
/* 
 * Простенькая программка для сохранения демок в l4d2, она просто раскладывает готовые демки по полочкам, в идеале
 * к ней еще нужен l4d2 конфиг для записи демок в разные файлы
 * 
 * исполняемый файл программы надо положить в папку куда сохраняются демки
 * 
 * как скомпилировать если у вас нет visual studio: я хз
*/
namespace Save_Demo
{
    class Program
    {
        const string delimitor = "--------------------------------------------------------------------------------";

        static bool deleteAutoDems = true;

        static void Main(string[] args)
        {
            // путь к этому файлу. Программа считает что демки лежат там же где и она
            string demosPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            // demosPath = @"C:\Program Files (x86)\Steam\steamapps\common\Left 4 Dead 2\left4dead2\demos";

            string command = "";
            while (true)
            {
                try
                {
                    Console.Clear();
                    if (deleteAutoDems) Console.WriteLine("автоматически сделаные демки удаляются!");
                    if (!deleteAutoDems) Console.WriteLine("автоматически сделаные демки сохраняются");
                    Console.WriteLine("- удалить эти демки, ! переключить сохранение автодемок, = сохранить без имени");
                    Console.WriteLine(delimitor);
                    Console.WriteLine(DateTime.Now.ToString() + " " + DateTime.Now.Millisecond.ToString());
                    Console.WriteLine();

                    var dems = Directory.GetFiles(demosPath, "*.dem");

                    if (dems.Length == 0)
                    {
                        Console.WriteLine("дэмок нет :(");
                    }
                    else
                    {
                        Console.WriteLine("найденые демки:");
                        foreach (var demo in dems)
                        {
                            Console.WriteLine(Path.GetFileName(demo));
                        }
                        Console.WriteLine();
                        Console.WriteLine("Как назвать это безобразие?");
                    }

                    // чтение комманды
                    command = Console.ReadLine().Trim();

                    if (command == "") continue;
                    if (command == "-")
                    {
                        foreach (var demo in dems)
                        {
                            File.Delete(demo);
                        }
                        continue;
                    }

                    if (command == "!")
                    {
                        deleteAutoDems = !deleteAutoDems;
                        continue;
                    }

                    if (command == "=")
                    {
                        command = "";
                    }

                    // во всех остальных случаях комманда это имя этого безобразия
                    if (dems.Length != 0)
                    {
                        // чтение имени игрока из заголовка демки
                        // https://developer.valvesoftware.com/wiki/DEM_Format#Demo_Header
                        string playerName;
                        using (BinaryReader reader = new BinaryReader(File.Open(dems[0], FileMode.Open)))
                        {
                            reader.BaseStream.Seek(276, 0);
                            // костыль, я не знаю что это за прикол, я должен считывать 260 байт
                            // но почему-то с именем "Не еш, подумой" считывается больше и это все ломает
                            // косяк в кодировке? не знаю, и знать не хочу, просто макс длинна имени будет меньше
                            playerName = new String(reader.ReadChars(200)).Trim('\x0');
                        }

                        DateTime date = File.GetCreationTime(dems[0]);

                        // использую String.Format для совместимости с древними компиляторами (зря кста)
						// а символ : низя использовать в названиях файлов :(
                        string dirname = String.Format("{0}.{1:d2}.{2:d2} {3:d2}-{4:d2}   {5}   {6}", 
                            date.Year, date.Month, date.Day, date.Hour, date.Minute, command, playerName).Trim();

                        Directory.CreateDirectory(Path.Combine(demosPath, dirname));

                        foreach (var demo in dems)
                        {
                            if (deleteAutoDems)
                                if (Regex.IsMatch(Path.GetFileName(demo), @".+_\d+.dem"))
                                {
                                    File.Delete(demo);
                                    continue;
                                }


                            // чтение времени игры и названия карты из заголовка демки
                            // https://developer.valvesoftware.com/wiki/DEM_Format#Demo_Header
                            string duration;
                            string mapName;

                            using (BinaryReader reader = new BinaryReader(File.Open(demo, FileMode.Open)))
                            {
                                reader.BaseStream.Seek(536, 0);
                                mapName = new String(reader.ReadChars(260)).Trim('\x0');

                                reader.BaseStream.Seek(1056, 0);
                                int seconds = (int)reader.ReadSingle();
                                duration = string.Format("{0:d2}-{1:d2}", seconds / 60, seconds % 60);
                            }

                            string newPath = Path.Combine(
                            demosPath,
                            dirname,
                            String.Format("{0}   {1} {2} .dem",
                                Path.GetFileNameWithoutExtension(demo),
                                duration,
                                mapName
                                )
                            );
                            File.Move(demo, newPath);

                        }

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("\n" + delimitor);
                    Console.WriteLine(e.Message);
                    Console.ReadKey();
                }

            }

        }
    }
}
