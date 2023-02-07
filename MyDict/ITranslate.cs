using System.Collections.Generic;

namespace MyDict
{
    public interface ITranslate
    {
        void Add(string word, string translateWord);
        bool Remove(string word, string translatedWord);
        void Set(string word, string oldWord, string newWord);
        List<string> Translate(string word);
    }
}