using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace BPlusLib
{
    /// <summary>
    /// Класс, экземпляры которого представляют вершины B+ tree
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Родитель вершины
        /// </summary>
        public Node parent;
        /// <summary>
        /// Булева переменная, значение которой true, если вершина является листом дерева, 
        /// и false в обратном случае 
        /// </summary>
        public bool isLeaf = false;
        /// <summary>
        /// Булева переменная, значение которой true, если вершина является корнем дерева (root), 
        /// и false в обратном случае 
        /// </summary>
        public bool isRoot = false;
        /// <summary>
        /// Максимально допустимое число детей вершины
        /// </summary>
        public int maxSize;
        /// <summary>
        /// Минимально допустимое число детей вершины
        /// </summary>
        public int minSize;
        /// <summary>
        /// Количество детей у вершины в настоящий момент
        /// </summary>
        public int size;
        /// <summary>
        /// Список детей вершины, в случае, если вершина не является листом
        /// </summary>
        public List<Node> children;
        /// <summary>
        /// Список детей вершины, если вершина является листом
        /// Ребёнком является список номеров записей, относящихся к соответствующему ключу листа
        /// </summary>
        public List<List<int>> listOfRecordID;
        /// <summary>
        /// Список ключей вершины
        /// </summary>
        public List<string> keys;
        /// <summary>
        /// Конструктор экземпляра класса, который создаёт новый лист при переполнении в существующем
        /// </summary>
        /// <param name="lNode">Переполнившийся лист</param>
        /// <param name="mid">номер ключа, начиная с которого необходимо перекинуть все ключи и их списки в новый лист</param>
        public Node(Node lNode, int mid)
        {
            if(!lNode.isRoot)
                this.parent = lNode.parent;
            this.isLeaf = true;
            this.isRoot = false;
            string midkey = lNode.keys[mid];
            int rSize = lNode.size - mid;
            this.maxSize = lNode.maxSize;
            this.minSize = (lNode.maxSize + 1) / 2;
            this.keys = new List<string>();
            this.keys = lNode.keys.GetRange(mid, rSize);
            lNode.keys.RemoveRange(mid, rSize);
            this.listOfRecordID = lNode.listOfRecordID.GetRange(mid, rSize);
            lNode.listOfRecordID.RemoveRange(mid, rSize);
            lNode.size = mid;
            this.size = rSize; 
        }
        /// <summary>
        /// Конструктор экземпляра класса, создающий новый лист при построении индекса по входному массиву
        /// </summary>
        /// <param name="start">номер элемента в массиве, начиная с которого ключи и их списки должны быть перенесены в лист</param>
        /// <param name="sortedKeys">Входной массив ключей</param>
        /// <param name="listOfRids">Входной массив списков номеров записей, относящихся к соответствующим ключам</param>
        /// <param name="m">Размер листа (количество ключей/списков)</param>
        /// <param name="max">Максимально допустимый размер листа</param>
        /// <param name="isRoot">Булева переменная, определяющая, является ли лист верхушкой дерева</param>
        public Node(int start, string[] sortedKeys, List<int>[] listOfRids, int m, int max, bool isRoot)
        {
            this.isRoot = isRoot;
            maxSize = max;
            if (isRoot)
                minSize = 1;
            else
                minSize = (max + 1) / 2;
            size = m;
            isLeaf = true;
            string[] keysPart = new string[m];
            List<int>[] ridsPart = new List<int>[m];
            Array.Copy(sortedKeys, start, keysPart, 0, size);
            Array.Copy(listOfRids, start, ridsPart, 0, size);
            keys = new List<string>(keysPart);
            listOfRecordID = new List<List<int>>(ridsPart);
        }
        /// <summary>
        /// Конструктор экземпляра класса
        /// </summary>
        /// <param name="lNode">Внутренняя вершина дерева</param>
        public Node(Node lNode)
        {
            this.maxSize = lNode.maxSize;
            this.minSize = lNode.minSize;
            children = new List<Node>(maxSize + 1);
            keys = new List<string>(maxSize);
            isLeaf = false;
            isRoot = false;
        }

        /// <summary>
        /// Конструктор экземпляра класса, создающий новую верхушку дерева по двум детям
        /// </summary>
        /// <param name="lNode">Бывшая переполнивашаяся верхушка</param>
        /// <param name="rNode">Новая вершина, получившаяся при разделении старой верхушки</param>
        /// <param name="key">Ключ новой верхушки</param>
        public Node(Node lNode, Node rNode, string key) // Добавить и пересмотреть все конструкторы
        {
            if (lNode.isLeaf)
                this.maxSize = lNode.maxSize + 1;
            else
                this.maxSize = lNode.maxSize;
            this.minSize = 2;
            isRoot = true;
            isLeaf = false;
            keys = new List<string>(maxSize);
            children = new List<Node>(maxSize + 1);
            keys.Add(key);
            children.Add(lNode);
            children.Add(rNode);
            size = 2;
        }

        /// <summary>
        /// Метод, добавляющий нового ребёнка в конец данной вершинуы 
        /// Метод используется при построении индекса по входному массиву
        /// </summary>
        /// <param name="node">Добавляемая вершина</param>
        /// <param name="tree">Объект класса tree</param>
        /// <param name="key">Добавляемый ключ</param>
        /// <param name="sNode">Вершина дерева, в которую будет добавляться следующий лист</param>
        public void pushBack(Node node, BplusTree tree, string key, ref Node sNode)
        {
            if (!this.isLeaf && this.size >= this.maxSize) // Переполнение?
            {
                Node rNode = new Node(this);
                // Переброс половины ключей в новую вершину
                if (this.maxSize % 2 == 0)
                {
                    rNode.keys = this.keys.GetRange(this.maxSize / 2, this.maxSize / 2 - 1);
                    rNode.children = this.children.GetRange(this.maxSize / 2, this.maxSize / 2);
                    this.keys.RemoveRange(this.maxSize / 2, this.maxSize / 2 - 1);
                    this.children.RemoveRange(this.maxSize / 2, this.maxSize / 2);
                    this.size = this.maxSize / 2;
                }
                else
                {
                    rNode.keys = this.keys.GetRange(this.maxSize / 2 + 1, this.maxSize / 2 - 1);
                    rNode.children = this.children.GetRange(this.maxSize / 2 + 1, this.maxSize / 2);
                    this.keys.RemoveRange(this.maxSize / 2 + 1, this.maxSize / 2 - 1);
                    this.children.RemoveRange(this.maxSize / 2 + 1, this.maxSize / 2);
                    this.size = this.maxSize / 2 + 1;
                }
                string midKey = this.keys[size - 1];
                this.keys.RemoveAt(size - 1);
                if (node.isLeaf)
                    sNode = rNode;
                rNode.keys.Add(key);
                rNode.children.Add(node);
                rNode.size = this.maxSize / 2 + 1;
                rNode.parent = this.parent;
                for (int i = 0; i < rNode.children.Count; i++)
                {
                    rNode.children[i].parent = rNode;
                }
                if (isRoot) // Переполнился корень дерева?
                {
                    isRoot = false;
                    if (this.maxSize % 2 == 0)
                        this.minSize = this.maxSize / 2;
                    else
                        this.minSize = this.maxSize / 2 + 1;
                    rNode.minSize = this.minSize;
                    Node root = new Node(this, rNode, midKey); // Создание нового корня
                    this.parent = root;
                    rNode.parent = root;
                    tree.root = root;
                }
                else
                {
                    this.parent.pushBack(rNode, tree, midKey, ref sNode);
                }
            }
            else
            {
                // Добавление вершины
                if (this.isLeaf && this.isRoot)
                {
                    Node root = new Node(this, node, node.keys[0]);
                    this.parent = root;
                    node.parent = root;
                    tree.root = root;
                    sNode = root;
                    this.isRoot = false;
                    this.minSize = node.minSize;
                    return;
                }
                if (node.isLeaf)
                {
                    node.parent = this;
                }
                this.keys.Add(key);
                this.children.Add(node);
                this.size++;
            }
        }

        /// <summary>
        /// Метод, добавляющий в данную вершину нового ребёнка, образовавшуюся при разделении переполнившегося ребёнка
        /// </summary>
        /// <param name="num">Номер добавляемоо ключа в списке ключей данной вершины</param>
        /// <param name="key">Добавляемый ключ</param>
        /// <param name="node">Добавляемая вершина</param>
        /// <param name="tree">Объект класса tree</param>
        public void Add(int num, string key, Node node, BplusTree tree)
        {
            node.parent = this;
            this.keys.Insert(num, key);
            this.children.Insert(num + 1, node);
            this.size++;
            // Вершина переполнилась?
            if (this.size > this.maxSize)
            {
                int mid = this.maxSize / 2; // Номер среднего элемента
                string midKey = this.keys[mid]; // Средний ключ
                this.keys.RemoveAt(mid);
                int rSize = this.maxSize - mid;
                Node rNode = new Node(this);
                rNode.keys = this.keys.GetRange(mid, rSize - 1);
                this.keys.RemoveRange(mid, rSize - 1);
                rNode.children = this.children.GetRange(mid + 1, rSize);
                this.children.RemoveRange(mid + 1, rSize);
                this.size = mid + 1;
                rNode.size = rSize;
                for (int i = 0; i < rNode.size; i++)
                {
                    rNode.children[i].parent = rNode;
                }
                // Нужно увеличить высоту дерева?
                if (this.isRoot)
                {
                    isRoot = false;
                    if (this.maxSize % 2 == 0)
                        this.minSize = this.maxSize / 2;
                    else
                        this.minSize = this.maxSize / 2 + 1;
                    rNode.minSize = this.minSize;
                    Node root = new Node(this, rNode, midKey); // Создаём новую вершину дерева
                    this.parent = root;
                    rNode.parent = root;
                    tree.root = root;
                }
                else
                {
                    int k = this.parent.keys.BinarySearch(midKey);
                    k = ~k;
                    this.parent.Add(k, midKey, rNode, tree);
                }
            }
        }
    }

    /// <summary>
    /// Класс BplusTree, объекты которого представляют B+ деревья
    /// </summary>
    public class BplusTree
    {
        /// <summary>
        /// Корень дерева
        /// </summary>
        public Node root;

        /// <summary>
        /// Вершина дерева, рассматриваемая одним из методов объекта класса tree
        /// </summary>
        Node searchNode;

        /// <summary>
        /// Номер найденного ключа в листе. Значение задаётся при поиске
        /// </summary>
        int num;

        /// <summary>
        /// Конструктор экземпляра класса tree, используемый при построениb индекса по входному массиву записей
        /// </summary>
        /// <param name="m">Максимальная степень вершин B+ tree</param>
        /// <param name="sortedKeys">Отсортированный массив входных ключей</param>
        /// <param name="listOfRids">Входной массив список номеров записей, соответствующих ключу из массива ключей</param>
        /// <param name="time">Время, затраченное на построение дерева</param>
        public BplusTree(int m, string[] sortedKeys, List<int>[] listOfRids, out double time) 
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            int i = 0;
            // Создаём дерево(с одной вершиной)
            this.root = new Node(i, sortedKeys, listOfRids, Math.Min(m - 1, sortedKeys.Length - i), m - 1, true); 
            searchNode = root; // Начинаем добавлять ключи в дерево
            i += m - 1;
            while (i < sortedKeys.Length)
            {
                if (sortedKeys.Length - i < m / 2)
                {
                    for (int j = sortedKeys.Length - i; j < sortedKeys.Length; j++)
                        for (int k = 0; k < listOfRids[i].Count; k++)
                            put(sortedKeys[i], listOfRids[i][k], out time);
                    break;
                }
                Node newLeaf = new Node(i, sortedKeys, listOfRids, Math.Min(m - 1, sortedKeys.Length - i), m - 1, false);
                searchNode.pushBack(newLeaf, this, newLeaf.keys[0], ref searchNode);
                i += m - 1;
            }
            timer.Stop();
            time = 1000 * 1000 * timer.ElapsedTicks / Stopwatch.Frequency;
        }
         /// <summary>
         /// Поиск записи по ключу в дереве
         /// </summary>
         /// <param name="key">Искомый ключ</param>
        /// <param name="time">Время, затраченное на поиск</param>
         /// <returns>Номер первой записи с данным ключом, если ключ найден, и  -1 в противном случае</returns>
        public int findFirst(string key, out double time)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            Node a = this.root; // Вершина, в которой проводится поиск 
            if (a.isLeaf && a.size == 0)
            {
                searchNode = a;
                timer.Stop();
                time = 1000 * 1000 * timer.ElapsedTicks / Stopwatch.Frequency;
                return -1;
            }
            while (!a.isLeaf)
            {
                int j = a.keys.BinarySearch(key);
                if (j < 0)
                {
                    j = ~j;
                    a = a.children[j];
                }
                else
                    a = a.children[j + 1];
            }
            int i = a.keys.BinarySearch(key);
            num = i;
            searchNode = a;
            if (i >= 0 && i < a.size) // Ключ найден?
            {
                timer.Stop();
                time = 1000 * 1000 * timer.ElapsedTicks / Stopwatch.Frequency;
                return a.listOfRecordID[i][0];
            }
            else
            {
                timer.Stop();
                time = 1000 * 1000 * timer.ElapsedTicks / Stopwatch.Frequency;
                return -1;
            }
        }

        /// <summary>
        /// Добавление новой записи в дерево
        /// </summary>
        /// <param name="key">Добавляемы ключ</param>
        /// <param name="recordID">Номер записи</param>
        /// <param name="time">Время, затраченное на добавление</param>
        public void put(string key, int recordID, out double time)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            int k;
            if (findFirst(key, out time) != -1) // Ключ уже существует?
                searchNode.listOfRecordID[num].Add(recordID);
            else
            {
                // Добавляем ключ и запись в лист
                k = searchNode.keys.BinarySearch(key);
                k = ~k;
                searchNode.keys.Insert(k, key);
                searchNode.listOfRecordID.Insert(k, new List<int>());
                searchNode.listOfRecordID[k].Add(recordID);
                searchNode.size++;
                if (searchNode.size > searchNode.maxSize) // Переполнение?
                {
                    int mid = searchNode.size / 2;
                    Node rNode = new Node(searchNode, mid); // Перемещаем половину ключей в новый лист
                    key = rNode.keys[0];
                    if (searchNode.isRoot)
                    {
                        Node root = new Node(searchNode, rNode, key);
                        searchNode.parent = root;
                        rNode.parent = root;
                        this.root = root;
                        searchNode.isRoot = false;
                        searchNode.minSize = rNode.minSize;
                    }
                    else
                    {
                        k = searchNode.parent.keys.BinarySearch(key);
                        k = ~k;
                        searchNode.parent.Add(k, key, rNode, this); // Добавляем новый лист в родителся
                    }
                }
            }
            timer.Stop();
            time = 1000 * 1000 * timer.ElapsedTicks / Stopwatch.Frequency;
        }

        /// <summary>
        /// Удаление ключа из дерева
        /// </summary>
        /// <param name="key">Удаляемый ключ</param>
        /// <param name="time">Время, затраченное на удаление</param>
        public void delete(string key, out double time)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            // Находим ключ для удаления
            if (findFirst(key, out time) != -1)
            {
                if (searchNode.isRoot && searchNode.size == 0)
                {
                    timer.Stop();
                    time = 1000 * 1000 * timer.ElapsedTicks / Stopwatch.Frequency;
                    return;
                }
                if (searchNode.isRoot)
                {
                    searchNode.listOfRecordID.RemoveAt(num);
                    searchNode.keys.RemoveAt(num);
                    timer.Stop();
                    time = 1000 * 1000 * timer.ElapsedTicks / Stopwatch.Frequency;
                    return;
                }
                int k = searchNode.parent.keys.BinarySearch(key);
                searchNode.listOfRecordID[num].Clear();
                searchNode.listOfRecordID.RemoveAt(num);
                searchNode.keys.RemoveAt(num);
                searchNode.size--;
                if (searchNode.isRoot && searchNode.size == 0)
                {
                    timer.Stop();
                    time = 1000 * 1000 * timer.ElapsedTicks / Stopwatch.Frequency;
                    return;
                }
                // Лист заполнен меньше, чем на половину?
                if (searchNode.size < searchNode.minSize)
                {
                    if (k < 0)
                        k = ~k;
                    else
                        k += 1;
                    redistribute(k);
                }
                else
                    if (k > 0)
                        searchNode.parent.keys[k] = searchNode.keys[0];
            }
            timer.Stop();
            time = 1000 * 1000 * timer.ElapsedTicks / Stopwatch.Frequency;
        }

        /// <summary>
        /// Перебалансировка дерева в случае, когда размер вершины searchNode меньше минимально допустимого
        /// </summary>
        /// <param name="k">Номер searchNode в списке детей его родителя</param>
        public void redistribute(int k)
        {    
            int lcount = -1, rcount = -1; // Количество дополнительных ключей в левом и правом бартьях
            if (k < searchNode.parent.size - 1)
            {
                Node rNode = searchNode.parent.children[k + 1];
                rcount = rNode.size - rNode.minSize;
            }
            if (k > 0)
            {
                Node lNode = searchNode.parent.children[k - 1];
                lcount = lNode.size - lNode.minSize;
            }
            Node sib; // Выбранный брат
            int count;
            bool greater; // True, если выбранный брат справа, и False в обратном случае
            int sharedKeyNum; // Общий с братом ключ
            if (lcount > rcount) 
            {
                sib = searchNode.parent.children[k - 1];
                sharedKeyNum = k - 1; 
                count = lcount;
                greater = false;
            }
            else
            {
                sib = searchNode.parent.children[k + 1];
                sharedKeyNum = k;
                count = rcount;
                greater = true;
            }
            if (count == 0) // В братьях нет дополнительных ключей
                merge(ref sib, greater, sharedKeyNum);
            else
            {
                if (count % 2 == 0)
                    count = count / 2;
                else
                    count = count / 2 + 1;
                // Перекидываем ключи и указатели на детей
                if (greater)
                {
                    if (searchNode.isLeaf)
                    {
                        searchNode.keys.AddRange(sib.keys.GetRange(0, count));
                        sib.keys.RemoveRange(0, count);
                        searchNode.listOfRecordID.AddRange(sib.listOfRecordID.GetRange(0, count));
                        sib.listOfRecordID.RemoveRange(0, count);
                        searchNode.parent.keys[sharedKeyNum] = sib.keys[0];
                    }
                    else
                    {
                        searchNode.keys.Add(searchNode.parent.keys[sharedKeyNum]);
                        searchNode.keys.AddRange(sib.keys.GetRange(0, count - 1));
                        sib.keys.RemoveRange(0, count - 1);
                        searchNode.parent.keys[sharedKeyNum] = sib.keys[0];
                        sib.keys.RemoveAt(0);
                        searchNode.children.AddRange(sib.children.GetRange(0, count));
                        for (int i = 0; i < searchNode.children.Count; i++)
                            searchNode.children[i].parent = searchNode;
                        sib.children.RemoveRange(0, count); 
                    }
                }
                else
                {
                    int startPosition = sib.size - count;
                    if (searchNode.isLeaf)
                    {
                        searchNode.keys.InsertRange(0, sib.keys.GetRange(startPosition, count));
                        sib.keys.RemoveRange(startPosition, count);
                        searchNode.listOfRecordID.InsertRange(0, sib.listOfRecordID.GetRange(startPosition, count));
                        sib.listOfRecordID.RemoveRange(startPosition, count);
                        searchNode.parent.keys[sharedKeyNum] = searchNode.keys[0];
                    }
                    else
                    {
                        searchNode.keys.Insert(0, searchNode.parent.keys[sharedKeyNum]);
                        if (count != 1)
                        {
                            searchNode.keys.InsertRange(0, sib.keys.GetRange(startPosition, count - 1));
                            sib.keys.RemoveRange(startPosition, count - 1);
                        }
                        searchNode.parent.keys[sharedKeyNum] = sib.keys[startPosition - 1];
                        sib.keys.RemoveAt(startPosition - 1);
                        searchNode.children.AddRange(sib.children.GetRange(startPosition, count));
                        for (int i = 0; i < searchNode.children.Count; i++)
                            searchNode.children[i].parent = searchNode;
                        sib.children.RemoveRange(startPosition, count);
                    }
                }
                searchNode.size += count;
                sib.size -= count;
            }
        }

        /// <summary>
        /// Слияние вершины searchNode с соседней вершиной
        /// </summary>
        /// <param name="sib">Вершина, с которой необходимо произвести слияние</param>
        /// <param name="greater">Больше ли ключи в вершине, скоторой происходит слияние, чем в searchNode</param>
        /// <param name="sharedKeyNum">Номер общего ключа сливаемых вершин в их родителе</param>
        public void merge(ref Node sib, bool greater, int sharedKeyNum)
        {
            // Объединяем узлы
            if (greater)
            {
                if (searchNode.isLeaf)
                {
                    searchNode.keys.AddRange(sib.keys.GetRange(0, sib.size));
                    searchNode.listOfRecordID.AddRange(sib.listOfRecordID.GetRange(0, sib.size));
                }
                else
                {
                    searchNode.keys.Add(searchNode.parent.keys[sharedKeyNum]);
                    searchNode.keys.AddRange(sib.keys.GetRange(0, sib.size - 1)); // mb just sib.keys?
                    searchNode.children.AddRange(sib.children.GetRange(0, sib.size));
                    for (int i = 0; i < searchNode.size + sib.size; i++)
                        searchNode.children[i].parent = searchNode;
                }
                searchNode.size += sib.size;
            }
            else
            {
                if (searchNode.isLeaf)
                {
                    sib.keys.AddRange(searchNode.keys.GetRange(0, searchNode.size));
                    sib.listOfRecordID.AddRange(searchNode.listOfRecordID.GetRange(0, searchNode.size));
                }
                else
                {
                    sib.keys.Add(searchNode.parent.keys[sharedKeyNum]);
                    sib.keys.AddRange(searchNode.keys.GetRange(0, searchNode.size - 1));
                    sib.children.AddRange(searchNode.children.GetRange(0, searchNode.size));
                    for (int i = 0; i < searchNode.size + sib.size; i++)
                        sib.children[i].parent = sib;
                }
                sib.size += searchNode.size;
            }
            // Удаляем общий ключ из родителя
            string key = searchNode.parent.keys[sharedKeyNum];
            searchNode.parent.keys.RemoveAt(sharedKeyNum);
            searchNode.parent.children.RemoveAt(sharedKeyNum + 1);
            searchNode.parent.size--;
            searchNode = searchNode.parent;
            if (searchNode.isRoot && searchNode.size == 1)
            {
                this.root = searchNode.children[0];
                if (root.isLeaf)
                    this.root.minSize = 1;
                else
                    this.root.minSize = 2;
                this.root.parent = null;
                this.root.isRoot = true;
                return;
            }
            if (searchNode.isRoot)
            {
                return;
            }
            int k = searchNode.parent.keys.BinarySearch(key);
            if (searchNode.size < searchNode.minSize)
            {
                if (k < 0)
                    k = ~k;
                else
                    k += 1;
                redistribute(k);
            }
            else
                if (k > 0)
                    searchNode.parent.keys[k] = searchNode.keys[0];
        }
    }
}