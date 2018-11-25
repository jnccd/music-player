using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicPlayer
{
    public static class DiscordRPCWrapper
    {
        static DiscordRpc.RichPresence presence;
        static DiscordRpc.EventHandlers handlers;

        public static void Initialize(string clientId)
        {
            handlers = new DiscordRpc.EventHandlers();

            handlers.readyCallback = ReadyCallback;
            handlers.disconnectedCallback += DisconnectedCallback;
            handlers.errorCallback += ErrorCallback;

            DiscordRpc.Initialize(clientId, ref handlers, true, null);
        }

        public static void UpdatePresence(string state)
        {
            presence.details = "";
            presence.state = state;

            presence.startTimestamp = 0;
            presence.endTimestamp = 0;

            presence.largeImageKey = "";
            presence.largeImageText = "";
            presence.smallImageKey = "";
            presence.smallImageText = "";

            DiscordRpc.UpdatePresence(ref presence);
        }
        public static void UpdatePresence(string details, string state, long startTimestamp, long endTimestamp)
        {
            presence.details = details;
            presence.state = state;

            presence.startTimestamp = startTimestamp;
            presence.endTimestamp = endTimestamp;

            presence.largeImageKey = "";
            presence.largeImageText = "";
            presence.smallImageKey = "";
            presence.smallImageText = "";

            DiscordRpc.UpdatePresence(ref presence);
        }
        public static void UpdatePresence(string details, string state, long startTimestamp, long endTimestamp, string largeImageKey, string largeImageText, string smallImageKey, string smallImageText)
        {
            presence.details = details;
            presence.state = state;

            presence.startTimestamp = startTimestamp;
            presence.endTimestamp = endTimestamp;

            presence.largeImageKey = largeImageKey;
            presence.largeImageText = largeImageText;
            presence.smallImageKey = smallImageKey;
            presence.smallImageText = smallImageText;

            DiscordRpc.UpdatePresence(ref presence);
        }
        public static void UpdatePresence(string details, string state, DateTime startTimestamp, DateTime endTimestamp)
        {
            presence.details = details;
            presence.state = state;

            if (startTimestamp.ToBinary() != 0 && endTimestamp.ToBinary() != 0)
            {
                presence.startTimestamp = DateTimeToTimestamp(startTimestamp);
                presence.endTimestamp = DateTimeToTimestamp(endTimestamp.Subtract(new TimeSpan(2, 0, 0)));
            }
            else
            {
                presence.startTimestamp = 0;
                presence.endTimestamp = 0;
            }

            presence.largeImageKey = "";
            presence.largeImageText = "";
            presence.smallImageKey = "";
            presence.smallImageText = "";

            DiscordRpc.UpdatePresence(ref presence);
        }
        public static void UpdatePresence(string details, string state, DateTime startTimestamp, DateTime endTimestamp, string largeImageKey, string largeImageText, string smallImageKey, string smallImageText, bool ElapsedTime)
        {
            presence.details = details;
            presence.state = state;

            if (startTimestamp.ToBinary() != 0 && endTimestamp.ToBinary() != 0)
            {
                if (ElapsedTime)
                {
                    presence.startTimestamp = DateTimeToTimestamp(startTimestamp.Subtract(new TimeSpan(1, 0, 0)));
                    presence.endTimestamp = 0;
                }
                else
                {
                    presence.startTimestamp = DateTimeToTimestamp(startTimestamp);
                    presence.endTimestamp = DateTimeToTimestamp(endTimestamp.Subtract(new TimeSpan(1, 0, 0)));
                }
            }
            else
            {
                presence.startTimestamp = 0;
                presence.endTimestamp = 0;
            }

            presence.largeImageKey = largeImageKey;
            presence.largeImageText = largeImageText;
            presence.smallImageKey = smallImageKey;
            presence.smallImageText = smallImageText;

            DiscordRpc.UpdatePresence(ref presence);
        }

        public static void RunCallbacks()
        {
            DiscordRpc.RunCallbacks();
        }

        public static void Shutdown()
        {
            DiscordRpc.Shutdown();
        }

        public static void ReadyCallback()
        {
            
        }

        public static void DisconnectedCallback(int errorCode, string message)
        {
            
        }

        public static void ErrorCallback(int errorCode, string message)
        {
            
        }

        public static long DateTimeToTimestamp(DateTime dt)
        {
            return (dt.Ticks - 621355968000000000) / 10000000;
        }
    }
}
