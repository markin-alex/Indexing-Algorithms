using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sorting
{
    /// <summary>
    /// Класс, объекты которого предназначены для хранения пар ключ/значение и их сортировки 
    /// </summary>
    public class Pair : IComparable
    {
        /// <summary>
        /// Поле ключ
        /// </summary>
        public string key;

        /// <summary>
        /// Поле значение
        /// </summary>
        public int rid;

        /// <summary>
        /// Метод сравнения двух объектов данного класса, используемый в сортировке
        /// </summary>
        /// <param name="obj">Объект, с которым сравнивается данная пара</param>
        /// <returns>1, если пара, для которой вызван этот метод больше пары из параметров,
        /// -1, если меньше, и 0, если они равны</returns>
        public int CompareTo(object obj) // сравнение по ключу (для сортировки)
        {
            Pair pair = (Pair)obj;
            if (this.key.CompareTo(pair.key) > 0)
                return 1;
            if (this.key.CompareTo(pair.key) < 0)
                return -1;
            if (this.rid > pair.rid)
                return 1;
            if (this.rid < pair.rid)
                return -1;
            return 0;
        }

        /// <summary>
        /// Конструктор экземпляра класса
        /// </summary>
        /// <param name="key">Значения поля key</param>
        /// <param name="rid">Значение поля rid</param>
        public Pair(string key, int rid) // конструктор объекта класса
        {
            this.key = key;
            this.rid = rid;
        }
    }

    /// <summary>
    /// Класс, содержащий один статический метод сортировки
    /// </summary>
    public class Methods
    {
        /// <summary>
        /// Метод, сортирующий входящий массив, используя класс Pair
        /// </summary>
        /// <param name="keys">Массив ключей</param>
        /// <param name="size">Количество ключей в массиве</param>
        /// <param name="sortedKeys">Возвращаемый массив отсортированных ключей</param>
        /// <param name="listOfRids">Возвращаемый массив списков ноеров записей, 
        /// соответствующих ключам из возвращаемого массива ключей</param>
        public static void SortKeys(string[] keys, int size, out string[] sortedKeys, out List<int>[] listOfRids)
        {
            Pair[] kr = new Pair[size];
            int i;
            for (i = 0; i < size; i++)
                kr[i] = new Pair(keys[i], i);
            Array.Sort(kr); // Начало преобразования массива пар в массив ключей и массив списков rids
            int k = 1;
            for (i = 1; i < kr.Length; i++) // Считаем количество различных ключей в массиве пар
                if (kr[i].key != kr[i - 1].key)
                    k++;
            sortedKeys = new string[k]; // Список ключей
            listOfRids = new List<int>[k]; // Список списков Rids
            sortedKeys[0] = kr[0].key; // Добавляем первый ключ
            listOfRids[0] = new List<int>();
            listOfRids[0].Add(kr[0].rid);
            i = 1;
            k = 0;
            while (i < kr.Length) // Формирование массива ключей и массива списков Rids
            {
                while (i < kr.Length && kr[i].key == kr[i - 1].key)
                {
                    listOfRids[k].Add(kr[i].rid);
                    i++;
                }
                if (i >= kr.Length)
                    break;
                k++;
                sortedKeys[k] = kr[i].key;
                listOfRids[k] = new List<int>();
                listOfRids[k].Add(kr[i].rid);
                i++;
            } // Конец преобразования
        }
    }
}
