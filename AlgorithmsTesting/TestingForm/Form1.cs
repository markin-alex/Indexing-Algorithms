using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using EWAHBitmap;
using BPlusLib;
using Bitmap;
using Sorting;

namespace TestingForm
{
    /// <summary>
    /// Класс главной формы проекта
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Метод, перезаписывающий файл run.dat
        /// </summary>
        void reenterRun()
        {
            StreamWriter swriter = new StreamWriter(runFileName);
            swriter.WriteLine(nextTestNum);
            for (int i = 0; i < listBox1.Items.Count; i++)
                swriter.WriteLine(listBox1.Items[i]);
            swriter.Flush();
            swriter.Close();
        }

        /// <summary>
        /// Делегат-тип, используемый для хранения последовательностей действий выбранного теста
        /// </summary>
        /// <param name="count">Номер действия, не считая построения индекса</param>
        delegate void del(ref int count);

        /// <summary>
        /// Метод для делегата test - построение индексов
        /// </summary>
        /// <param name="count">Номер действия, не считая построения индекса</param>
        public void BulkLoad(ref int count)
        {
            double time;
            textBox1.AppendText(System.Environment.NewLine + "Строим индексы:");
            textBox1.Refresh();
            tree = new BplusTree(m, sortedKeys, listOfRids, out time); // Построение нового B+ tree
            textBox1.AppendText(System.Environment.NewLine + "B+ tree построено за " + time + " микросекунд");
            textBox1.Refresh();
            bulkLoadingElapsedTime[0] = (long)time;
            compressedBmp = new EWAHIndex(sortedKeys, listOfRids, out time); // Построение нового EWAHIndex
            textBox1.AppendText(System.Environment.NewLine + "Compressed Bitmap построено за " + time + " микросекунд");
            textBox1.Refresh();
            bulkLoadingElapsedTime[1] = (long)time;
            bmp = new BitmapIndex(sortedKeys, listOfRids, out time); // Построение новго BitmapIndex
            textBox1.AppendText(System.Environment.NewLine + "Bitmap построено за " + time + " микросекунд");
            textBox1.Refresh();
            bulkLoadingElapsedTime[2] = (long)time;
            textBox1.AppendText(System.Environment.NewLine + "------------------------------------");
            textBox1.Refresh();
        }

        /// <summary>
        /// Метод для делегата test - добавление записи по ключу и номеру в индексы
        /// </summary>
        /// <param name="count">Номер действия, не считая построения индекса</param>
        public void Put(ref int count)
        {
            double time;
            textBox1.AppendText(String.Format(System.Environment.NewLine + "Добавляем запись ({0} {1}):", pairs[count].key, pairs[count].rid));
            textBox1.Refresh();
            tree.put(pairs[count].key, pairs[count].rid, out time);
            textBox1.AppendText(System.Environment.NewLine + "Ключ добавлен в B+ tree за " + time + " микросекунд");
            textBox1.Refresh();
            addingElapsedTime[0].Add((long)time);
            compressedBmp.Put(pairs[count].key, pairs[count].rid, out time);
            textBox1.AppendText(System.Environment.NewLine + "Ключ добавлен в Compressed Bitmap за " + time + " микросекунд");
            textBox1.Refresh();
            addingElapsedTime[1].Add((long)time);
            bmp.Put(pairs[count].key, pairs[count].rid, out time);
            addingElapsedTime[2].Add((long)time);
            textBox1.AppendText(System.Environment.NewLine + "Ключ добавлен в Bitmap за " + time + " микросекунд");
            textBox1.AppendText(System.Environment.NewLine + "------------------------------------");
            textBox1.Refresh();
            count++; // Переход к следующему действию
        }

        /// <summary>
        /// Метод для делегата test - удаление ключа и соответствующих ему номеров записей из индексов
        /// </summary>
        /// <param name="count">Номер действия, не считая построения индекса</param>
        public void Delete(ref int count)
        {
            double time;
            textBox1.AppendText(System.Environment.NewLine + "Удаляем ключ " + pairs[count].key + ":");
            textBox1.Refresh();
            tree.delete(pairs[count].key, out time);
            textBox1.AppendText(System.Environment.NewLine + "Ключ удалён из B+ tree за " + time + " микросекунд");
            textBox1.Refresh();
            deletionElapsedTime[0].Add((long)time);
            compressedBmp.Delete(pairs[count].key, out time);
            textBox1.AppendText(System.Environment.NewLine + "Ключ удалён из Compressed Bitmap за " + time + " микросекунд");
            textBox1.Refresh();
            deletionElapsedTime[1].Add((long)time);
            bmp.Delete(pairs[count].key, out time);
            deletionElapsedTime[2].Add((long)time);
            textBox1.AppendText(System.Environment.NewLine + "Ключ удалён из Bitmap за " + time + " микросекунд");
            textBox1.AppendText(System.Environment.NewLine + "------------------------------------");
            textBox1.Refresh();
            count++; // Переход к следующему действию
        }

        /// <summary>
        /// Метод для делегата test - поиск записи по ключу в индексах
        /// </summary>
        /// <param name="count">Номер действия, не считая построения индекса</param>
        public void Search(ref int count)
        {
            double time;
            textBox1.AppendText(System.Environment.NewLine + "Производим поиск ключа " + pairs[count].key + ":");
            textBox1.Refresh();
            tree.findFirst(pairs[count].key, out time);
            textBox1.AppendText(System.Environment.NewLine + "Ключ найден в B+ tree за " + time + " микросекунд");
            textBox1.Refresh();
            searchingElapsedTime[0].Add((long)time);
            compressedBmp.FindFirst(pairs[count].key, out time);
            textBox1.AppendText(System.Environment.NewLine + "Ключ найден в Compressed Bitmap за " + time + " микросекунд");
            textBox1.Refresh();
            searchingElapsedTime[1].Add((long)time);
            bmp.FindFirst(pairs[count].key, out time);
            searchingElapsedTime[2].Add((long)time);
            textBox1.AppendText(System.Environment.NewLine + "Ключ найден в Bitmap за " + time + " микросекунд");
            textBox1.AppendText(System.Environment.NewLine + "------------------------------------");
            textBox1.Refresh();
            count++; // Переход к следующему действию
        }

        /// <summary>
        /// Добавление нового теста в список
        /// </summary>
        /// <param name="testName"></param>
        public void AddTest(string testName)
        {
            listBox1.Items.Add(testName);
            reenterRun();
        }

        /// <summary>
        /// Изменение имени существующего теста
        /// </summary>
        /// <param name="testNum">номер изменяемого теста</param>
        /// <param name="newTestName">новое имя теста</param>
        public void RenameTest(int testNum, string newTestName)
        {
            listBox1.Items[testNum] = newTestName;
            reenterRun();
        }

        /// <summary>
        /// Увелечение номера для имени нового теста по умолчанию (testX, где X - номер)
        /// </summary>
        public void IncNum()
        {
            nextTestNum++;
            reenterRun();
        }

        /// <summary>
        /// Максимально допустимое количество записей для построения индексов
        /// </summary>
        const int MaxRecordCount = 1000000;

        /// <summary>
        /// Максимально допустимое количество действий в тесте, помимо построения
        /// </summary>
        const int maxPossibleChangings = 100;

        /// <summary>
        /// Путь к файлу run.dat, используемого при запуске программы
        /// </summary>
        string runFileName = @"run.dat";

        /// <summary>
        /// Делегат, содержащий последовательность методов, соответствующих действиям в выбранном тесте
        /// </summary>
        del test;

        /// <summary>
        /// Объект класса BplusTree
        /// </summary>
        BplusTree tree;

        /// <summary>
        /// Объект класса EWAHIndex
        /// </summary>
        EWAHIndex compressedBmp;

        /// <summary>
        /// Объект класса BitmapIndex
        /// </summary>
        BitmapIndex bmp;

        /// <summary>
        /// Отсортированный массив ключей для построения индексов
        /// </summary>
        string[] sortedKeys;

        /// <summary>
        /// Массив списков номеров записей, соответствующих ключам из массива sortedKeys
        /// </summary>
        List<int>[] listOfRids;

        /// <summary>
        /// Массив величин, обозначающих время построения индексов 
        /// по трём алгоритмам
        /// </summary>
        long[] bulkLoadingElapsedTime = new long[3];

        /// <summary>
        /// Массив списков времён добавления записей в индекс по трём алгоритмам. 
        /// Каждый список относится к одному из алгоритмов
        /// </summary>
        List<long>[] addingElapsedTime = new List<long>[3];

        /// <summary>
        /// Массив списков времён удаления ключей из индекса по трём алгоритмам. 
        /// Каждый список относится к одному из алгоритмов
        /// </summary>
        List<long>[] deletionElapsedTime = new List<long>[3];

        /// <summary>
        /// Массив списков времён поиска записей по ключу по трём алгоритмам. 
        /// Каждый список относится к одному из алгоритмов
        /// </summary>
        List<long>[] searchingElapsedTime = new List<long>[3];

        /// <summary>
        /// Максимальная стпепень вершин B+tree
        /// </summary>
        int m;

        /// <summary>
        /// Массив пар ключ/номер записи
        /// </summary>
        Sorting.Pair[] pairs;

        /// <summary>
        /// Номер для имени следующего теста по умолчанию (testX, где X - номер)
        /// </summary>
        int nextTestNum = 0;

        /// <summary>
        /// Конструктор экземпляра класса
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            if (!File.Exists(runFileName)) // Проверка наличия файла run.dat
            {
                StreamWriter swriter = new StreamWriter(runFileName); // Создание нового файла run.dat, если он не был найден
                swriter.Close();
            }
            try
            {
                // Обработка файла run.dat
                StreamReader reader = new StreamReader(runFileName); 
                string s = reader.ReadLine();
                if (s == null)
                    return;
                if (!int.TryParse(s, out nextTestNum))
                {
                    reader.Close();
                    throw new Exception();
                }
                s = reader.ReadLine();
                while (s != null)
                {
                    listBox1.Items.Add(s);
                    if (s.Length > 15)
                    {
                        reader.Close();
                        throw new Exception();
                    }
                    s = reader.ReadLine();
                }
                reader.Close();
            }
            catch
            {
                MessageBox.Show("Файл run.dat был повреждён. Вся информация о тестах утеряна.", "Ошибка!",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                StreamWriter swriter = new StreamWriter(runFileName); // Пересохдание файла run.dat, если он был повреждён
                swriter.Close();
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку добавления нового теста.
        /// Запускается новая форма добавления
        /// </summary>
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string[] tests = new string[listBox1.Items.Count];
            for (int i = 0; i < listBox1.Items.Count; i++)
                tests[i] = (string)listBox1.Items[i];
            string nextTestName = "test" + nextTestNum;
            Adding form2 = new Adding(this, nextTestName, tests); // Запуск новой формы добавления теста
            form2.Show();
        }

        /// <summary>
        /// Удаление теста из списка
        /// </summary>
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            int i = listBox1.SelectedIndex;
            if (i < 0)
                return;
            listBox1.Items.RemoveAt(i);
            reenterRun();
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку изменения выбранного теста.
        /// Открывается новая форма добавления с уже занесёнными туда данными из файла теста
        /// </summary>
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
                return;
            int testNum = listBox1.SelectedIndex;
            string testName = (string)listBox1.Items[listBox1.SelectedIndex];
            string fileName = testName + ".dat";
            int keySize, changingsCount, startCount;
            string[] keys;
            int count;
            List<string> acts = new List<string>();
            if (!File.Exists(fileName)) // Проверка наличия файла с тестом в директории
            {
                MessageBox.Show("Файл с содержимым теста не найден!", "Ошибка!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                listBox1.Items.RemoveAt(testNum); // Удаление теста из списка в случае отсутствия файла
                reenterRun();
                return;
            }
            StreamReader reader;
            try
            {
                // Обработка файла с тестом, проверка корректности данных в файле
                reader = new StreamReader(fileName);
                string s = reader.ReadLine();
                if (!int.TryParse(s, out keySize)) // Нахождение размера ключа в символах для данного теста
                {
                    reader.Close();
                    throw new Exception();
                }
                if (keySize < 5 || keySize > 20)
                {
                    reader.Close();
                    throw new Exception();
                }
                s = reader.ReadLine();
                if (!int.TryParse(s, out m)) // Нахождение максимальной степени вершин B+ tree для данного теста
                {
                    reader.Close();
                    throw new Exception();
                }
                if (m < 3 || m > 20)
                {
                    reader.Close();
                    throw new Exception();
                }
                s = reader.ReadLine();
                keys = new string[MaxRecordCount];
                count = 0;
                while (s != "" && count < MaxRecordCount) // Цикл чтения списка ключей из файла
                {
                    if (s.Length != keySize || s.Contains(' '))
                    {
                        reader.Close();
                        throw new Exception();
                    }
                    keys[count] = s;
                    s = reader.ReadLine();
                    count++;
                }
                if (s != "" || count == 0)
                {
                    reader.Close();
                    throw new Exception();
                }
                acts.Add("Построить индекс");
                startCount = count;
                changingsCount = 0;
                s = reader.ReadLine();
                while (s != null && changingsCount < maxPossibleChangings) // Цикл чтения списка действий обрабатываемого теста
                {
                    string[] strAr = s.Split();
                    if (strAr.Length != 2)
                    {
                        reader.Close();
                        throw new Exception();
                    }
                    if (strAr[1].Length != keySize || strAr[1].Contains(' '))
                    {
                        reader.Close();
                        throw new Exception();
                    }
                    switch (strAr[0])
                    {
                        case "put":
                            acts.Add(String.Format("Добавить запись ({0} {1})", strAr[1], count));
                            count++;
                            break;
                        case "delete":
                            acts.Add(String.Format("Удалить ключ {0}", strAr[1]));
                            break;
                        case "search":
                            acts.Add(String.Format("Поиск записи по ключу {0}", strAr[1]));
                            break;
                        default:
                            reader.Close();
                            throw new Exception();
                    }
                    s = reader.ReadLine();
                    changingsCount++;
                }
                if (s != null)
                {
                    reader.Close();
                    throw new Exception();
                }
                reader.Close();
            }
            catch
            {
                // Оповещение пользователя о том, что файл был каким-то образом повреждён, и удаление теста из списка
                MessageBox.Show("Файл с тестом повреждён!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                listBox1.Items.RemoveAt(testNum);
                try
                {
                    File.Delete(fileName);
                }
                catch
                {
                    MessageBox.Show("Не удалось удалить файл " + fileName + ". Попытайтесь сделать это вручную.",
                        "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                reenterRun();
                return;
            }
            string nextTestName = "test" + nextTestNum;
            string[] tests = new string[listBox1.Items.Count];
            for (int i = 0; i < listBox1.Items.Count; i++)
                tests[i] = (string)listBox1.Items[i]; // Формирование массива имён тестов
            Adding form2 = new Adding(this, nextTestName, testNum, tests, keySize, m, keys, startCount, 
                acts, changingsCount, count); // Инициализация новой формы изменения тестов
            form2.Show(); // Запуск формы
            keys = null;
            acts = null;
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку запуска выбранного теста.
        /// Обрабатывается файл с тестом, по которому начинается тестирование алгоритмов
        /// </summary>
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            test = null;
            if (listBox1.SelectedIndex == -1)
                return;
            textBox1.Clear();
            textBox1.AppendText("Идёт обработка файла с содержимым теста:");
            textBox1.Refresh();
            int testNum = listBox1.SelectedIndex;
            string testName = (string)listBox1.Items[testNum];
            string path = testName + ".dat";
            if (File.Exists(path)) // Проверка наличия файла с тестом в директории
            {
                // Обработка файла с тестом и проверка корректности данных в нём
                StreamReader reader;
                try
                {
                    reader = new StreamReader(path);
                    string s = reader.ReadLine();
                    int keySize;
                    if (!int.TryParse(s, out keySize)) // Нахождение размера ключа в символах для данного теста
                    {
                        reader.Close();
                        throw new Exception();
                    }
                    if (keySize < 5 || keySize > 20)
                    {
                        reader.Close();
                        throw new Exception();
                    }
                    s = reader.ReadLine();
                    if (!int.TryParse(s, out m)) // Нахождение максимальной степени вершин B+ tree для данного теста
                    {
                        reader.Close();
                        throw new Exception();
                    }
                    if (m < 3 || m > 20)
                    {
                        reader.Close();
                        throw new Exception();
                    }
                    s = reader.ReadLine();
                    string[] keys = new string[MaxRecordCount];
                    int count = 0;
                    while (s != "" && count < MaxRecordCount) // Цикл чтения списка ключей из файла
                    {
                        if (s.Length != keySize || s.Contains(' '))
                        {
                            reader.Close();
                            throw new Exception();
                        }
                        keys[count] = s;
                        s = reader.ReadLine();
                        count++;
                    }
                    if (s != "" || count == 0)
                    {
                        reader.Close();
                        throw new Exception();
                    }
                    Methods.SortKeys(keys, count, out sortedKeys, out listOfRids); // Сортировка ключей из файла
                    test += BulkLoad;
                    pairs = new Sorting.Pair[maxPossibleChangings];
                    int changingsCount = 0;
                    s = reader.ReadLine();
                    while (s != null && changingsCount < maxPossibleChangings) // Цикл чтения списка действий обрабатываемого теста
                    {
                        string[] strAr = s.Split();
                        if (strAr.Length != 2)
                        {
                            reader.Close();
                            throw new Exception();
                        }
                        if (strAr[1].Length != keySize || strAr[1].Contains(' '))
                        {
                            reader.Close();
                            throw new Exception();
                        }
                        switch (strAr[0]) // Определение типа действия (добавление/удаление/поиск)
                        {
                            case "put":
                                test += Put; // Добавление в делегат метода Put
                                pairs[changingsCount] = new Sorting.Pair(strAr[1], count);
                                count++;
                                break;
                            case "delete":
                                test += Delete; // Добавление в делегат метода Delete
                                pairs[changingsCount] = new Sorting.Pair(strAr[1], 0);
                                break;
                            case "search":
                                test += Search; // Добавление в делегат метода Search
                                pairs[changingsCount] = new Sorting.Pair(strAr[1], 0);
                                break;
                            default:
                                reader.Close();
                                throw new Exception();
                        }
                        s = reader.ReadLine();
                        changingsCount++;
                    }
                    if (s != null)
                    {
                        reader.Close();
                        throw new Exception();
                    }
                    reader.Close(); // Конец обработки теста
                }
                catch
                {
                    // Оповещение пользователя о том, что файл был каким-то образом повреждён, и удаление теста из списка
                    MessageBox.Show("Файл с тестом повреждён!", "Ошибка!");
                    listBox1.Items.RemoveAt(testNum);
                    try
                    {
                        File.Delete(path);
                    }
                    catch
                    {
                        MessageBox.Show("Не удалось удалить файл " + path + ". Попытайтесь сделать это вручную.",
                            "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    reenterRun();
                    textBox1.Text = "Ошибка при обработке теста!";
                    return;
                }
            }
            else
            {
                // Оповещение пользователя о том, что файл не был найден, и удаление теста из списка
                MessageBox.Show("Файл с содержимым выбранного теста не найден!", "Ошибка!");
                listBox1.Items.RemoveAt(testNum);
                reenterRun();
                textBox1.Text = "Ошибка при обработке теста!";
                return;
            }
            textBox1.Text += Environment.NewLine + "Тест успешно обработан"; 
            textBox1.Refresh();
            for (int i = 0; i < addingElapsedTime.Length; i++)
                addingElapsedTime[i] = new List<long>();
            for (int i = 0; i < deletionElapsedTime.Length; i++)
                deletionElapsedTime[i] = new List<long>();
            for (int i = 0; i < searchingElapsedTime.Length; i++)
                searchingElapsedTime[i] = new List<long>();
            int a = 0;
            test(ref a); // Запуск тестирования алгоритмов
            tree = null;
            bmp = null;
            compressedBmp = null;
            sortedKeys = null;
            listOfRids = null;
            long[] addingAverageValues = new long[3];
            long[] deletionAverageValues = new long[3];
            long[] searchingAverageValues = new long[3];
            // Подсчёт среднего времени, затраченного на добавление/удаление/поиск по всем трём алгоритмам
            for (int i = 0; i < addingAverageValues.Length; i++)
                addingAverageValues[i] = (addingElapsedTime[i].Count != 0) ? (long)addingElapsedTime[i].Average() : 0;
            for (int i = 0; i < deletionAverageValues.Length; i++)
                deletionAverageValues[i] = (deletionElapsedTime[i].Count != 0) ? (long)deletionElapsedTime[i].Average() : 0;
            for (int i = 0; i < searchingAverageValues.Length; i++)
                searchingAverageValues[i] = (searchingElapsedTime[i].Count != 0) ? (long)searchingElapsedTime[i].Average() : 0;
            Form2 form2 = new Form2(bulkLoadingElapsedTime, addingAverageValues, deletionAverageValues,
                searchingAverageValues, testName); // Инициализация новой формы результатов теста
            form2.Show(); // Запуск формы
        }
    }
}

