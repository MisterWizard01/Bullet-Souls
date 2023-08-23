using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class FileManager
    {
        public static string GetCommonFolder()
        {
            string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (path == null)
            {
                path = "";
            }
            string[] pathSplit = path.Split("\\");
            string newPath = "";
            for (int i = 0; i < pathSplit.Length - 4; i++)
            {
                newPath = Path.Combine(newPath, pathSplit[i]);
            }
            return Path.Combine(newPath, "Common");
        }
    }
}
