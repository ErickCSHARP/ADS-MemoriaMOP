/////////////////////////////////////////////////////////////////////////////////////////////////////////////
///
///   ATV2 - Sistemas operacionais - Análise e Desenvolvimentos de Sistemas - UNIAVAN  - 26/02/2023
///   Elaborar um programa que manipule valores existentes na memória RAM gerados por outro aplicativo.
///   
///                                                                                  Aluno:Erick Lopes 
///   
/////////////////////////////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;


namespace Atv2_Memoria
{

    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; set; } = new();
        private DispatcherTimer _timer;
        private MemoryProcess _memProcess; // classe de apoio para manipular dados na memória de outro processo
        private Process _processInfo;  // objeto com informaçoes do processo alvo
        private long _offset = 0x31690; // descomento em bytes do endereço base do processo notepad até o seu ponteiro estático, este valor foi encontrado utilizando um aplicativo chamado CheatEngine
        private long _staticPointer, _dynamicPointer; // ponteiro estático e dinamico que aponta para o local de memória onde será lido e escritó no processo do notepad
        private string _text;  // armazena o texto do conteúdo lido na memória do notepad e também o texto que será enviado para a mesma memória
        private bool _keyDown; // Sinalizador quando alguma tecla for pressionado dentro do textbox de memória do noetpad

        public MainWindow()
        {
            InitializeComponent();

            DataContext = ViewModel;

            Loaded += MainWindow_Loaded;

            // Cria o temporizador que será usado para monitorar o processo alvo e o conteúdo da memória
            // pelo método delegado
            _timer = new DispatcherTimer();
            _timer.Tick += timer_Tick;
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // da inicio ao temporizador
            _timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            // se o obj _processInfo ainda for nulo faz uma nova busca pelo processo notepad 
            if (_processInfo == null)
            {
                // retorna um obj com as informações do processo notepad, ou nulo caso não encontre
                _processInfo = Process.GetProcessesByName("notepad").FirstOrDefault();

                if (_processInfo != null)
                {
                    try
                    {
                        // aguarda um tempo para garantir que o processo foi completamente carregado na memoria principal
                        Thread.Sleep(1000);

                        // instancia um obj MemoryProcess passando o identificador do processo "notepad" 
                        _memProcess = new MemoryProcess(_processInfo.Id);

                        // armazena o endereço base do modulo principal do processo
                        var baseAdress = _processInfo.MainModule.BaseAddress.ToInt64();

                        // armazena o endereço do ponteiro estatico
                        _staticPointer = _memProcess.ReadAsAddress(baseAdress + _offset);

                        // BaseAddress -> Endereço do modulo principal do processo onde está alocado na memória
                        // _offset -> Quantidade de bytes de deslocamento relativo ao endereço base até o ponteiro estático

                        // BaseAddress + _offset = _staticPointer;
                        // _staticPointer armazena um outro pointeiro com valor dinamico e seu valor aponta para o endereço 
                        // de inicio da caixa de texto digitado no "notepad"
                    }
                    catch (Exception) 
                    {
                        _processInfo = null;
                        _memProcess = null;
                    }
                }
            }
            else
            {
                // se o processo notepad ainda esta na memória chama o método targetProcess(), caso contrário finaliza as instancias do ProcessInfo e MemoryProcess
                if (!_processInfo.HasExited)
                {
                    // chama um método para a leitura e escrita do conteúdo do texto digitado no processo do notepad
                    targetProcessOn();
                }
                else
                {
                    _processInfo = null;
                    _memProcess = null;
                }
            }

            // atualiza a interface da janela 
            updateViewModel();
        }

        private void targetProcessOn()
        {
            // precisa recuperar o valor do ponteiro dinamica armazenado
            _dynamicPointer = _memProcess.ReadAsAddress(_staticPointer);

            // Quando nada foi digitado pelo usuário no textblock do programa a memória do texto do notepad é lida
            // Mas se algo foi digítado então o desvia para realizar o procedimento de envio para memória do notepad
            if (!_keyDown)
            {
                // Lê os bytes para o endereço apontado e retorna em formado string
                _text = _memProcess.ReadAsString(_dynamicPointer, ViewModel.BufferSize, Encoding.Unicode);
            }
            else
            {
                // Envia o texto do TextBox da UI para ser convertido em bytes e escritos no endereço de memória ao endereço apontado
                _memProcess.WriteAsString(_dynamicPointer, ViewModel.Text, Encoding.Unicode);

                _text = ViewModel.Text;
            }

            // reseta o sinalizador
            _keyDown = false;
        }

        private void updateViewModel()
        {
            var pCursor = UIText.SelectionStart;

            ViewModel.Text = _text;

            UIText.SelectionStart = pCursor;

            ViewModel.TargetOn = _memProcess != null;
            ViewModel.SetTextAddress(_dynamicPointer);

            if (_memProcess == null)
            {
                ViewModel.Text = String.Empty;
            }
        }


        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            _keyDown = true;
        }
    }
}
