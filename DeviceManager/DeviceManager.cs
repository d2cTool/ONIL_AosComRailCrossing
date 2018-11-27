using AosComDevice;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ComDeviceManager
{
    public class DeviceManager : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ComWorker comWorker;
        private string lastMsg = string.Empty;

        private List<int> _lamps = new List<int>();
        private List<int> _buttons = new List<int>();

        public event EventHandler<ButtonsStateEventArgs> ChangeState;

        public DeviceManager(ComWorker comWorker)
        {
            this.comWorker = comWorker;
            comWorker.GetMessage += ComWorker_GetMessage;
        }

        private void ComWorker_GetMessage(object sender, ComMsgEventArgs e)
        {
            //logger.Debug($"DeviceManager get msg: {e.Data}");
            Parse(e.Data);
        }

        private void Parse(string msg)
        {
            if (lastMsg == msg)
            {
                return;
            }

            try
            {
                Regex pattern = new Regex(@"^b");
                if (!pattern.IsMatch(msg))
                    return;

                lastMsg = msg;

                //b17b18b47b48 l11l12l13l14l15l18l22l48
                //b0 l0 
                Regex buttons = new Regex(@"b(\d+)");
                Regex lamps = new Regex(@"l(\d+)");

                var buttonsList = buttons.Matches(msg)
                    .Cast<Match>()
                    .Select(m => int.Parse(m.Groups[1].Value))
                    .ToList();

                var lampsList = lamps.Matches(msg)
                    .Cast<Match>()
                    .Select(m => int.Parse(m.Groups[1].Value))
                    .ToList();

                GetDiff(buttonsList);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Msg: {msg}");
            }
        }

        private void GetDiff(List<int> btns)
        {
            List<int> offBtns = new List<int>(_buttons);
            List<int> onBtns = new List<int>(btns);

            Dictionary<int, byte> result = new Dictionary<int, byte>();

            foreach (var item in _buttons)
            {
                if (btns.Contains(item))
                {
                    offBtns.Remove(item);
                    onBtns.Remove(item);
                }
            }

            foreach (var item in offBtns)
            {
                result.Add(item, 0);
                //logger.Debug($"Diff btn: {item}: 0");
            }

            foreach (var item in onBtns)
            {
                result.Add(item, 1);
                //logger.Debug($"Diff btn: {item}: 1");
            }

            if (result.Count > 0)
            {
                _buttons = btns;
                //logger.Debug($"Device manager ChangeState?.Invoke()");
                ChangeState?.Invoke(this, new ButtonsStateEventArgs(result));
            }
        }

        public void Dispose()
        {
            comWorker.GetMessage -= ComWorker_GetMessage;
            _lamps.Clear();
            _buttons.Clear();
        }
    }
}
