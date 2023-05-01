using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace PlaceholderAPI
{
    public class PlaceholderManager
    {
        private Dictionary<string, string> placeholders;
        private Dictionary<char, string> colors;
        public PlaceholderManager() 
        {
            placeholders = new Dictionary<string, string>();
            colors = new Dictionary<char, string>();
        }
        public string GetText(string text,TSPlayer player) 
        {
            Hooks.OnGetText(placeholders,player);
            foreach (var key in placeholders.Keys)
            {
                text=text.Replace(key,placeholders[key]);
            }
            return Colorful(text);
        }
        public string Colorful(string text)
        {
            string final = "";
            var flag = text.StartsWith("&");
            var texts = text.Split(new char[]{ '&'}, StringSplitOptions.RemoveEmptyEntries);
            


            for (int i = 0; i < texts.Length; i++)
            {
                if (colors.ContainsKey(texts[i][0]))
                {
                    char letter = texts[i][0];
                    bool flag2 =  texts[i].Contains("[i:") || 
                                 texts[i].Contains("[c/") || 
                                 texts[i].Contains("[g:") ||
                                 texts[i].Contains("[i/");
                    if (flag&&i==0)
                    {
                        texts[i] = texts[i].Remove(0, 1);
                        texts[i] = texts[i].Color(colors[letter]);
                    }
                    
                    if (!flag2) 
                    {
                        texts[i] = texts[i].Remove(0, 1);
                        texts[i] = texts[i].Color(colors[letter]);
                    }
                    
                }
                final += texts[i];
            }
            if (string.IsNullOrEmpty(final)) final = text;
            return final;
        }
        public void Register(string key) 
        {
            if (!placeholders.ContainsKey(key))
            {
                placeholders.Add(key,"");
            }
            else
            {
                Console.WriteLine($"[PlaceholderAPI] 占位符 {key} 注册冲突");
            }
        }
        public void Deregister(string key) 
        {
            if (placeholders.ContainsKey(key))
            {
                placeholders.Remove(key);
            } 
        }
        public void InitializeColors()
        {
            colors.Add('1', "00008B");
            colors.Add('4', "8B0000");
            colors.Add('c', "FF0000");
            colors.Add('6', "FFD700");
            colors.Add('e', "FFFF00");
            colors.Add('2', "006400");
            colors.Add('a', "00FF00");
            colors.Add('b', "D4F2E7");
            colors.Add('3', "00CED1");
            colors.Add('9', "0000FF");
            colors.Add('d', "FF00FF");
            colors.Add('5', "8B008B");
            colors.Add('f', "FFFFFF");
            colors.Add('7', "808080");
            colors.Add('8', "696969");
            colors.Add('0', "000000");
            colors.Add('r', "FFFFFF");
        }
    }
}
