using System;
using System.Collections.Generic;

namespace MyDict
{
    public class TranslateDictionary: ITranslate
    {
        private Dictionary<string, int> _words;
        private Dictionary<string, int> _translatedWords;
        private List<Tuple<int, int>> _mergeList;

        private Language _base;
        private Language _translate;
        
        private static string UNKNOWN_TRANSLATED = "*";

        public TranslateDictionary(Language l1, Language l2)
        {
            _base = l1;
            _translate = l2;
            _words = new Dictionary<string, int>();
            _translatedWords = new Dictionary<string, int>();
            _mergeList = new List<Tuple<int, int>>();
        }

        public void Add(string word, string translateWord)
        {
            bool presentInFirstDict = _words.ContainsKey(word);
            bool presentInSecondDict = _translatedWords.ContainsKey(translateWord);
            if (presentInFirstDict && !presentInSecondDict)
            {
                _translatedWords[translateWord] = translateWord.GetHashCode();
            }
            else if (!presentInFirstDict && presentInSecondDict)
            {
                _words[word] = word.GetHashCode();
            }
            else
            {
                _words[word] = word.GetHashCode();
                _translatedWords[translateWord] = translateWord.GetHashCode();
            } 
            _mergeList.Add(new Tuple<int, int>(word.GetHashCode(), translateWord.GetHashCode()));
        }
        
        public List<string> Translate(string word)
        {
            List<string> result = new List<string>();
            if (_words.ContainsKey(word))
            {
                List<int> temp = GetMapping(word, _base);
                foreach (var pair in _translatedWords)
                {
                    if(temp.Contains(pair.Value))
                        result.Add(pair.Key);
                }
            } 
            else if (_translatedWords.ContainsKey(word))
            {
                List<int> temp = GetMapping(word, _translate);
                foreach (var pair in _translatedWords)
                {
                    if(temp.Contains(pair.Value))
                        result.Add(pair.Key);
                }
            }
            else
            {
                result.Add(UNKNOWN_TRANSLATED);
            }

            return result;
        }

        private List<int> GetMapping(string word, Language target)
        {
            List<int> result = new List<int>();
            if (target == _base)
            {
                int hash = _words[word];
                foreach (var item in _mergeList)
                {
                    if(item.Item1 == hash)
                        result.Add(item.Item2);
                }
            }
            else
            {
                int hash = _words[word];
                foreach (var item in _mergeList)
                {
                    if(item.Item2 == hash)
                        result.Add(item.Item1);
                }
            }

            return result;
        }

        public void Remove(string word)
        {
            if (_words.ContainsKey(word))
            {
                
            }
            else if (_translatedWords.ContainsKey(word))
            {

            }
            else
            {
                throw new ArgumentException($"Deleting a non-existent word: {word}");
            }
        }

        public void SetWord(string word, string oldWord, string newWord)
        {
            throw new System.NotImplementedException();
        }

        
    }

    public enum Language
    {
        RUS, ENG
    }
}