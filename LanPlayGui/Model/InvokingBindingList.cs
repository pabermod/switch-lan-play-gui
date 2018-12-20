using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace LanPlayGui.Model
{
    public class InvokingBindingList<T> : BindingList<T>
    {
        public InvokingBindingList(IList<T> list, Control control = null) : base(list)
        {
            Control = control;
        }

        public InvokingBindingList(Control control = null)
        {
            Control = control;
        }

        public Control Control { get; set; }

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            if (Control?.InvokeRequired == true)
                Control.Invoke(new Action(() => base.OnListChanged(e)));
            else
                base.OnListChanged(e);
        }
    }
}
