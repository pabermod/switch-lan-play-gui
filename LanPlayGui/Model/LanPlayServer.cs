using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LanPlayGui.Model
{
    public class LanPlayServer : ILanPlayServer
    {
        public LanPlayServer(Uri uri)
        {
            Uri = uri;
        }

        private string name;

        public string Name
        {
            get
            {
                return string.IsNullOrEmpty(name) ? Uri.Host : name;
            }
            set
            {
                if (value != name)
                {
                    name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Uri uri;

        public Uri Uri
        {
            get { return uri; }
            set
            {
                if (value != uri)
                {
                    uri = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private ServerStatus status;

        public ServerStatus Status
        {
            get { return status; }
            set
            {
                if (value != status)
                {
                    status = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string version;

        public string Version
        {
            get { return version; }
            set
            {
                if (value != version)
                {
                    version = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private long onlinePeople;

        public long Online
        {
            get { return onlinePeople; }
            set
            {
                if (value != onlinePeople)
                {
                    onlinePeople = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private long ping;

        public long Ping
        {
            get { return ping; }
            set
            {
                if (value != ping)
                {
                    ping = value;
                    NotifyPropertyChanged();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return Uri.Host;
        }
    }
}
