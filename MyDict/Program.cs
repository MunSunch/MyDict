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

            var res = dictionary.Translate("получить");

            bool removeStatus = dictionary.Remove("house", "дом");
        }
    }
}