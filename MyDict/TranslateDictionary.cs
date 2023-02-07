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
                foreach (var pair in _words)
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

            List<int> temp = GetMapping(word, _base);
            if (temp.Count == 1)
                return false;
            _mergeList.Remove(new Tuple<int, int>(word.GetHashCode(), translatedWord.GetHashCode()));

            _translatedWords.Remove(word);
            RemoveWordsWithoutConnection(_translate);
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
            RemoveWordsWithoutConnection(_translate);
            return true;
        }
        
        

        private void RemoveWordsWithoutConnection(Language lang)
        {
            if (lang == _translate)
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