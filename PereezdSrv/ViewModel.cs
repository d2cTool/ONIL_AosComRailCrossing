using PereezdSrv.Helpers;
using PereezdSrv.Helpers.Command;
using PereezdSrv.Networking;
using PereezdSrv.Networking.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace PereezdSrv
{
    public class ViewModel : INotifyPropertyChanged
    {
        private bool isConnected = false;
        private int connectionCount = 0;
        private string endPoint = "none";
        private int delayTime = 0;

        private string currentCommand = "";

        private string logTxt = "";
        private string lastMsg = "";

        private string selectedObject;
        private Method selectedMethod;
        private UI_Class selectedClass;

        private Grid ArgsGrid;
        private RichTextBox UiLogger;
        private Window UiWindow;
        private AosCommands aosCommands;

        private List<ComboBox> ArgCBoxes;

        public List<UI_Class> UI_Classes { get; set; }
        public ObservableCollection<string> UI_Objects { get; set; }
        public ObservableCollection<Method> Methods { get; set; }

        public ICommand SendBtnClicked { get; set; }
        public ICommand LogClearBtnClicked { get; set; }

        public ICommand PreviewBtnClicked { get; set; }

        private AosTcpListener aosTcpListener;

        public ViewModel()
        {
            aosCommands = Helpers.Command.CommandManager.GetCommands();

            UI_Classes = new List<UI_Class>();
            UI_Objects = new ObservableCollection<string>();
            Methods = new ObservableCollection<Method>();

            ArgCBoxes = new List<ComboBox>();

            SendBtnClicked = new SimpleCommand(obj => true,
                obj =>
                {
                    UpdateCommand();
                    if (isConnected)
                    {
                        AddToLog(GetAosCommand().ToUnicodeString(), LogMsgType.outcomming);
                        var bArray = Encoding.Unicode.GetBytes(GetAosCommand().ToUnicodeString());
                        aosTcpListener.Send(bArray);
                    }
                });

            LogClearBtnClicked = new SimpleCommand(obj => true,
                obj =>
                {
                    UiLogger.Document.Blocks.Clear();
                });

            PreviewBtnClicked = new SimpleCommand(obj => true,
                obj =>
                {
                    UpdateCommand();
                });
        }

        public void UpdateCommand()
        {
            CurrentCommand = GetAosCommand().ToUnicodeString();
        }

        public void Start(Grid grid, RichTextBox uiLogger, Window window)
        {
            UiLogger = uiLogger;
            ArgsGrid = grid;
            UiWindow = window;

            PopuleteClasses();
            PopuleteObjects();
            PopulateMethods();
            PopulateArguments();

            aosTcpListener = new AosTcpListener();
            aosTcpListener.GetConnectionEvent += AosTcpListener_GetConnectionEvent;

            aosTcpListener.StatusChangedEvent += AosTcpListener_StatusChangedEvent;
            aosTcpListener.LostConnectionEvent += AosTcpListener_LostConnectionEvent;

            aosTcpListener.Start();
        }

        private void AosTcpListener_LostConnectionEvent(object sender, EventArgs e)
        {
            IsConnected = false;
            EndPoint = "none";
            aosTcpListener.GetRequestReceivedEvent -= AosTcpListener_GetRequestReceivedEvent;

            try
            {
                UiLogger.Dispatcher.Invoke(new Action(()=>
                {
                    AddToLog("Lost connection", LogMsgType.system);
                }));
            }
            catch(Exception ex)
            {

            }
        }

        public void Stop()
        {
            aosTcpListener.Stop();
        }

        private void AosTcpListener_StatusChangedEvent(object sender, StatusChangedEventArgs e)
        {
            AddToLog("AosListener status: " + e.Status.ToString(), LogMsgType.system);
        }

        private void AosTcpListener_GetRequestReceivedEvent(object sender, AosRequestEventArgs e)
        {
            if (IsConnected)
            {
                lastMsg = e.AosRequest;

                try
                {
                    UiLogger.Dispatcher.Invoke(new Action(() =>
                    {
                        LogMsgType mType = e.AosRequest.Contains(":error:") ? LogMsgType.error : LogMsgType.incomming;
                        AddToLog($"{e.AosRequest}", mType);
                    }));
                }
                catch(Exception ex)
                {

                }
            }
        }

        private void AosTcpListener_GetConnectionEvent(object sender, AosConnectionArgs e)
        {
            IsConnected = true;
            aosTcpListener.GetRequestReceivedEvent += AosTcpListener_GetRequestReceivedEvent;
            try
            {
                UiLogger.Dispatcher.Invoke(new Action(() =>
                {
                    AddToLog($"Get connection: {e.ClientEndPoint}", LogMsgType.system);
                }));
            }
            catch(Exception ex)
            {

            }

            EndPoint = e.ClientEndPoint.ToString();
        }

        private void PopuleteClasses()
        {
            UI_Classes.Clear();
            foreach (var item in aosCommands.UI_Classes)
            {
                UI_Classes.Add(item);
            }
            SelectedClass = UI_Classes[0];
        }

        private void PopuleteObjects()
        {
            UI_Objects.Clear();
            foreach (var item in selectedClass.UI_Objects)
            {
                UI_Objects.Add(item);
            }
            SelectedObject = UI_Objects[0];
        }

        private void PopulateMethods()
        {
            Methods.Clear();
            foreach (var item in selectedClass.Methods)
            {
                Methods.Add(item);
            }
            SelectedMethod = Methods[0];
        }

        private void PopulateArguments()
        {
            if (ArgsGrid != null && selectedMethod != null)
            {
                ArgsGrid.Children.Clear();
                ArgCBoxes.Clear();

                if (selectedMethod.Arguments != null)
                {
                    for (int i = 0; i < selectedMethod.Arguments.Length; i++)
                    {
                        CreateWPFComboBox(selectedMethod.Arguments[i], i, ArgCBoxes);
                    }
                }
            }
        }

        private void AddToLog(string msg, LogMsgType msgType)
        {
            if (UiLogger != null)
            {
                //LogTxt += $"{DateTime.Now.ToString("HH:mm:ss")}   {msg}\r\n";
                TextRange rangeOfText1 = new TextRange(UiLogger.Document.ContentEnd, UiLogger.Document.ContentEnd);
                rangeOfText1.Text = DateTime.Now.ToString("HH:mm:ss") + "  ";
                rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkBlue);
                rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                TextRange rangeOfWord = new TextRange(UiLogger.Document.ContentEnd, UiLogger.Document.ContentEnd);
                rangeOfWord.Text = msg + "\r";

                switch (msgType)
                {
                    case LogMsgType.error:
                        rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                        rangeOfWord.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);
                        break;
                    case LogMsgType.incomming:
                        rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGreen);
                        rangeOfWord.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);
                        break;
                    case LogMsgType.outcomming:
                        rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGray);
                        rangeOfWord.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);
                        break;
                    case LogMsgType.system:
                        rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkMagenta);
                        rangeOfWord.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);
                        break;
                    default:
                        rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                        rangeOfWord.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);
                        break;
                }

                UiLogger.ScrollToEnd();
            }
        }

        private AosCommand GetAosCommand()
        {
            string obj = selectedObject;
            string cls = selectedClass.Name;
            string mthd = selectedMethod.Name;
            int delay = delayTime;
            string args = string.Empty;

            if (selectedMethod.Arguments != null)
            {
                for (int i = 0; i < selectedMethod.Arguments.Length; i++)
                {
                    args += (ArgCBoxes[i].SelectedItem as ComboBoxItem).Content;
                    if (i < selectedMethod.Arguments.Length - 1)
                        args += ",";
                }
            }

            return new AosCommand() { ObjName = obj, Class = cls, Delay = delay, Method = mthd, Arguments = args };
        }

        private void CreateWPFComboBox(Argument arg, int index, List<ComboBox> argCBoxes)
        {
            Label label = new Label();
            label.Content = arg.Name;
            label.Margin = new Thickness(25, 0, 0, 0);

            ComboBox cbox = new ComboBox();
            cbox.ToolTip = arg.Description;
            cbox.Margin = new Thickness(55, 5, 5, 5);

            foreach (var item in arg.Values)
            {
                ComboBoxItem cboxitem = new ComboBoxItem();
                cboxitem.Content = item;
                cbox.Items.Add(cboxitem);
            }

            cbox.SelectedItem = cbox.Items[0];

            ArgsGrid.Children.Add(label);
            ArgsGrid.Children.Add(cbox);

            Grid.SetRow(label, index);
            Grid.SetColumn(label, 0);

            Grid.SetRow(cbox, index);
            Grid.SetColumn(cbox, 1);

            argCBoxes.Add(cbox);
        }

        public bool IsConnected
        {
            get => isConnected;
            set
            {
                if (value != isConnected)
                {
                    isConnected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsConnected"));
                }
            }
        }

        public int ConnectionCount
        {
            get => connectionCount;
            set
            {
                if (value != connectionCount)
                {
                    connectionCount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConnectionCount"));
                }
            }
        }

        public int DelayTime
        {
            get => delayTime;
            set
            {
                if (value != delayTime)
                {
                    delayTime = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DelayTime"));
                }
            }
        }

        public string EndPoint
        {
            get => endPoint;
            set
            {
                if (value != endPoint)
                {
                    endPoint = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EndPoint"));
                }
            }
        }

        public string LogTxt
        {
            get => logTxt;
            set
            {
                if (value != logTxt)
                {
                    logTxt = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LogTxt"));
                }
            }
        }

        public string CurrentCommand
        {
            get => currentCommand;
            set
            {
                if (value != currentCommand)
                {
                    currentCommand = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentCommand"));
                }
            }
        }

        public UI_Class SelectedClass
        {
            get => selectedClass;
            set
            {
                selectedClass = value;
                PopuleteObjects();
                PopulateMethods();
                PopulateArguments();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedClass"));
            }
        }

        public string SelectedObject
        {
            get => selectedObject;
            set
            {
                selectedObject = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedObject"));
            }
        }

        public Method SelectedMethod
        {
            get => selectedMethod;
            set
            {
                selectedMethod = value;
                PopulateArguments();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedMethod"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
