using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace SyncBindingSourceLib
{
    public partial class SyncBindingSource : BindingSource
    {
        private SynchronizationContext syncContext;
        public SyncBindingSource() : base()
        {
            syncContext = SynchronizationContext.Current;
            InitializeComponent();
        }

        public SyncBindingSource(IContainer container) : base(container)
        {
            syncContext = SynchronizationContext.Current;
            this.components = container;
            InitializeComponent();
        }

        public SyncBindingSource(object dataSource, string dataMember) : base(dataSource, dataMember)
        {
            syncContext = SynchronizationContext.Current;
            InitializeComponent();
        }

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            if (syncContext != null)
                syncContext.Send(_ => base.OnListChanged(e), null);
            else
                base.OnListChanged(e);
        }
    }
}
