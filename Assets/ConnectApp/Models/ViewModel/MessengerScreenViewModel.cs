using System.Collections.Generic;
using ConnectApp.Models.Model;

namespace ConnectApp.Models.ViewModel {
    public class MessengerScreenViewModel {
        public string myUserId;
        public int currentTabBarIndex;
        public bool hasUnreadNotifications;
        public bool socketConnected;
        public Dictionary<string, string> lastMessageMap;
        public List<ChannelView> joinedChannels;
        public List<ChannelView> popularChannels;
        public List<ChannelView> publicChannels;
    }
}