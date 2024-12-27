using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphColoring
{
    public partial class MainWindow : Window
    {
        private int[,] adjacencyMatrix;
        private int vertexCount;
        private Point[] vertexPositions;
        private Brush[] colorPalette = { Brushes.Red, Brushes.Orange, Brushes.Gold,
            Brushes.Green, Brushes.Cyan, Brushes.Blue, Brushes.Purple,
            Brushes.HotPink, Brushes.DarkRed, Brushes.Chartreuse };

        public MainWindow()
        {
            InitializeComponent();
            GenerateGraphButton.Click += GenerateGraphButton_Click;
        }
        
        private void GenerateGraphButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(VertexCountTextBox.Text, out vertexCount) || vertexCount <= 0)
            {
                MessageBox.Show("Введите корректное число вершин!");
                return;
            }

            GenerateAdjacencyMatrix(vertexCount);
            DisplayAdjacencyMatrix();
            GenerateVertexPositions(vertexCount);
            DrawGraph();
            ColorGraph();
        }
        
        private void GenerateAdjacencyMatrix(int vertexCount)
        {
            Random rnd = new Random();
            adjacencyMatrix = new int[vertexCount, vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                for (int j = i + 1; j < vertexCount; j++)
                {
                    adjacencyMatrix[i, j] = adjacencyMatrix[j, i] = rnd.Next(0, 2);
                }
            }
        }

        private void DisplayAdjacencyMatrix()
        {
            AdjacencyMatrixGrid.Columns.Clear();
            AdjacencyMatrixGrid.Items.Clear();

            for (int i = 0; i < vertexCount; i++)
            {
                AdjacencyMatrixGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = i.ToString(),
                    Binding = new System.Windows.Data.Binding($"[{i}]"),
                    IsReadOnly = true
                });
            }

            for (int i = 0; i < vertexCount; i++)
            {
                var row = new int[vertexCount];
                for (int j = 0; j < vertexCount; j++)
                {
                    row[j] = adjacencyMatrix[i, j];
                }
                AdjacencyMatrixGrid.Items.Add(row);
            }
        }
        
        private void GenerateVertexPositions(int vertexCount)
        {
            Random rnd = new Random();
            vertexPositions = new Point[vertexCount];

            double canvasWidth = GraphCanvas.ActualWidth;
            double canvasHeight = GraphCanvas.ActualHeight;

            if (canvasWidth == 0 || canvasHeight == 0)
            {
                MessageBox.Show("Холст ещё не инициализирован. Убедитесь, что окно программы загружено.");
                return;
            }

            double radius = Math.Min(canvasWidth, canvasHeight) / 2 - 50; // Радиус окружности размещения
            Point center = new Point(canvasWidth / 2, canvasHeight / 2); // Центр холста

            for (int i = 0; i < vertexCount; i++)
            {
                // Распределяем вершины равномерно по окружности
                double angle = 2 * Math.PI * i / vertexCount;
                double x = center.X + radius * Math.Cos(angle);
                double y = center.Y + radius * Math.Sin(angle);

                vertexPositions[i] = new Point(x, y);
            }
        }
        
        private void DrawGraph()
        {
            GraphCanvas.Children.Clear();

            // Рисуем рёбра
            for (int i = 0; i < vertexCount; i++)
            {
                for (int j = i + 1; j < vertexCount; j++)
                {
                    if (adjacencyMatrix[i, j] == 1)
                    {
                        DrawEdge(vertexPositions[i], vertexPositions[j]);
                    }
                }
            }

            // Рисуем вершины
            for (int i = 0; i < vertexCount; i++)
            {
                DrawVertex(vertexPositions[i], i.ToString(), Brushes.Gray);
            }
        }
        
        private void ColorGraph()
        {
            if (adjacencyMatrix == null || vertexCount <= 0)
            {
                MessageBox.Show("Граф не инициализирован. Пожалуйста, создайте граф перед раскраской.");
                return;
            }

            int[] colors = new int[vertexCount];
            Array.Fill(colors, -1); // Инициализация массива цветов значением -1

            for (int i = 0; i < vertexCount; i++)
            {
                if (i >= colors.Length)
                {
                    MessageBox.Show("Ошибка: индекс вершины превышает размер массива цветов.");
                    return;
                }

                bool[] usedColors = new bool[colorPalette.Length];

                // Проверяем цвет соседей
                for (int j = 0; j < vertexCount; j++)
                {
                    if (adjacencyMatrix[i, j] == 1 && colors[j] != -1)
                    {
                        if (colors[j] < usedColors.Length)
                        {
                            usedColors[colors[j]] = true;
                        }
                    }
                }

                // Выбираем первый доступный цвет
                int color = 0;
                while (color < usedColors.Length && usedColors[color])
                {
                    color++;
                }

                if (color >= usedColors.Length)
                {
                    MessageBox.Show($"Ошибка: недостаточно цветов в палитре для вершины {i}.");
                    return;
                }

                colors[i] = color; // Присваиваем цвет текущей вершине
            }

            // Отрисовка вершин с их цветами
            for (int i = 0; i < vertexCount; i++)
            {
                if (i < colors.Length && colors[i] < colorPalette.Length)
                {
                    DrawVertex(vertexPositions[i], i.ToString(), colorPalette[colors[i]]);
                }
            }
        }
        
        private void DrawVertex(Point position, string label, Brush color)
        {
            Ellipse vertex = new Ellipse
            {
                Width = 30,
                Height = 30,
                Fill = color
            };

            Canvas.SetLeft(vertex, position.X - 15);
            Canvas.SetTop(vertex, position.Y - 15);
            GraphCanvas.Children.Add(vertex);

            TextBlock text = new TextBlock
            {
                Text = label,
                Foreground = Brushes.White,
                FontSize = 16
            };

            Canvas.SetLeft(text, position.X - 8);
            Canvas.SetTop(text, position.Y - 8);
            GraphCanvas.Children.Add(text);
        }
        
        private void DrawEdge(Point start, Point end)
        {
            Line edge = new Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 3
            };
            GraphCanvas.Children.Add(edge);
        }
    }
}
