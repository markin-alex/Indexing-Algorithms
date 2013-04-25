using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Bitmap
{
    /// <summary>
    /// Класс, объект которого представляют битовый вектор и содержат операции над ним
    /// </summary>
    public class Bitmap
    {
        /// <summary>
        /// Битовый вектор со значениями тпа bool (true - 1, false - 0)
        /// </summary>
        bool[] map;
        /// <summary>
        /// Количество бит в векторе
        /// </summary>
        int size;
        /// <summary>
        /// Конструктор экземпляра класса
        /// </summary>
        /// <param name="size">размер вектора</param>
        public Bitmap(int size)
        {
            map = new bool[size];
            this.size = size;
        }
        /// <summary>
        /// Метод, изменяющий значение заданного в параметрах бита на единичку
        /// </summary>
        /// <param name="pos">Номер бита</param>
        public void ToTrue(int pos)
        {
            if (pos >= size)
            {
                Array.Resize(ref map, pos * 2);
                size = pos * 2;
                map[pos] = true;
            }
            else
                map[pos] = true;
        }
        /// <summary>
        /// Поиск первого единичного бита в векторе
        /// </summary>
        /// <returns></returns>
        public int Find()
        {
            int i = 0;
            while (!map[i] && i < size)
                i++;
            return i;
        }
    }
    /// <summary>
    /// Структура, объекты которой - пары ключ/битовый вектор
    /// </summary>
    public struct Pair
    {
        /// <summary>
        /// Ключ пары
        /// </summary>
        public string key;
        /// <summary>
        /// битовый вектор
        /// </summary>
        public Bitmap bmp;
        /// <summary>
        /// Конструктор экземпляра структуры Pair
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="module">Моудль, по которому берутся номера записей</param>
        /// <param name="rids">Массив номеров записей, относящихся к ключу пары</param>
        public Pair(string key, int module, params int[] rids)
        {
            this.key = key;
            this.bmp = new Bitmap(Math.Min(rids[rids.Length - 1] + 1, module)); 
            for (int i = 0; i < rids.Length; i++)
                bmp.ToTrue(rids[i]%module);
        }
    }
    /// <summary>
    /// Класс, представляющий индекс bitmap
    /// </summary>
    public class BitmapIndex
    {
        /// <summary>
        /// Максимальное число бит во всех векторах, определяющихся при построении индекса
        /// </summary>
        const int maxTotalSizeInBits = 100000000;
        /// <summary>
        /// Массив пар ключ/битовый вектор
        /// </summary>
        Pair[] pairs = new Pair[1000100];
        /// <summary>
        /// Количество пар в индексе
        /// </summary>
        int size;
        /// <summary>
        /// Модуль, по которому берутся все номера записей
        /// </summary>
        int module;
        /// <summary>
        /// Конструктор экземпляра класса, задающий пары по входным массивам ключей и списков записей
        /// </summary>
        /// <param name="keys">Массив отсортированных ключей</param>
        /// <param name="rids">Массив списков записей, соответствующих ключам из массива ключей</param>
        /// <param name="time">Время, затраченное на построение индекса</param>
        public BitmapIndex(string[] keys, List<int>[] rids, out double time)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            module = maxTotalSizeInBits / keys.Length;
            for (int i = 0; i < keys.Length; i++)
                pairs[i] = new Pair(keys[i], module, rids[i].ToArray());
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
            pairs[size] = new Pair(key, module, rid);
            size++;
            timer.Stop();
            time = 1000 * 1000 * timer.ElapsedTicks / Stopwatch.Frequency;
        }
        /// <summary>
        /// Поиск записи по ключ
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
