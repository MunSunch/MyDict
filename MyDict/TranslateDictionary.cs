using System;
using System.Collections.Generic;

namespace MyDict
{
    public class TranslateDictionary: ITranslate
    {
        private Dictionary<string, int> _words;
        private Dictionary<string, int> _translatedWords;
        private LinkedList<Tuple<int, int>> _mergeList;

        private Language _baseLanguage;
        private Language _translateLanguage;
        
        private static string UNKNOWN_TRANSLATED_WORD = "*";

        public TranslateDictionary(Language l1, Language l2)
        {
            _baseLanguage = l1;
            _translateLanguage = l2;
            _words = new Dictionary<string, int>();
            _translatedWords = new Dictionary<string, int>();
            _mergeList = new LinkedList<Tuple<int, int>>();
        }

        public Language BaseLanguageLang => _baseLanguage;
        public Language TranslateLanguageLang => _translateLanguage;
        public static string UnknownTranslatedWord => UNKNOWN_TRANSLATED_WORD;

        
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
            _mergeList.AddLast(new Tuple<int, int>(word.GetHashCode(), translateWord.GetHashCode()));
        }

        public List<string> Translate(string word)
        {
            List<string> result = new List<string>();
            if (_words.ContainsKey(word))
            {
                List<int> temp = GetMapping(word, _baseLanguage);
                foreach (var pair in _translatedWords)
                {
                    if(temp.Contains(pair.Value))
                        result.Add(pair.Key);
                }
            } 
            else if (_translatedWords.ContainsKey(word))
            {
                List<int> temp = GetMapping(word, _translateLanguage);
                foreach (var pair in _words)
                {
                    if(temp.Contains(pair.Value))
                        result.Add(pair.Key);
                }
            }
            else
            {
                result.Add(UNKNOWN_TRANSLATED_WORD);
            }

            return result;
        }

        private List<int> GetMapping(string word, Language target)
        {
            List<int> result = new List<int>();
            if (target == _baseLanguage)
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
                int hash = _translatedWords[word];
                foreach (var item in _mergeList)
                {
                    if(item.Item2 == hash)
                        result.Add(item.Item1);
                }
            }

            return result;
        }

        public bool Remove(string word, string translatedWord)
        {
            if (!_translatedWords.ContainsKey(translatedWord))
                return false;

            List<int> temp = GetMapping(word, _baseLanguage);
            if (temp.Count == 1)
                return false;
            _mergeList.Remove(new Tuple<int, int>(word.GetHashCode(), translatedWord.GetHashCode()));

            _translatedWords.Remove(word);
            RemoveWordsWithoutConnection(_translateLanguage);
            return true;
        }

        public bool Remove(string word)
        {
            if (!_words.ContainsKey(word))
                return false;
            
            int hash = _words[word];
            List<Tuple<int, int>> temp = new List<Tuple<int, int>>();
            foreach (var item in _mergeList)
            {
                if (item.Item1 == hash)
                    temp.Add(new Tuple<int, int>(item.Item1, item.Item2));
            }

            foreach (var item in temp)
            {
                _mergeList.Remove(item);
            }

            _words.Remove(word);
            RemoveWordsWithoutConnection(_translateLanguage);
            return true;
        }
        
        public void Set(string word, string oldWord, string newWord)
        {
            Add(word, newWord);
            _mergeList.Remove(new Tuple<int, int>(word.GetHashCode(), oldWord.GetHashCode()));
            RemoveWordsWithoutConnection(_translateLanguage);
        }

        private void RemoveWordsWithoutConnection(Language lang)
        {
            if (lang == _translateLanguage)
            {
                if (_mergeList.Count == 0)
                    _translatedWords.Clear();

                List<KeyValuePair<string, int>> temp = new List<KeyValuePair<string, int>>();
                foreach (var pair in _translatedWords)
                {
                    int hash = pair.Value;
                    bool flag = true;
                    foreach (var item in _mergeList)
                    {
                        if (item.Item2 == hash)
                        {
                            flag = false;
                            break;
                        }
                    }

                    if (flag)
                        temp.Add(new KeyValuePair<string, int>(pair.Key, pair.Value));
                }

                foreach (var pair in temp)
                {
                    _translatedWords.Remove(pair.Key);
                }
            }
            else
            {
                if (_mergeList.Count == 0)
                    _words.Clear();
                
                List<KeyValuePair<string, int>> temp = new List<KeyValuePair<string, int>>();
                foreach (var pair in _words)
                {
                    int hash = pair.Value;
                    bool flag = true;
                    foreach (var item in _mergeList)
                    {
                        if (item.Item1 == hash)
                        {
                            flag = false;
                            break;
                        }
                    }

                    if (flag)
                        temp.Add(new KeyValuePair<string, int>(pair.Key, pair.Value));
                }
                foreach (var pair in temp)
                {
                    _words.Remove(pair.Key);
                }
            }
        }
    }

    public enum Language
    {
        RUS, ENG
    }
}