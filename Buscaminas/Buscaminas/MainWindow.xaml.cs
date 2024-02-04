using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Buscaminas
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Variables globales
        private int numRows = 9;
        private int numCols = 9;
        private int numRestBombs = 10;
        private int numCorrectFlags = 0;
        private int numDiscoveredCells = 0;
        private int numClicks = 0;
        private int spentSeconds = 0;
        private DispatcherTimer timer;
        
        //Método main
        public MainWindow()
        {
            InitializeComponent();

            //Método para iniciar la partida. 
            startGame();
            carita.MouseLeftButtonDown += caritaClick;

        }

        //Método que iniciar la partida.
        private void startGame()
        {
            //Actualizo todas las variables para una nueva partida.
            numRestBombs = 10;
            bombasRestantes.Text = numRestBombs.ToString();
            numCorrectFlags = 0;
            numDiscoveredCells = 0;
            numClicks = 0;
            Casilla[,] matrizTablero = new Casilla[9, 9];
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timerTick;
            tablero.Children.Clear();


            setCasillas(matrizTablero); //Genero las casillas.
            putTheBombs(matrizTablero); //Pongo las bombas
            CalculateNextBombs(matrizTablero); //Calculo ls bombas adyacentes para cada casilla que no sea una bomba
            showMatrixConsole(matrizTablero); //Muestro la matriz resultante por consola
            desBlockButtons(); //Desbloqueo los botones que se han bloqueado al final de cada partida para poder jugar una nueva. 
        }
        //Método para mostrar la matriz por consola.
        private void showMatrixConsole(Casilla[,] matrizTablero)
        {
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    if (matrizTablero[i, j].bomb)
                    {
                        Console.Write("b\t");
                    }
                    else
                    {
                        Console.Write(matrizTablero[i, j].numBombsNext.ToString() + "\t");
                    }
                }
                Console.WriteLine();
            }
        }
        //Método para desbloquear todos los botones del tablero. 
        private void desBlockButtons()
        {
            foreach (Button button in tablero.Children.OfType<Button>())
            {
                button.IsHitTestVisible = true;
            }
        }
        //Método que reinicia la partida.
        private void caritaClick(object sender, RoutedEventArgs e)
        {
            carita.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xD2, 0x00)); //Vuelvo a poner el color amarillo a la imagen de la cara.
            caritaText.Text = "XD"; //Cambio de nuevo el texto de la cara.
            stopClock(); //Detengo el temporizador
            resetClock(); //Reseteo el temporizador.
            startGame(); //Inicio una nueva partida. 
        }
        //Método para generar las casillas de la matriz matrizTablero y los botones dentro del UniformGrid del XAML.
        private void setCasillas(Casilla[,] matrizTablero)
        {
            //Recorro la matriz generando una nueva casilla y un nuevo botón por cada posición del a misma. 
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    matrizTablero[i, j] = new Casilla();

                    //Instancio row y column para que sean únicas para cada botón, de este modo no se modifican en cada vuelta de los bucles.
                    int row = i;
                    int column = j;

                    Button button = new Button();

                    button.Style = (Style)FindResource("closedCellStyle");
                    button.Content = "";

                    //Eventos de click izquierdo y derecho sobre el botón.
                    button.Click += (sender, e) => clickBomb(row, column, button, matrizTablero); //Establezco el Listener para el click izquierdo
                    button.MouseRightButtonDown += (sender, e) => setFlags(row, column, button, matrizTablero); //Establezo el Listener para el click derecho

                    Grid.SetRow(button, i); //Establezco la fila en la que se colocará el botón dentro del grid.
                    Grid.SetColumn(button, j); //Establezco la columna en la que se colocará el botón dentro del grid.

                    tablero.Children.Add(button); //Coloco el boton dentro del grid.
                }
            }
        }
        //Método que coloca las bombas en la matriz.
        private void putTheBombs(Casilla[,] matrizTablero)
        {
            int numBombas = 0;

            Random random = new Random();

            while (numBombas < numRestBombs)
            {
                int fila = random.Next(0, numRows); //Random para escoger la fila de la bomba.
                int columna = random.Next(0, numCols); //Random para escoger la columna de la bomba.

                //Si la casilla escogida no es ya una bomba se convierte en una y se resta 1 a número al número de bombas. El bucle sigue hasta que este número es 10.
                if (matrizTablero[fila, columna].bomb == false)
                {
                    matrizTablero[fila, columna].bomb = true;
                    numBombas++;
                }
            }
        }
        //Método que recorre la matriz y, por cada casilla que no sea una bomba, invoca al método counNextBombs() para contar el número de bombas adyacentes.
        private void CalculateNextBombs(Casilla[,] matrizTablero)
        {
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    if (!matrizTablero[i, j].bomb)
                    {
                        int bombsNext = countNextBombs(i, j, matrizTablero);
                        matrizTablero[i, j].numBombsNext = bombsNext;
                    }
                }
            }
        }
        /*
         * Método para contar el número de bombas adyacentes de la celda de la matriz que se llega como parámetro (row, column y matriz).
         * Retorna el número de bombas adyacentes que tiene una celda de la matriz. 
         */
        private int countNextBombs(int row, int column, Casilla[,] matrizTablero)
        {
            int numNextBombas = 0;

            //Se utilizan las funciones Math.Max y Math.Min para evitar salir de los límite sde la matriz.
            for (int i = Math.Max(0, row - 1); i <= Math.Min(8, row + 1); i++)
            {
                for (int j = Math.Max(0, column - 1); j <= Math.Min(8, column + 1); j++)
                {
                    //COndicional para evitar pasar por la misma celda que se ha recibido como parámetro. 
                    if (i != row || j != column)
                    {
                        //Si la celda es bomba se suma 1 al número de celdas. 
                        if (matrizTablero[i, j].bomb)
                        {
                            numNextBombas++;
                        }
                    }
                }
            }
            return numNextBombas;
        }
        //Método para el click izquierdo de los botones del tablero. 
        private void clickBomb(int row, int column, Button button, Casilla[,] matrizTablero)
        {
            if (button.Content != "🏴") //No se puede usar el click izquierdo si el botón es una bandera.
            {
                //Condición para iniciar el temporizador con el primer click. 
                if (numClicks == 0)
                {
                    initClock(); //Inicia el temporizador
                }
                numClicks++; //Incremento el número de clicks.
                Console.WriteLine(numClicks.ToString());

                button.IsHitTestVisible = false; //Lo primero es inutilizar el botón para que no pueda volver a ser pulsado en esta partida.
                //En caso de que la casilla corresponda con una bomba se pierde la partida. 
                if (matrizTablero[row, column].bomb)
                {
                    matrizTablero[row, column].discovered = true;
                    button.Style = (Style)FindResource("xplosionStyle");
                    button.Content = "💥"; //No incluyo este icono en el Style correspondiente porque entraba en conflicto con la bandera cuando la casilla explotada había sido bandera previamente.
                    gameOver(matrizTablero); //Método para el caso de Game Over. 
                }
                else //Si la casilla no corresponde con una bomba...
                {
                    if (matrizTablero[row, column].numBombsNext > 0)
                    {
                        /*
                         * Si el número de bombas adyacentes es mayor que 0
                         * se pone la casilla como descubierta,
                         * se pone el número de bombas adyacentes como contenido del botón,
                         * se cambia el color de fondo del botón
                         * y se incrementa el número de celdas descubiertas.
                        */
                        matrizTablero[row, column].discovered = true;
                        button.Content = matrizTablero[row, column].numBombsNext.ToString();
                        setBackButtonDiscovered(button);
                        setNumberColors(button);
                        incrementNumDiscoveredCells();
                    }
                    else if (matrizTablero[row, column].numBombsNext == 0)
                    {
                        //Si el número de bombas adyacentes es 0 se invoca al a función recursiva para despejar el teclado hasta el punto en que haya bombas adyacentes.
                        discoverVoidGrid(row, column, matrizTablero);
                    }
                }
            }
        }
        //Método para descubrir, partiendo de una celda sin bombas adyacentes, todo el tablero hasta llegar a las celdas que si que tengan alguna bomba adyacente. 
        private void discoverVoidGrid(int row, int column, Casilla[,] matrizTablero)
        {
            //Condición de retorno de recursividad. Si se cumple se detiene esa rama recursiva.
            if (row < 0 || row > numRows - 1 || column < 0 || column > numCols - 1 || matrizTablero[row, column].discovered)
            {
                return;
            }

            /*
             * Se coge el botón del tablero correspondiente a la celda de recibia como parámetro (row, column y matriz).
             * Se inutiliza el botón.
             * Se marca la casilla como descubierta.
             * Se incremente el número de casillas descubiertas.
             * Se cambia el color del botón.
            */
            Button button = tablero.Children.OfType<Button>().FirstOrDefault(b => Grid.GetRow(b) == row && Grid.GetColumn(b) == column);
            button.IsHitTestVisible = false;
            matrizTablero[row, column].discovered = true;
            incrementNumDiscoveredCells();
            setBackButtonDiscovered(button);

            /*
             * Condicional que comprueba si el número de bombas adyacentes de la celda recibida como parámetro es 0.
             * En caso afirmativo se recorren todas las celdas adyacentas y la propia celda recibida como parámetro y se envían como argumento de nuevo a ésta misma función.
            */
            if (matrizTablero[row, column].numBombsNext == 0)
            {
                for (int i = row - 1; i <= row + 1; i++)
                {
                    for (int j = column - 1; j <= column + 1; j++)
                    {
                        if (button.Content == "🏴")
                        {
                            button.Content = "";
                            numRestBombs++;
                            bombasRestantes.Text = numRestBombs.ToString();
                        }
                        discoverVoidGrid(i, j, matrizTablero);
                    }
                }
            }
            else //Si la celda recibida como parámetro tiene alguna bomba adyacente se muestra el número y se invoca a la función setNumberColors para ponerle el número el color correspondiente.
            {
                button.Content = matrizTablero[row, column].numBombsNext.ToString();
                setNumberColors(button);
            }
        }
        //Método que incrementa el número de celdas descubiertas e invoca al método que comprueba si son suficientes para ganar la partida. 
        private void incrementNumDiscoveredCells()
        {
            numDiscoveredCells++;
            checkDiscoveredCells();
        }
        //Método que comprueba si han sido descubiertas todas las celdas que no son bombas. En caso afirmativo se gana la partida. 
        private void checkDiscoveredCells()
        {
            if (numDiscoveredCells == 71)
            {
                winGame();
            }
        }
        //Método para mostrar todas las bombas del tablero. 
        private void explodeAllBombs(Casilla[,] matrizTablero)
        {
            //Bucle para recorrer todos los botones del tablero.
            foreach (Button button in tablero.Children.OfType<Button>())
            {
                int row = Grid.GetRow(button);
                int column = Grid.GetColumn(button);

                button.IsHitTestVisible = false; //Se bloquean todos los botones del tablero. 

                //Condicional para gestionar los botones correspondientes a bombas. 
                if (matrizTablero[row, column].bomb && !matrizTablero[row, column].discovered)
                {
                    button.Style = (Style)FindResource("explodeAllBombsStyle");
                    button.Content = "💣";
                }
            }
        }
        //Método para el click derecho de los botones del grid.
        private void setFlags(int row, int column, Button button, Casilla[,] matrizTablero)
        {
            if (numRestBombs > 0 && numClicks > 0) //Condición para que no se puedan poner más banderas que el número de bombas existentes en el tablero. 
            {
                //Si el botón no es una bandera, se coloca una y se resta del número restante de bombas.
                if (matrizTablero[row, column].flag == false)
                {
                    button.Content = "🏴";
                    matrizTablero[row, column].flag = true;
                    numRestBombs--;

                    //Si realmente hay una bomba tras esta bandera se suma 1 al número de banderas correctas. En caso de que ese número llegue a 10 se gana la partida. 
                    if (matrizTablero[row, column].bomb)
                    {
                        numCorrectFlags++;
                        if (numCorrectFlags == 10)
                        {
                            bombasRestantes.Text = numRestBombs.ToString();
                            winGame();
                        }
                    }
                }
                else //Si el botón ya es una bandera se quita al icono de la bandera del botón y se suma uno al número de bombas restantes.
                {
                    outFlag(row, column, button, matrizTablero);
                }
                //Finalmente se modifica el texto indicador del número de bombas restantes en el tablero.
                bombasRestantes.Text = numRestBombs.ToString();
            }
            else if (numRestBombs == 0 && button.Content == "🏴")
            {
                outFlag(row, column, button, matrizTablero);
            }
        }
        private void outFlag(int row, int column, Button button, Casilla[,] matrizTablero)
        {
            button.Content = "";
            matrizTablero[row, column].flag = false;
            numRestBombs++;
            bombasRestantes.Text = numRestBombs.ToString();
            if (matrizTablero[row, column].bomb) //En caso de la que la bandera estuviese colocada correctamente sobre una bomba se resta uno al número de banderas correctas. 
            {
                numCorrectFlags--;
            }
        }
        //Método para cambiar el color de las casillas que han sido descubiertas y, por tanto, no pueden volver a ser pulsadas. 
        private void setBackButtonDiscovered(Button button)
        {
            button.Style = (Style)FindResource("openCellStyle");
        }

        //Método para cambiar el color del contenido del botón dependiendo del número de bombas adyacentes.
        private void setNumberColors(Button button)
        {
            switch (button.Content)
            {
                case "1":
                    button.Foreground = new SolidColorBrush(Colors.Green);
                    break;
                case "2":
                    button.Foreground = new SolidColorBrush(Colors.Blue);
                    break;
                case "3":
                    button.Foreground = new SolidColorBrush(Colors.DarkRed);
                    break;
                case "4":
                    button.Foreground = new SolidColorBrush(Colors.DarkBlue);
                    break;
                case "5":
                    button.Foreground = new SolidColorBrush(Colors.DarkViolet);
                    break;
                default:
                    button.Foreground = new SolidColorBrush(Colors.DarkMagenta);
                    break;
            }
        }
        //Método para incrementar el temporizador y modificar el texto del contador de segundos de la pantalla. 
        private void timerTick(object sender, EventArgs e)
        {
            spentSeconds++;
            tiempo.Text = spentSeconds.ToString();
        }
        //Método para iniciar el temporizador.
        private void initClock()
        {
            timer.Start();
        }
        //Método para detener el temporizador.
        private void stopClock()
        {
            timer.Stop();
        }
        //Método para reiniciar el temporizador y actualizar el texto del contador de segundos de la pantalla a 0.
        private void resetClock()
        {
            spentSeconds = 0;
            tiempo.Text = spentSeconds.ToString();
        }
        //Método para el caso de ganar una partida.
        private void winGame()
        {
            stopClock(); //Detengo el temporizador.
            MessageBox.Show($"Has ganado!!!\n\nNúmero de clicks: {numClicks}\nSegundos: {spentSeconds}", "Win Game"); //Muestro una ventana emergente con el mensaje de victoria.
            foreach (Button button in tablero.Children.OfType<Button>()) //Recorro el tablero bloqueando todos los botones.
            {
                button.IsHitTestVisible = false;
            }
        }
        //Método para gestionar la partida perdida, el GameOver. 
        private void gameOver(Casilla[,] matrizTablero)
        {
            explodeAllBombs(matrizTablero);
            carita.Background = new SolidColorBrush(Color.FromRgb(170, 0, 0)); //Cambia el color del botón de la carita.
            caritaText.Text = "TT"; //Cambia el contenido de la carita.
            stopClock(); //Detiene el temporizador.
            MessageBox.Show($"Has perdido!!!\n\nNúmero de clicks: {numClicks}\nSegundos: {spentSeconds}", "Game Over"); //Muestra una ventana emergente con la info del GameOver.
        }
    }
}
