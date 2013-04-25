using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestingForm
{
    /// <summary>
    /// Класс формы для отрисовки графических результатов теста
    /// </summary>
    public partial class Form2 : Form
    {
        /// <summary>
        /// Метод, возвращающий максимальное число, которое будет отображено на верхнем делении диаграммы
        /// </summary>
        /// <param name="arr">Массив значений, которые должны быть графически отображены на диграмме</param>
        /// <returns>Число, которое будет отображено на верхнем делении диаграммы</returns>
        long GetMaxBar(long[] arr)
        {
            long max = arr.Max(); // Нахождение максимального элемента в массиве arr
            int length = (max.ToString()).Length;
            if (length == 1)
                return ((max <= 5) ? 5 : 10);
            char firstNum = (max.ToString())[0];
            int fnum = int.Parse(firstNum.ToString()) + 1; 
            return (long.Parse(fnum + new string('0', length - 1)));
        }

        /// <summary>
        /// Конструктор экземпляра класса
        /// </summary>
        /// <param name="bulkLoadingElapsedTime">Массив величин, обозначающих время построеня индексов 
        /// по трём алгоритмам</param>
        /// <param name="addingAverageValues">Массив величин, обозначающих среднее время добавления 
        /// записи в индекс по трём алгоритмам</param>
        /// <param name="deletionAverageValues">Массив величин, обозначающих среднее время удаления 
        /// ключа и всех номеров записей, соответствующих этому ключу, по трём алгоритмам</param>
        /// <param name="searchingAverageValues">Массив величин, обозначающих среднее время поиска 
        /// записи по ключу по трём алгоритмам</param>
        /// <param name="testName">Название теста, результаты которого необоходимо вывести</param>
        public Form2(long[] bulkLoadingElapsedTime, long[] addingAverageValues, long[] deletionAverageValues,
            long[] searchingAverageValues, string testName)
        {
            InitializeComponent();
            this.Text = testName + " - Результаты";
            this.bulkLoadingElapsedTime = bulkLoadingElapsedTime;
            this.addingAverageValues = addingAverageValues;
            this.deletionAverageValues = deletionAverageValues;
            this.searchingAverageValues = searchingAverageValues;
            maxGraphBul = GetMaxBar(bulkLoadingElapsedTime); // Значение на верхнем делении диаграммы построения индексов
            maxGraphAdd = GetMaxBar(addingAverageValues); // Значение на верхнем делении диаграммы среднего времени добавления в индексы
            maxGraphDel = GetMaxBar(deletionAverageValues); // Значение на верхнем делении диаграммы среднего времени удаления из индексов
            maxGraphSearch = GetMaxBar(searchingAverageValues); // Значение на верхнем делении диаграммы среднего времени поиска в индексах
        }

        /// <summary>
        /// Метод, отрисовывающий диаграмму
        /// </summary>
        /// <param name="max">Число на верхнем делении диаграммы</param>
        /// <param name="arr">Массив величин, которые нужно отобразить на диаграмме</param>
        /// <param name="e">Переменная для отрисовки</param>
        /// <param name="points">Координаты трёх точек, задающих каркас диаграммы</param>
        void DrawGraph(long max, long[] arr, PaintEventArgs e, Point[] points)
        {
            Pen pen = new Pen(Color.Black);
            Font numFont = new Font("Arial", 7);
            SolidBrush numBrush = new SolidBrush(Color.Black);
            e.Graphics.DrawLines(pen, points); // Изображение каркаса диаграммы
            Point point1 = new Point(points[0].X - 3, points[0].Y + 10), 
                point2 = new Point(points[0].X + 3, points[0].Y + 10);
            long currentNum = max;
            long shift = max / 5; 
            int pixelsShift = 20;
            int numStart = points[0].Y + 6;
            for (int i = 0; i < 5; i++) // Цикл изображения делений и соответствующих им значений на диаграмме
            {
                e.Graphics.DrawLine(pen, point1, point2);
                float numSize = (e.Graphics.MeasureString(currentNum.ToString(), numFont)).Width;
                PointF numPoint = new PointF(points[0].X - 5 - numSize, numStart);
                e.Graphics.DrawString(currentNum.ToString(), numFont, numBrush, numPoint);
                currentNum -= shift;
                point1.Y += pixelsShift;
                point2.Y += pixelsShift;
                numStart += pixelsShift;
            }
            Color[] colors = { Color.Blue, Color.Red, Color.Green };
            SolidBrush recBrush;
            int startX = points[1].X + 5;
            for (int i = 0; i < arr.Length; i++) // Цил изображения долей каждого из алгоритмов на диаграмме
            {
                recBrush = new SolidBrush(colors[i]);
                long heightInPixels = arr[i] * 100 / max;
                e.Graphics.FillRectangle(recBrush, startX, points[1].Y - heightInPixels, 20, heightInPixels);
                startX += 25;
            }

        }

        /// <summary>
        /// Массивы, хранящие времена построения, среднего времени добавления, удаления и поиска соответственно
        /// (По трём алгоритмам)
        /// </summary>
        long[] bulkLoadingElapsedTime, addingAverageValues, deletionAverageValues, searchingAverageValues;

        /// <summary>
        /// Значения верхних делений каждой из четырёх диаграмм
        /// </summary>
        long maxGraphBul, maxGraphAdd, maxGraphDel, maxGraphSearch;

        /// <summary>
        /// Отрисовка элемента управления pictureBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Font stringFont = new Font("Arial", 8);
            SolidBrush stringBrush = new SolidBrush(Color.Black);
            PointF pointF = new PointF(140, 460);
            e.Graphics.DrawString("* - Все значения указаны в микросекундах", stringFont, stringBrush, pointF);
            pointF.X = 30;
            pointF.Y = 20;
            e.Graphics.DrawString("Время построения индекса*", stringFont, stringBrush, pointF);
            Point[] points = { new Point(70, 50), new Point(70, 160), new Point(170, 160) };
            DrawGraph(maxGraphBul, bulkLoadingElapsedTime, e, points); // Изображение диаграммы построения индексов
            for (int i = 0; i < points.Length; i++)
                points[i].X += 200;
            pointF.X += 230;
            e.Graphics.DrawString("Среднее время добавления ключей*", stringFont, stringBrush, pointF);
            DrawGraph(maxGraphAdd, addingAverageValues, e, points); // Изображение диаграммы среднего времени добавления в индексы
            for (int i = 0; i < points.Length; i++)
            {
                points[i].X -= 200;
                points[i].Y += 200;
            }
            pointF.X -= 230;
            pointF.Y += 200;
            e.Graphics.DrawString("Среднее время удаления ключей*", stringFont, stringBrush, pointF);
            DrawGraph(maxGraphDel, deletionAverageValues, e, points); // Изображение диаграммы среднего времени удаления из индексов
            for (int i = 0; i < points.Length; i++)
                points[i].X += 200;
            pointF.X += 230;
            e.Graphics.DrawString("Среднее время поиска записи по ключу*", stringFont, stringBrush, pointF);
            DrawGraph(maxGraphSearch, searchingAverageValues, e, points); // Изображение диаграммы среднего времени поиска в индексах
            pointF.X = 70;
            pointF.Y = 380;
            SolidBrush recBrush = new SolidBrush(Color.Blue);
            e.Graphics.FillRectangle(recBrush, 50, pointF.Y + 8, 10, 3);
            e.Graphics.DrawString(" - B+ tree", stringFont, stringBrush, pointF);
            pointF.Y += 25;
            recBrush = new SolidBrush(Color.Red);
            e.Graphics.FillRectangle(recBrush, 50, pointF.Y + 8, 10, 3);
            e.Graphics.DrawString(" - copressed bitmap", stringFont, stringBrush, pointF);
            pointF.Y += 25;
            recBrush = new SolidBrush(Color.Green);
            e.Graphics.FillRectangle(recBrush, 50, pointF.Y + 8, 10, 3);
            e.Graphics.DrawString(" - bitmap", stringFont, stringBrush, pointF);
        }
    }
}
