using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HookThemKeys
{
    public partial class Mainform : Form
    {
        public Mainform()
        {
            InitializeComponent();
        }
        
        private SettingEntries _settings = new SettingEntries();
        private readonly ProcessWatcher _watcher = new ProcessWatcher();
        private Process _lastKnownSc2;

        private void FillWithKeys(ComboBox bx)
        {
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                bx.Items.Add(key.ToString());
            }
        }

        private void SimulateToSc2(Keys key)
        {
            NativeMethods.SendMessage(_lastKnownSc2.MainWindowHandle, (int)NativeMethods.WMessages.Keydown, (IntPtr)key, (IntPtr)0x000C0001);
            NativeMethods.SendMessage(_lastKnownSc2.MainWindowHandle, (int)NativeMethods.WMessages.Keyup, (IntPtr)key, (IntPtr)0x000C0001);
        }

        private void MouseButtonOnMouseButtonInputChanged(object sender, MouseInputArgs e)
        {
            if (_lastKnownSc2 == null)
                return;

            if (_lastKnownSc2.HasExited)
                return;

            if (NativeMethods.GetForegroundWindow() != _lastKnownSc2.MainWindowHandle)
                return;

            var mapping = _settings.Mappings.Find((x) => x.Source == e.Key);
            if (mapping == null)
                return;

            SimulateToSc2(mapping.Target);
            e.Suppress = mapping.Hook;
        }
        
        private void Mainform_Load(object sender, EventArgs e)
        {
            _settings = Preferences.Load();

            cbSource.Items.Add(Keys.XButton1.ToString());
            cbSource.Items.Add(Keys.XButton2.ToString());
            FillWithKeys(cbTarget);
            RefreshListView(-1);

            Mouse.MouseButtonInputChanged += MouseButtonOnMouseButtonInputChanged;
            Mouse.HookMouse();
            _watcher.ProcessStarted += WatcherOnProcessStarted;
            _watcher.ProcessFinished += WatcherOnProcessFinished;
            _watcher.Hook();
        }

        private void WatcherOnProcessFinished(object sender, ProcessArgs e)
        {
            if (Constants.StarCraftProcesses.Contains(e.Process.ProcessName))
            {
                _lastKnownSc2 = null;
            }
        }

        private void WatcherOnProcessStarted(object sender, ProcessArgs e)
        {
            if (Constants.StarCraftProcesses.Contains(e.Process.ProcessName))
            {
                _lastKnownSc2 = e.Process;
            }
        }

        private void Mainform_FormClosing(object sender, FormClosingEventArgs e)
        {
            Preferences.Write(_settings);
            Mouse.UnhookMouse();
            _watcher.Unhook();
        }

        private void txtCheckKey_KeyDown(object sender, KeyEventArgs e)
        {
            var origin = sender as TextBox;

            if (origin == null)
                return;

            origin.Text = e.KeyCode.ToString();
            e.SuppressKeyPress = true;
        }

        private void txtCheckKey_MouseDown(object sender, MouseEventArgs e)
        {
            var origin = sender as TextBox;

            if (origin == null)
                return;

            origin.Text = e.Button.ToString();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (cbTarget.SelectedIndex < 0 || cbSource.SelectedIndex < 0)
                return;
            
            Keys sourceKey = (Keys)Enum.Parse(typeof(Keys), cbSource.SelectedItem.ToString());
            Keys targetKey = (Keys)Enum.Parse(typeof(Keys), cbTarget.SelectedItem.ToString());

            _settings.Mappings.Add(new Mapping(sourceKey, targetKey));

            RefreshListView(-1);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var lastIdx = RemoveSelectionFromList();
            RefreshListView(lastIdx);
        }

        private int RemoveSelectionFromList()
        {
            var items = lstBindings.SelectedItems;

            if (items.Count <= 0)
                return -1;

            var selectedItem = lstBindings.SelectedItems[0];
            var subItems = selectedItem.SubItems;
            if (subItems.Count != 2)
                return -1;

            Keys sourceKey = (Keys)Enum.Parse(typeof(Keys), subItems[0].Text);
            Keys targetKey = (Keys)Enum.Parse(typeof(Keys), subItems[1].Text);

            var mappingIdx = _settings.Mappings.FindIndex((x) => { return x.Source == sourceKey && x.Target == targetKey; });
            _settings.Mappings.RemoveAt(mappingIdx);
            return mappingIdx;
        }

        private void RefreshListView(int lastIdx)
        {
            lstBindings.Items.Clear();
            foreach (var mapping in _settings.Mappings)
            {
                var items = new[] {mapping.Source.ToString(), mapping.Target.ToString()};
                lstBindings.Items.Add(new ListViewItem(items));
            }

            if (lstBindings.Items.Count <= 0)
                return;

            if (lastIdx == -1)
                return;

            if (lstBindings.Items.Count <= lastIdx)
                lstBindings.Items[lstBindings.Items.Count - 1].Selected = true;
             else
                lstBindings.Items[lastIdx].Selected = true;
        }

        private void lstBindings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete)
                return;

            var lastIdx = RemoveSelectionFromList();
            RefreshListView(lastIdx);
        }
    }
}
