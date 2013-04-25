using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace EWAHBitmap
{
    /// <summary>
    /// Класс, объекты которого представляют MarkerWords сжатого битового вектора
    /// </summary>
    public class MarkerWord
    {
        /// <summary>
        /// Список 64-битовых слов, представляющих сжатый битовый вектор
        /// </summary>
        List<long> words;

        /// <summary>
        /// Номер данного MarkerWord в списке 
        /// </summary>
        public int MWPosition;

        /// <summary>
        /// Конструктор экземпляра класса 
        /// </summary>
        /// <param name="words">Список 64-битовых слов</param>
        public MarkerWord(ref List<long> words)
        {
            this.words = words;
            MWPosition = 0;
        }

        /// <summary>
        /// Свойство, принимающее и задающее количество непустых 64-битовых слов, следующих после данного MarkerWord 
        /// </summary>
        public long DirtyWords
        {
            get 
            {
                return (words[MWPosition]) >> 33;
            }
            set 
            {
                long newnum = value << 33;
                words[MWPosition] = ((words[MWPosition] << 31) >> 31) | newnum;
            }
        }
        /// <summary>
        /// Свойство, принимающее и задающее количество пустых 64-битовых слов, обозначенных с помощью данного MarkerWord 
        /// </summary>
        public long EmptyWords
        {
            get
            {
                return ((words[MWPosition] << 31) >> 31) >> 1;
            }
            set
            {
                long newnum = value << 1;
                long pattern = ~((1L << 33) - 2);
                words[MWPosition] &= pattern;
                words[MWPosition] |= newnum; 
            }
        }
        /// <summary>
        /// Свойство, принимающее и задающее значение бита, обозначающего какого типа являются пустые слова в данном MarkerWord
        /// Свойство принимает значение true, если пустые слова являются последовательностями единичных битов,
        /// и false, если пустые слова являются последовательностями нулевых битов
        /// </summary>
        public bool emptyBit
        {
            get
            {
                return (words[MWPosition] % 2 == 0 ? false : true);
            }
            set
            {
                if (value)
                    words[MWPosition] |= 1L;
                else
                    words[MWPosition] &= ~1L; 
            }
        }
    }
    /// <summary>
    /// Класс, экземпляры которого содержат сжатый битовых вектор и операции над ним
    /// </summary>
    public class WordArray
    {
        /// <summary>
        /// Длина вектора в несжатом состоянии
        /// </summary>
        long bitSize;
        /// <summary>
        /// Количество 64-битовых слов в сжатом состоянии
        /// </summary>
        int size;
        /// <summary>
        /// Список 64-битовых слов
        /// </summary>
        List<long> words;
        /// <summary>
        /// Последний MarkerWord в списке слов
        /// </summary>
        MarkerWord currentMarkerWord;
        /// <summary>
        /// Максимально возможное количество пустых слов, которое может быть обозначено в MarkerWord
        /// </summary>
        const long maxEmptyWords = (1L << 32) - 1;
        /// <summary>
        /// Максимально возможное количество ytпустых слов, которое может следовать после MarkerWord
        /// </summary>
        const long maxDirtyWords = (1L << 31) - 1;
        /// <summary>
        /// Конструктор экземпляра класса WordArray
        /// </summary>
        public WordArray()
        {
            words = new List<long> {0};
            currentMarkerWord = new MarkerWord(ref words);
            size = 1;
            bitSize = 0;

        }
        /// <summary>
        /// Метод, изменяющий значение заданного в параметрах бита на единичку
        /// </summary>
        /// <param name="pos">Номер бита в несжатом векторе, значение которого необходимо сделать единичным
        /// Номер бита должен быть неменьшим числа бит в несжатом векторе</param>
        public void ToTrue(long pos)
        {
            if (pos >= bitSize) // Позиция изменяемого бита больше размера вектора в несжатом состоянии?
            {
                if (bitSize % 64 != 0)
                {
                    long nsize = (bitSize / 64 +1)* 64;
                    if (nsize < pos + 1)
                        bitSize = nsize;
                }
                long emptyWords = pos / 64 - bitSize / 64;
                if (emptyWords > 0)
                    PushEmptyWords(false, emptyWords); // Добавляем поток пустых слов в конец вектора
                int shift = 0;
                if (currentMarkerWord.DirtyWords == 0)
                {
                    currentMarkerWord.DirtyWords = 1;
                    words.Add(1L << shift);
                    size++;
                    bitSize = pos + 1;
                    return;
                }
                if (pos / 64 - (bitSize - 1) / 64 > 0)
                {
                    if (currentMarkerWord.DirtyWords != maxDirtyWords)
                    {
                        currentMarkerWord.DirtyWords = 1;
                        words.Add(1L << shift);
                        size++;
                    }
                    else
                    {
                        words.Add(0);
                        size++;
                        currentMarkerWord.MWPosition = size - 1;
                        currentMarkerWord.DirtyWords = 1;
                        words.Add(1L << shift);
                        size++;
                    }
                    bitSize = pos + 1;
                    return;
                }
                words[size - 1] |= 1L << shift;
                if (words[size - 1] == ~0L)
                {
                    words.RemoveAt(size - 1);
                    size--;
                    currentMarkerWord.DirtyWords--;
                    if (words[currentMarkerWord.MWPosition] == 0)
                        currentMarkerWord.emptyBit = true;
                    if (currentMarkerWord.DirtyWords == 0 && currentMarkerWord.emptyBit && 
                        currentMarkerWord.EmptyWords != maxEmptyWords)
                        currentMarkerWord.EmptyWords++;
                    else
                    {
                        words.Add(1L);
                        size++;
                        currentMarkerWord.MWPosition = size - 1;
                        currentMarkerWord.EmptyWords++;
                    }
                }
                bitSize = pos + 1;
            }
        }

        /// <summary>
        /// Метод, добавляющий поток пустых слов в конец битового вектора
        /// </summary>
        /// <param name="bit">Тип пустых слов(единичные или нулевые)</param>
        /// <param name="num">Число добавляемых пустых слов</param>
        void PushEmptyWords(bool bit, long num)
        {
            if (currentMarkerWord.DirtyWords == currentMarkerWord.EmptyWords && currentMarkerWord.DirtyWords == 0)
            {
                currentMarkerWord.emptyBit = bit;
            }
            if (currentMarkerWord.DirtyWords == 0 && currentMarkerWord.emptyBit == bit && 
                currentMarkerWord.EmptyWords != maxEmptyWords)
            {
                long ewords = currentMarkerWord.EmptyWords;
                long wordsToInsert = Math.Min(num, maxEmptyWords - ewords);
                num -= wordsToInsert;
                currentMarkerWord.EmptyWords += wordsToInsert;
                bitSize += wordsToInsert * 64;
            }
            else
            {
                words.Add(0);
                size++;
                currentMarkerWord.MWPosition = size - 1;
                long wordsToInsert = Math.Min(num, maxEmptyWords);
                currentMarkerWord.EmptyWords += wordsToInsert;
                num -= wordsToInsert;
                bitSize += wordsToInsert * 64;
            }
            if (num > 0)
                PushEmptyWords(bit, num);
        }

        /// <summary>
        /// Поиск первого бита (с начала), значение которого 1
        /// </summary>
        /// <returns>Номер найденного бита</returns>
        public int Find()
        {
            MarkerWord mw = new MarkerWord(ref words);
            mw.MWPosition = 0;
            int bitNum = 0;
            while (true)
            {
                if (mw.emptyBit)
                    return bitNum;
                if (mw.DirtyWords == 0)
                {
                    bitNum += (int)(mw.EmptyWords * 64);
                    mw.MWPosition++;
                }
                else
                {
                    bitNum += (int)(mw.EmptyWords * 64);
                    int wordpos = mw.MWPosition + 1;
                    int i = 0;
                    long word = words[wordpos];
                    while (word % 2 != 0 && i < 64)
                    {
                        word /= 2;
                        i++;
                    }
                    return bitNum + i;
                }
            }
        }
    }
    /// <summary>
    /// Структура, объекты которой - пары ключ/сжатый битовый вектор
    /// </summary>
    public struct Pair
    {
        /// <summary>
        /// Ключ
        /// </summary>
        public string key;

        /// <summary>
        /// Сжатый битовый вектор, где едичными являются те биты, номера которых равны номерам записей, 
        /// относящихся к ключу этой пары
        /// </summary>
        public WordArray bmp;

        /// <summary>
        /// Конструктор экземпляра структуры
        /// </summary>
        /// <param name="key">Ключ пары</param>
        /// <param name="rids">Отсортированный массив номеров записей</param>
        public Pair(string key, params int[] rids)
        {
            this.key = key;
            this.bmp = new WordArray();
            for (int i = 0; i < rids.Length; i++)
                bmp.ToTrue(rids[i]);
        }
    }
    /// <summary>
    /// Класс, представляющий индекс compressed bitmap
    /// </summary>
    public class EWAHIndex
    {
        /// <summary>
        /// Массив пар ключ/сжатый битовый вектор
        /// </summary>
        Pair[] pairs = new Pair[1000100];
        /// <summary>
        /// Количество добавленных пар
        /// </summary>
        int size;
        /// <summary>
        /// Конструктор экземпляра класса, задающий пары по входным массивам ключей и списков записей
        /// </summary>
        /// <param name="keys">Массив отсортированных ключей</param>
        /// <param name="rids">Массив списков записей, соответствующих ключам из массива ключей</param>
        /// <param name="time">Время, затраченное на построение индекса</param>
        public EWAHIndex(string[] keys, List<int>[] rids, out double time)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < keys.Length; i++)
                pairs[i] = new Pair(keys[i], rids[i].ToArray());
            size = keys.Length;
            timer.Stop();
            time = 1000 * 1000 * timer.ElapsedTicks / Stopwatch.Frequency;
        }
        /// <summary>
        /// Добавление новой записи в индекс
        /// </summary>
        /// <param name="key">Ключ записи</param>
        /// <param name="rid">Номер записи</param>
        /// <param name="time">Время, затраченное на добавление</param>
        public void Put(string key, int rid, out double time)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < size; i++)
            {
                if (pairs[i].key == key)
                {
                    pairs[i].bmp.ToTrue(rid);
                    timer.Stop();
                    time = 1000 * 1000 * timer.ElapsedTicks / Stopwatch.Frequency;
                    return;
                }
            }
            pairs[size] = new Pair(key, rid);
            size++;
            timer.Stop();
            time = 1000 * 1000 * timer.ElapsedTicks / Stopwatch.Frequency;
        }
        /// <summary>
        /// Поиск записи по ключу
        /// </summary>
        /// <param name="key">Ключ, по которому проводится поиск</param>
        /// <param name="time">Время, затраченное на поиск</param>
        /// <returns>Номер первой записи с данным ключом, если ключ найден, и  -1 в противном случае</returns>
        public int FindFirst(string key, out double time)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < size; i++)
            {
                if (pairs[i].key == key)
                {
                    int res = pairs[i].bmp.Find();
                    timer.Stop();
                    time = 1000 * 1000 * timer.ElapsedTicks / Stopwatch.Frequency;
                    return res;
                }
            }
            timer.Stop();
            time = 1000 * 1000 * timer.ElapsedTicks / Stopwatch.Frequency;
            return -1;
        }
        /// <summary>
        /// Удаление ключа из индекса
        /// </summary>
        /// <param name="key">Удаляемы ключ</param>
        /// <param name="time">Время, затраченное на удаление</param>
        public void Delete(string key, out double time)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < size; i++)
            {
                if (pairs[i].key == key)
                {
                    for (int j = i + 1; j < size; j++)
                        pairs[j - 1] = pairs[j];
                    size--;
                }
            }
            timer.Stop();
            time = 1000 * 1000 * timer.ElapsedTicks / Stopwatch.Frequency;
        }
    }
}
