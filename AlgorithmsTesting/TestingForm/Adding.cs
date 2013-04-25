using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace TestingForm
{
    /// <summary>
    /// Форма добавления/изменения тестов
    /// </summary>
    public partial class Adding : Form
    {
        /// <summary>
        /// Метод, генерирующий список ключей для построения индексов
        /// </summary>
        /// <param name="keysCount">Количество случайно генерируемых ключей</param>
        /// <param name="recordsOnKey">Количество записей на каждый из генерируемых ключей</param>
        void generate(int keysCount, int recordsOnKey)
        {
            Random gen = new Random();
            for (int i = 0; i < keysCount * recordsOnKey; i += recordsOnKey)
            {
                string s = "";
                for (int j = 0; j < keySize; j++)
                    s += (char)gen.Next('a', 'z');
                for (int j = 0; j < recordsOnKey; j++)
                    keys[i + j] = s;
            }
            RIDsCount = keysCount * recordsOnKey; // Определение количества записей, по которым будут строиться индексы
            TotalRecordsCount = RIDsCount;
        }

        /// <summary>
        /// Максимально допустимое число записей для построения индексов
        /// </summary>
        const int MaxRecordCount = 1000000;

        /// <summary>
        /// Проверка корректности введённых данных при добавлении действия "Построить индекс"
        /// </summary>
        /// <param name="message">Сообщение об ошибке в случае, если какие-то данные некорректны</param>
        /// <returns>true, если данные введены корректно, и false в обратном случае</returns>
        bool checkBulk(out string message)
        {
            message = "";
            if (fromFile) // Проверка корректности данных, если пользователь добавляет массив ключей из файла
            {
                if (!fileOpened)
                {
                    message = "Не выбран файл для чтения.";
                    return false;
                }
                fileOpened = false;
            }
            if (genData) // Проверка корректности данных, если пользователь выбирает случайную генерацию ключей
            {
                int recordsOnKey, keysCount;
                if (!int.TryParse(textBox2.Text, out keysCount))
                {
                    message = "Число ключей должно быть целым положительным числом";
                    return false;
                }
                if (!int.TryParse(textBox1.Text, out recordsOnKey) || recordsOnKey < 1)
                {
                    message = "Число записей должно быть целым числом, большим 1.";
                    return false;
                }
                if (recordsOnKey * keysCount > MaxRecordCount || recordsOnKey * keysCount < 0)
                {
                    message = "Общее число записей на все ключи превышает максимально допустимое число записей " +
                        MaxRecordCount;
                    return false;
                }
                generate(keysCount, recordsOnKey);
            }
            return true;
        }

        /// <summary>
        /// Проверка корректности введённых данных при добавлении действия "Добавить запись"
        /// </summary>
        /// <param name="message">Сообщение об ошибке в случае, если какие-то данные некорректны</param>
        /// <param name="key">Возвращаемое значение поля "ключ"</param>
        /// <param name="RID">Возвращаемое значение поля "Номер записи"</param>
        /// <returns>true, если данные введены корректно, и false в обратном случае</returns>
        bool checkPut(out string message, out string key, out int RID)
        {
            message = "";
            key = textBox3.Text;
            RID = 0;
            if (key.Length != keySize)
            {
                message = "Длина ключа должна равняться указанной в критериях величине.";
                return false;
            }
            if (key.Contains(" "))
            {
                message = "Ключ не может содержать пробелов.";
                return false;
            }
            RID = int.Parse(textBox4.Text);
            if(!actChanged)
                TotalRecordsCount++;
            return true;
        }

        /// <summary>
        /// Проверка корректности введённых данных при добавлении действия "Удалить ключ"
        /// </summary>
        /// <param name="message">Сообщение об ошибке в случае, если какие-то данные некорректны</param>
        /// <param name="key">Возвращаемое значение поля "ключ"</param>
        /// <returns>true, если данные введены корректно, и false в обратном случае</returns>
        bool checkDelete(out string message, out string key)
        {
            message = "";
            key = textBox3.Text;
            if (key.Length != keySize)
            {
                message = "Длина ключа должна равняться указанной в критериях величине";
                return false;
            }
            if (key.Contains(" "))
            {
                message = "Ключ не может содержать пробелов";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Проверка корректности введённых данных при добавлении действия "Поиск записи по ключу"
        /// </summary>
        /// <param name="message">Сообщение об ошибке в случае, если какие-то данные некорректны</param>
        /// <param name="key">Возвращаемое значение поля "ключ"</param>
        /// <returns>true, если данные введены корректно, и false в обратном случае</returns>
        bool checkSearch(out string message, out string key)
        {
            message = "";
            key = textBox3.Text;
            if (key.Length != keySize)
            {
                message = "Длина ключа должна равняться указанной в критериях величине";
                return false;
            }
            if (key.Contains(" "))
            {
                message = "Ключ не может содержать пробелов";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Булевы переменные, обозначающие, какое действие сейчас выбрано для добавления 
        /// </summary>
        bool bulkLoading, put, delete, search;

        /// <summary>
        /// Булевы переменные, обозначающие, какое какой способ построения массива ключей выбран пользователем.
        /// Из файла и генерируемый соответственно
        /// </summary>
        bool genData, fromFile;

        /// <summary>
        /// Метод, обозначающий свойство Enabled части компонентов формы 
        /// согласно значению булевой переменной из параметров
        /// </summary>
        /// <param name="enabled">Булева переменная, задающая свойство Enabled некоторым компонентам</param>
        void EnabledComponents(bool enabled)
        {
            groupBox2.Enabled = enabled;
            button4.Enabled = enabled;
            button5.Enabled = enabled;
            listBox1.Enabled = enabled;
            button7.Enabled = enabled;
            button3.Enabled = enabled;
            textBox5.Enabled = enabled;
            numericUpDown1.Enabled = enabled;
            button8.Enabled = enabled;
        }

        /// <summary>
        /// Метод, переводящий часть компонентов формы в невидимое для пользователя состояние
        /// </summary>
        void AllToInvisible()
        {
            label2.Visible = false;
            label6.Visible = false;
            label3.Visible = false;
            label7.Visible = false;
            label8.Visible = false;
            groupBox1.Visible = false;
            textBox1.Visible = false;
            textBox2.Visible = false;
            textBox3.Visible = false;
            textBox4.Visible = false;
            comboBox1.Visible = false;
            button1.Visible = false;
            radioButton1.Visible = false;
            radioButton2.Visible = false;
        }

        /// <summary>
        /// Метод, переводящий часть компонентов формы, отвечающих за действие "Построить индекс" 
        /// в видимое для пользователя состояние
        /// </summary>
        void BulkLoadingToVisible()
        {
            label2.Visible = true;
            groupBox1.Visible = true;
            groupBox1.Text = "Выбор массива";
            radioButton1.Visible = true;
            radioButton2.Visible = true;
            bulkLoading = true;
            put = delete = search = false;
        }

        /// <summary>
        /// Максимальная степень вершин B+ tree
        /// </summary>
        int m = 3; 

        /// <summary>
        /// Размер ключа в символах
        /// </summary>
        int keySize = 5;

        /// <summary>
        /// Массив ключей, по которым будет производиться построение индексов
        /// </summary>
        string[] keys = new string[MaxRecordCount];

        /// <summary>
        /// Переменные, отвечающие за количество записей при создании списка действий в тесте
        /// </summary>
        int RIDsCount, TotalRecordsCount, currentRid;

        /// <summary>
        /// Номер для имени следующего теста по умолчанию (testX, где X - номер)
        /// </summary>
        string nextTestName;

        /// <summary>
        /// Номер данного теста в общем списке тестов 
        /// </summary>
        int currentTestNum;

        /// <summary>
        /// Массив имён добавленных тестов
        /// </summary>
        string[] tests;

        /// <summary>
        /// Количество добавленных с список действий, не считая действия "Построить индекс"
        /// </summary>
        int actsCount;

        /// <summary>
        /// Максимально допустимое количество действий в тесте, помимо построения
        /// </summary>
        const int maxActsCount = 100;

        /// <summary>
        /// Начальная форма, откуда вызвана данная форма добавления/изменения
        /// </summary>
        Form1 form1;

        /// <summary>
        /// Конструктор экземпляра класса, испоьзуемый при создании новой формы для добавления теста
        /// </summary>
        /// <param name="form1">Ссылка на начальную форму, откуда вызвана данная форма добавления теста</param>
        /// <param name="nextTestName">Номер для имени следующего теста по умолчанию (testX, где X - номер)</param>
        /// <param name="tests">Массив имён добавленных тестов</param>
        public Adding(Form1 form1, string nextTestName, string[] tests)
        {
            InitializeComponent();
            this.nextTestName = nextTestName;
            this.form1 = form1;
            this.tests = tests;
            AllToInvisible();
            BulkLoadingToVisible();
            EnabledComponents(false);
            button8.Visible = false;
            textBox5.Text = nextTestName;
        }

        /// <summary>
        /// Конструктор экземпляра класса, используемый при создании новой формы 
        /// для изменения выбранного из списка теста
        /// </summary>
        /// <param name="form1">Ссылка на начальную форму, откуда вызвана данная форма изменения теста</param>
        /// <param name="nextTestName">Номер для имени следующего теста по умолчанию (testX, где X - номер)</param>
        /// <param name="currentTestNum">Номер изменяемого теста в списке всех тестов</param>
        /// <param name="tests">Массив имён добавленных тестов</param>
        /// <param name="keySize">Размер ключа в изменяемом тесте</param>
        /// <param name="m">Максимальная степень вершин B+ tree в изменяемом тесте</param>
        /// <param name="keys">Массив кючей, по которому строятся индексы в изменяемом тесте</param>
        /// <param name="count">Количество ключей в массиве</param>
        /// <param name="acts">Список действий в изменяемом тесте</param>
        /// <param name="changingsCount">Количество действий в изменяемом тесте, помимо помтроения индексов</param>
        /// <param name="totalRIDs">Общее количество записей в индексах, добавляющихся в ходе выполнения теста</param>
        public Adding(Form1 form1, string nextTestName, int currentTestNum, string[] tests,
            int keySize, int m, string[] keys, int count, List<string> acts, int changingsCount, int totalRIDs)
        {
            InitializeComponent();
            this.form1 = form1;
            this.Text = tests[currentTestNum] + " - Действия";
            groupBox3.Enabled = false;
            button7.Enabled = true;
            button8.Visible = true;
            button3.Text = "Сохранить как новый";
            this.TotalRecordsCount = totalRIDs;
            this.currentRid = totalRIDs;
            AllToInvisible();
            OtherFeaturesToVisible();
            comboBox1.SelectedIndex = 0;
            this.keySize = keySize;
            numericUpDown2.Value = keySize;
            this.m = m;
            numericUpDown1.Value = m;
            this.RIDsCount = count;
            this.keys = keys;
            listBox1.Items.AddRange(acts.ToArray());
            this.nextTestName = nextTestName;
            this.currentTestNum = currentTestNum;
            this.tests = tests;
            this.actsCount = changingsCount;
            textBox5.Text = tests[currentTestNum];
        }

        /// <summary>
        /// Метод, переводящий часть компонентов формы, отвечающих за действия добавления/удаления/поиска 
        /// в видимое для пользователя состояние
        /// </summary>
        void OtherFeaturesToVisible()
        {
            groupBox1.Visible = true;
            groupBox1.Text = "Параметры";
            bulkLoading = false;
            comboBox1.Visible = true;
            label7.Visible = true;
            label8.Visible = true;
            textBox3.Visible = true;
            textBox4.Visible = true;
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку "Добавить тест"/"Сохранить как новый" 
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0) // Список действий пуст?
            {
                MessageBox.Show("Вы не задали никаких действий!", "Ошибка!");
                rewriteTest = false;
                return;
            }
            else
            {
                string testName = textBox5.Text.Trim();
                if (testName == "" || testName.Length > 15) // Определение корректности имени теста
                {
                    MessageBox.Show("Названием теста должна быть непустая строка с количеством символов, не превышающим 15.", 
                        "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rewriteTest = false;
                    return;
                }
                if (rewriteTest) // Случай перезаписи существующего теста
                {
                    for (int i = 0; i < tests.Length; i++) // Проверка оригинальноти нового имени
                        if (i != currentTestNum && tests[i] == testName)
                        {
                            MessageBox.Show("Выбранное имя теста уже существует, выберите другое.", "Ошибка!",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            rewriteTest = false;
                            return;
                        }
                    if (testName != tests[currentTestNum]) // Случай, когда выбрано другое имя теста
                    {
                        string oldFileName = tests[currentTestNum] + ".dat";
                        form1.RenameTest(currentTestNum, testName);
                        try
                        {
                            if (File.Exists(oldFileName)) // Удаление файла перезаписываемого теста
                                File.Delete(oldFileName);
                        }
                        catch
                        {
                            MessageBox.Show("Файл с предыдущим именем теста не может быть удалён. Попытайтесь сделать это вручную.",
                                "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < tests.Length; i++) // Проверка оригинальности имени добавляемого теста
                        if (tests[i] == testName)
                        {
                            MessageBox.Show("Выбранное имя теста уже существует, выберите другое.", "Ошибка!",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            rewriteTest = false;
                            return;
                        }
                }
                // Запись теста в файл
                string testFile = testName + ".dat";
                StreamWriter swriter = new StreamWriter(testFile);
                swriter.WriteLine(keySize);
                swriter.WriteLine(m);
                for (int i = 0; i < RIDsCount; i++)
                    swriter.WriteLine(keys[i]);
                swriter.WriteLine("");
                for (int i = 1; i < listBox1.Items.Count; i++)
                {
                    string s = (string)listBox1.Items[i];
                    string key;
                    switch (s[0])
                    {
                        case 'Д':
                            key = s.Substring(s.IndexOf('(') + 1, keySize);
                            swriter.WriteLine("put " + key);
                            break;
                        case 'У':
                            key = s.Substring(s.LastIndexOf(' ') + 1, keySize);
                            swriter.WriteLine("delete " + key);
                            break;
                        case 'П':
                            key = s.Substring(s.LastIndexOf(' ') + 1, keySize);
                            swriter.WriteLine("search " + key);
                            break;
                    }
                }
                swriter.Flush();
                swriter.Close(); // Конец записи
                if (!rewriteTest)
                    form1.AddTest(testName);
                if (testName == nextTestName)
                    form1.IncNum();
                this.Close();
                if (form1.WindowState == FormWindowState.Minimized)
                    form1.WindowState = FormWindowState.Maximized;
                form1.Focus(); // Переброс фокуса на главную форму проекта
            }
        }

        /// <summary>
        /// Обработчик события изменения выбора пользвователя в пользу пункта "Сгенерировать массив"
        /// </summary>
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            AllToInvisible();
            BulkLoadingToVisible();
            textBox1.ReadOnly = true;
            label3.Text = "Путь к файлу:";
            textBox1.Text = openedFileName;
            button1.Visible = true;
            textBox1.Visible = true;
            label3.Visible = true;
            genData = false;
            fromFile = true;
        }

        /// <summary>
        /// Обработчик события изменения выбора пользвователя в пользу пункта "Массив из файла"
        /// </summary>
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            AllToInvisible();
            BulkLoadingToVisible();
            textBox1.ReadOnly = false;
            textBox1.Text = "";
            label3.Text = "Количество записей на генерируемый ключ:";
            label3.Visible = true;
            label6.Visible = true;
            textBox1.Visible = true;
            textBox2.Visible = true;
            genData = true;
            fromFile = false;
        }

        /// <summary>
        /// Обработчик события изменения пользователем типа добавляемого действия (добавление/удаление/поиск)
        /// </summary>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            put = delete = search = false;
            if (comboBox1.SelectedIndex == 0)
            {
                put = true;
                label8.Visible = true;
                textBox4.Visible = true;
                textBox4.Text = currentRid.ToString();
            }
            else
            {
                label8.Visible = false;
                textBox4.Visible = false;
                if (comboBox1.SelectedIndex == 1)
                    delete = true;
                else
                    search = true;
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку "Добавить действие"/"Изменить действие"
        /// Метод проверяет корректность введённых данных и добавляет действие список, 
        /// или изменяет выбранное действие
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            string message; // Возможное сообщение об ошибке
            if (bulkLoading) // Случай, если добавляется действие "Построить индекс"
            {
                if (checkBulk(out message)) // Проверка корректности данных
                {
                    listBox1.Items.Insert(0, "Построить индекс"); // Добавление действия
                    if (bulkLoadingDeleted)
                    {
                        int currentCount = RIDsCount;
                        for (int i = 1; i < listBox1.Items.Count; i++)
                        {
                            string s = (string)listBox1.Items[i];
                            if (s[0] == 'Д')
                            {
                                int length = s.LastIndexOf(')') - s.LastIndexOf(' ') - 1;
                                string num = s.Substring(s.LastIndexOf(' ') + 1, length);
                                listBox1.Items[i] = s.Replace(num, currentCount.ToString());
                                currentCount++;
                            }
                        }
                        TotalRecordsCount = currentCount;
                        button4.Enabled = true;
                        button5.Enabled = true;
                        listBox1.Enabled = true;
                        bulkLoadingDeleted = false;
                    }
                    AllToInvisible();
                    OtherFeaturesToVisible();
                    currentRid = TotalRecordsCount;
                    comboBox1.SelectedIndex = 0;
                    comboBox1_SelectedIndexChanged(sender, e);
                    button2.Text = "Добавить действие"; 
                }
                else
                    MessageBox.Show(message, "Ошибка!"); // Оповещение пользователя о некорректности данных
                return;
            }
            if (put) // Случай, если добавляется действие "Добавить запись"
            {
                string key;
                int RID;
                if (checkPut(out message, out key, out RID)) // Проверка корректности данных
                {
                    // Добавление действия
                    if (actChanged) // Случай изменения действия
                    {
                        listBox1.Items[indexOfChangedAct] = String.Format("Добавить запись ({0} {1})", key, RID);
                        if (fromOther)
                        {
                            int currentCount = RID + 1;
                            for (int i = indexOfChangedAct + 1; i < listBox1.Items.Count; i++)
                            {
                                string s = (string)listBox1.Items[i];
                                if (s[0] == 'Д')
                                {
                                    int length = s.LastIndexOf(')') - s.LastIndexOf(' ') - 1;
                                    string num = s.Substring(s.LastIndexOf(' ') + 1, length);
                                    listBox1.Items[i] = s.Replace(num, currentCount.ToString());
                                    currentCount++;
                                }
                            }
                            TotalRecordsCount++;
                        }
                        actChanged = false;
                        fromOther = false;
                        fromAdding = false;
                        button4.Enabled = true;
                        button5.Enabled = true;
                        listBox1.Enabled = true;
                        button2.Text = "Добавить действие";
                        currentRid = TotalRecordsCount;
                        comboBox1.SelectedIndex = 0;
                        comboBox1_SelectedIndexChanged(sender, e);
                        return;
                    }
                    actsCount++;
                    listBox1.Items.Add(String.Format("Добавить запись ({0} {1})", key, RID));
                    textBox4.Text = TotalRecordsCount.ToString();
                    currentRid = TotalRecordsCount;
                    if (actsCount == maxActsCount)
                        button2.Enabled = false;
                }
                else
                    MessageBox.Show(message, "Ошибка!"); // Оповещение пользователя о некорректности данных
                return;
            }
            if (delete) // Случай, если добавляется действие "Удалить ключ"
            {
                string key;
                if (checkDelete(out message, out key)) // Проверка корректности данных
                {
                    // Добавление действия
                    if (actChanged) // Случай изменения действия
                    {
                        listBox1.Items[indexOfChangedAct] = String.Format("Удалить ключ {0}", key);
                        if (fromAdding)
                        {
                            for (int i = indexOfChangedAct + 1; i < listBox1.Items.Count; i++)
                            {
                                string s = (string)listBox1.Items[i];
                                if (s[0] == 'Д')
                                {
                                    int length = s.LastIndexOf(')') - s.LastIndexOf(' ') - 1;
                                    string num = s.Substring(s.LastIndexOf(' ') + 1, length);
                                    int newNum = int.Parse(num) - 1;
                                    listBox1.Items[i] = s.Replace(num, newNum.ToString());
                                }
                            }
                            TotalRecordsCount--;
                        }
                        actChanged = false;
                        fromOther = false;
                        fromAdding = false;
                        button4.Enabled = true;
                        button5.Enabled = true;
                        listBox1.Enabled = true;
                        button2.Text = "Добавить действие";
                        currentRid = TotalRecordsCount;
                        comboBox1.SelectedIndex = 1;
                        comboBox1_SelectedIndexChanged(sender, e);
                        return;
                    }
                    actsCount++;
                    listBox1.Items.Add(String.Format("Удалить ключ {0}", key));
                    currentRid = TotalRecordsCount;
                    if (actsCount == maxActsCount)
                        button2.Enabled = false;
                }
                else
                    MessageBox.Show(message, "Ошибка!"); // Оповещение пользователя о некорректности данных
                return;
            }
            if (search) // Случай, если добавляется действие "Поиск записи по ключу"
            {
                string key;
                if (checkSearch(out message, out key)) // Проверка корректности данных
                {
                    // Добавление действия
                    if (actChanged) // Случай изменения действия
                    {
                        listBox1.Items[indexOfChangedAct] = String.Format("Поиск записи по ключу {0}", key);
                        if (fromAdding)
                        {
                            for (int i = indexOfChangedAct + 1; i < listBox1.Items.Count; i++)
                            {
                                string s = (string)listBox1.Items[i];
                                if (s[0] == 'Д')
                                {
                                    int length = s.LastIndexOf(')') - s.LastIndexOf(' ') - 1;
                                    string num = s.Substring(s.LastIndexOf(' ') + 1, length);
                                    int newNum = int.Parse(num) - 1;
                                    listBox1.Items[i] = s.Replace(num, newNum.ToString());
                                }
                            }
                            TotalRecordsCount--;
                        }
                        actChanged = false;
                        fromOther = false;
                        fromAdding = false;
                        button4.Enabled = true;
                        button5.Enabled = true;
                        listBox1.Enabled = true;
                        button2.Text = "Добавить действие";
                        currentRid = TotalRecordsCount;
                        comboBox1.SelectedIndex = 2;
                        comboBox1_SelectedIndexChanged(sender, e);
                        return;
                    }
                    actsCount++;
                    listBox1.Items.Add(String.Format("Поиск записи по ключу {0}", key));
                    currentRid = TotalRecordsCount;
                    if (actsCount == maxActsCount)
                        button2.Enabled = false;
                }
                else
                    MessageBox.Show(message, "Ошибка!"); // Оповещение пользователя о некорректности данных
                return;
            }
        }

        /// <summary>
        /// Булева переменная, принимающая значения true, если был выбран файл с ключами для построения индексов,
        /// и false в обратном случае
        /// </summary>
        bool fileOpened;

        /// <summary>
        /// Путь к выбранному файлу с ключами для построения индексов.
        /// Ключи в файе должны иметь длину в символах, уазанную в форме добавления/изменения.
        /// </summary>
        string openedFileName;

        /// <summary>
        /// Обработка события нажатия на кнопку "Выбрать файл".
        /// Открывается диалоговое окно, в котором пользователь должен указать файл с ключами.
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            StreamReader reader;
            if (openFileDialog1.ShowDialog() == DialogResult.OK) // Открытие диалогового окна для выбора файла
            {
                string fName = openFileDialog1.FileName;
                try
                {
                    reader = new StreamReader(fName);
                }
                catch
                {
                    MessageBox.Show("Невозможно открыть выбранный файл", "Ошибка!");
                    return;
                }
                // Обработка файла и проверка корректности формата ключей в нём
                string s = reader.ReadLine();
                int count = 0;
                while (s != null && count < MaxRecordCount)
                {
                    if (s.Length == keySize && !s.Contains(" "))
                        keys[count] = s;
                    else
                    {
                        if (s.Length != keySize)
                            MessageBox.Show("Файл содержит ключ длины, не равной указанной", "Ошибка!");
                        else
                            MessageBox.Show("Ключ не может содержать пробелов", "Ошибка!");
                        reader.Close();
                        return;
                    }
                    count++;
                    s = reader.ReadLine();
                }
                reader.Close();
                if (s != null)
                {
                    DialogResult buttonClicked;
                    string message = "Число строк в файле превышает максимально допустимое число записей." +
                        "\n Обрезать файл?"; // Оповещение пользвователя о слишком большом количестве ключей в файле, и предложение обрезать его
                    buttonClicked =
                        MessageBox.Show(message, "Ошибка!", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (buttonClicked == DialogResult.Cancel)
                        return;
                }
                fileOpened = true;
                RIDsCount = count;
                TotalRecordsCount = RIDsCount;
                openedFileName = fName;
                textBox1.Text = openedFileName;
            }
        }

        /// <summary>
        /// Булева переменная, принимающая значение true, если было удалено действие "Построить индекс",
        /// и false, если данное действие на месте, или ещё не было дабавлено никаких действий в список 
        /// </summary>
        bool bulkLoadingDeleted = false;
        
        /// <summary>
        /// Обработчик события нажатия на кнопку "Удалить выделенное".
        /// Выбранное действие удаляется из списка, и, если было удалено действие "Построить индекс", 
        /// часть формы "замораживается" до тех пор, пока не будет добавлено данное действие  
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0) // Не выбрано никакое действие?
                return;
            if (listBox1.SelectedIndex == 0) // Выбрано действие "Построить индекс"?
            {
                listBox1.Items.RemoveAt(0);
                AllToInvisible();
                BulkLoadingToVisible();
                bulkLoadingDeleted = true;
                button4.Enabled = false;
                button5.Enabled = false;
                listBox1.Enabled = false;
                return;
            }
            int changeNum = listBox1.SelectedIndex;
            string s = (string)listBox1.Items[changeNum];
            listBox1.Items.RemoveAt(changeNum);
            if (s[0] == 'Д') // Выбрано действие "Добавить запись"?
            {
                int length = s.LastIndexOf(')') - s.LastIndexOf(' ') - 1;
                string num = s.Substring(s.LastIndexOf(' ') + 1, length);
                int currentCount = int.Parse(num);
                for (int i = changeNum; i < listBox1.Items.Count; i++)
                {
                    s = (string)listBox1.Items[i];
                    if (s[0] == 'Д')
                    {
                        length = s.LastIndexOf(')') - s.LastIndexOf(' ') - 1;
                        num = s.Substring(s.LastIndexOf(' ') + 1, length);
                        listBox1.Items[i] = s.Replace(num, currentCount.ToString());
                        currentCount++;
                    }
                }
                TotalRecordsCount = currentCount;
            }
            actsCount--;
            if (actsCount == maxActsCount - 1)
                button2.Enabled = true;
        }

        /// <summary>
        /// Булева переменная, принимающая значение true, если пользователь указал для изменения действие, 
        /// отличное от действия "Построить индекс", и false в остальных случаях
        /// </summary>
        bool actChanged = false;
        
        /// <summary>
        /// Номер изменённого действия в списке действий теста
        /// </summary>
        int indexOfChangedAct;

        /// <summary>
        /// Булева переменная, принимающая значение true, если некоторое действие было изменено с 
        /// добавления на удаление/поиск, и flase в остальных случаях
        /// </summary>
        bool fromAdding = false;

        /// <summary>
        /// Булева переменная, принимающая значение true, если некоторое действие было изменено с 
        /// удаления/поиска на добавление, и flase в остальных случаях
        /// </summary>
        bool fromOther = false;

        /// <summary>
        /// Обработчик события нажатия на кнопку "Изменить выделенное действие".
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0) // Не выбрано никакое действие?
                return;
            button4.Enabled = false;
            button5.Enabled = false;
            listBox1.Enabled = false;
            button2.Text = "Изменить действие";
            if (listBox1.SelectedIndex == 0)
            {
                listBox1.Items.RemoveAt(0);
                bulkLoadingDeleted = true;
                AllToInvisible();
                BulkLoadingToVisible();
                return;
            }
            int i = listBox1.SelectedIndex;
            string s = (string)listBox1.Items[i];
            AllToInvisible();
            OtherFeaturesToVisible();
            actChanged = true;
            indexOfChangedAct = i;
            currentRid = RIDsCount;
            for (i = 1; i < indexOfChangedAct; i++)
            {
                string nextAct = (string)listBox1.Items[i];
                if (nextAct[0] == 'Д')
                    currentRid++;
            }
            if (s[0] == 'Д')
            {
                comboBox1.SelectedIndex = 0;
                comboBox1_SelectedIndexChanged(sender, e);
                textBox3.Text = s.Substring(s.IndexOf('(') + 1, keySize);
                int length = s.LastIndexOf(')') - s.LastIndexOf(' ') - 1;
                textBox4.Text = s.Substring(s.LastIndexOf(' ') + 1, length);
                fromAdding = true;
                return;
            }
            if (s[0] == 'У')
            {
                textBox3.Text = s.Substring(s.LastIndexOf(' ') + 1, keySize);
                fromOther = true;
                comboBox1.SelectedIndex = 1;
                comboBox1_SelectedIndexChanged(sender, e);
                return;
            }
            if (s[0] == 'П')
            {
                comboBox1.SelectedIndex = 2;
                comboBox1_SelectedIndexChanged(sender, e);
                textBox3.Text = s.Substring(s.LastIndexOf(' ') + 1, keySize);
                fromOther = true;
                return;
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку "Утвердить"
        /// </summary>
        private void button6_Click(object sender, EventArgs e)
        {
            keySize = (int)numericUpDown2.Value;
            groupBox3.Enabled = false;
            button6.Enabled = false;
            EnabledComponents(true);
            AllToInvisible();
            BulkLoadingToVisible();
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку "Изменить критерий"
        /// </summary>
        private void button7_Click(object sender, EventArgs e)
        {
            DialogResult buttonClicked;
            string message = "Вы действительно хотите изменить критерии? Все добавленные действия будут удалены.";
            buttonClicked =
                MessageBox.Show(message, "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (buttonClicked == DialogResult.Yes)
            {
                fileOpened = false;
                actChanged = false;
                fromAdding = false;
                fromOther = false;
                bulkLoadingDeleted = false;
                groupBox3.Enabled = true;
                button6.Enabled = true;
                EnabledComponents(false);
                listBox1.Items.Clear();
            }
        }

        /// <summary>
        /// Булева переменная, принимающая значение true, если пользватель нажал на кнопку "Изменить тест" 
        /// в форме изменения, и false в остальных случаях
        /// </summary>
        bool rewriteTest = false;

        /// <summary>
        /// Обработчик события нажатия на кнопку "Изменить тест"
        /// </summary>
        private void button8_Click(object sender, EventArgs e)
        {
            rewriteTest = true;
            button3_Click(sender, e);
            rewriteTest = false;
        }

        /// <summary>
        /// Обработчик события изменения значения поля "Максимальная степень вершин B+ tree".
        /// Меняется значение переменной m согласно новому значению поля
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            m = (int)numericUpDown1.Value;
        }
    }
}
