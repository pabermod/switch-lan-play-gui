using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace LanPlayGui
{
    public partial class SyncBindingSource : BindingSource
    {
        private SynchronizationContext syncContext;
        public SyncBindingSource() : base()
        {
            syncContext = SynchronizationContext.Current;
        }
        public SyncBindingSource(IContainer container) : base(container)
        {
            syncContext = SynchronizationContext.Current;
        }
        public SyncBindingSource(object dataSource, string dataMember) : base(dataSource, dataMember)
        {
            syncContext = SynchronizationContext.Current;
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
