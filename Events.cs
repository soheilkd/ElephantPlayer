using System;
using System.Collections.Generic;
using System.Linq;

namespace Player.Events
{
    public class InstanceEventArgs : EventArgs
    {
        private InstanceEventArgs() { }
        public InstanceEventArgs(IList<string> args) { _Args = args; }
        private IList<string> _Args { get; set; }
        public string this[int index] => Args[index];
        public int ArgsCount => _Args.Count;
        public string[] Args => _Args.ToArray();
    }

    public enum InfoType
    {
        Integer, Double, Media, Object, StringArray,
        RequestNext, RequestPrev, Handling, UserInterface,
        NewMedia, MediaRequested, EditingTag,
        InterfaceUpdate, MediaUpdate, PopupRequest,
        MediaRemoved, MediaMoved
    }

    public class InfoExchangeArgs : EventArgs
    {
        public InfoType Type { get; set; }
        public object Object { get; set; }
        public int Integer { get; set; }

        public InfoExchangeArgs() { }
        public InfoExchangeArgs(InfoType type) => Type = type;
        public InfoExchangeArgs(int integer)
        {
            Type = InfoType.Integer;
            Integer = integer;
        }
    }
}