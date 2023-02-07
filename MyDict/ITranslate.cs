using System.Collections.Generic;

namespace MyDict
{
    public interface ITranslate
    {
        void Add(string word, string translateWord);
        void Remove(string word);
        void SetWord(string word, string oldWord, string newWord);
        List<string> Translate(string word);
    }
}