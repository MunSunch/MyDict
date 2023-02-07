# Задание 1. Создать приложение «Словари». #
### Основная задача проекта: хранить словари на разных языках и разрешать пользователю находить перевод нужного слова или фразы. ###
Интерфейс приложения должен предоставлять такие возможности:
- Создавать словарь. При создании нужно указать тип словаря. Например, англо-русский или русско-английский.
- Добавлять слово и его перевод в уже существующий словарь. Так как у слова может быть несколько переводов, необходимо поддерживать возможность создания нескольких вариантов перевода.
- Заменять слово или его перевод в словаре.
- Удалять слово или перевод. Если удаляется слово, все его переводы удаляются вместе с ним. Нельзя удалить перевод слова, если это последний вариант перевода.
- Искать перевод слова.
- Словари должны храниться в файлах.
- Слово и варианты его переводов можно экспортировать в отдельный файл результата.
- При старте программы необходимо показывать меню для работы с программой. Если выбор пункта меню открывает подменю, то тогда в нем требуется предусмотреть возможность возврата в предыдущее меню.

Ход решения:
Отразим основные возможности словаря в интерфейсе ITranslate.cs:

    public interface ITranslate
    {
        void Add(string word, string translateWord);
        bool Remove(string word, string translatedWord);
        void Set(string word, string oldWord, string newWord);
        List<string> Translate(string word);
    }

Каждому слову будет соответствовать несколько переводов и , наоборот, каждому переводу будет соответствовать несколько слов.
Значит, между такими словами существует связь **многие-ко многим**. Из теории баз данных для приведения в нормализованную форму такую задачу
обычно сводят к декомпозиции на отдельные таблицы с добавлением соединительной таблицы. Позаимствуем идею и реализуем подобну структуру.
Создадим класс TranslateDictionary.cs, который будет реализовывать интерфейс, описанный выше, и будет содержать следующие поля:

    private Dictionary<string, int> _words;
    private Dictionary<string, int> _translatedWords;
    private LinkedList<Tuple<int, int>> _mergeList;

### Добавление в словарь ###
1) Если слово уже есть, но нет его перевода, то добавляем перевод и связку слово-перевод в соединительный контейнер;
2) Если слова нет, но есть перевод, то добавляем слово и связку слово-перевод в соединительный контейнер;
3) Если слова и его перевода нет, то вставляем их в соответствующие подсловари и добавляем их связку
в соединительный контейнер.

Подсчитаем сложность операции: 
- так как коллекция Dictionary<> основана на коллекции HashTable и коллизий практически не будет, то поиск ключа составит O(1);
- вставка записи что в Dictionary, что в List составит O(1).

В итоге получим O(1). Реализация:

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

### Поиск перевода ###
Если слово присутствует в словаре, то получаем его хэш и перебираем все записи в соединительном контейнере:
заводим временный контейнер с хэшами, и при совпадении хэша и хэша записи на соответствующей итерации-добавляем его 
во временный контейнер. Затем поэлементно перебираем Dictionary с переводами и временный контейнер, также сравнивая хэши и добавляя в уже 
результирующий контейнер со словами.
Тут же стоит учесть, что слово может являться словом-переводом, тогда действия будут аналогичные за исключением сравнения
определенных элементов(смотри в реализацию).

Подсчитаем сложность: 
1) Поиск слова в Dictionary - O(1)
2) Сопоставление хэшей в соединительном контейнере - O(n)
3) Сопоставление хэшей в Dictionary-переводы и временном контейнере  - O(n * m)

В итоге получаем O(n + n*m) = O(nm). Реализация: 

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

Реализация вспомогательной функции:

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

### Удаление слова с его переводами или перевода ###
По условию требуется удалить слово вместе со всеми его переводами. Для этого получаем хеш слова, 
проходимся по соединительному контейнеру и находим сопоставления, удаляем их. После проходимся по контейнеру
с переводами и удаляем те слова, которые не имеют связки в соединительном контейнере.

Оценим сложность операции:
1) Получение хэша слова - О(1);
2) Выявление сопоставлений - O(n);
3) Удаление связок - O(n*m);
4) Удаление переводов - O(n*m).
 
В итоге получим O(2nm + n) = O(n*m). Реализация: 

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

Метод для удаления слов без связки:

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

По условию можно удалить отдельный перевод слова. Опять же находим сопоставления, и, если возможных переводов
у слова больше одного, то удаляем связку и перевод слова. Проходимся по контейнеру с переводами и удаляем переводы
без связок.

Реализация:

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

### Замена перевода ###
Добавляем связку и перевод, удаляем старую связку, проходимся по контейнеру с переводами и удаляем переводы
без связок.
Реализация: 

    public void Set(string word, string oldWord, string newWord)
    {
        Add(word, newWord);
        _mergeList.Remove(new Tuple<int, int>(word.GetHashCode(), oldWord.GetHashCode()));
        RemoveWordsWithoutConnection(_translateLanguage);
    }

Сложность - O(n^2): 
1) Добавление - O(1);
2) Удаление связки - O(n);
3) Удаление переводов без связки - O(n^2).




















