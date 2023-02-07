namespace MyDict
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            TranslateDictionary dictionary = new TranslateDictionary(Language.ENG, Language.RUS);
            dictionary.Add("get", "получить");
            dictionary.Add("get", "войти");
            dictionary.Add("get", "получать");
            dictionary.Add("in", "в");
            dictionary.Add("in", "на");
            dictionary.Add("in", "по");
            dictionary.Add("in", "под");
            dictionary.Add("house", "дом");
            dictionary.Add("save", "сохранять");
            dictionary.Add("keep", "сохранять");
            dictionary.Add("retain", "сохранять");


            var res = dictionary.Translate("сохранять");
           bool removeStatus = dictionary.Remove("save");
           bool removeStatus2 = dictionary.Remove("in", "на");
           dictionary.Remove("house", "дом");
        }
    }
}