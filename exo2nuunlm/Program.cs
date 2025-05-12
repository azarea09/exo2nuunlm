using System;
using exo2nuunlm;

namespace exo2nuunlm;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("ファイルをドラッグアンドドロップしてください。");
            return;
        }

        foreach (var filePath in args)
        {
            if (Path.GetExtension(filePath) == ".exo")
            {
                exo2nuunlm.Save(filePath);
            }
        }
    }
}

