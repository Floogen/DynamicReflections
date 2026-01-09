using DynamicReflections.Framework.Models;
using DynamicReflections.Framework.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Linq;

namespace DynamicReflections.Framework.Managers
{
    internal class MessageManager
    {
        private IMonitor _monitor;
        private IModHelper _helper;
        private string _modID;

        internal enum MessageType
        {
            Unknown,
            LocationPuddleRequest,
            LocationPuddleResponse
        }

        internal class LocationPuddleRequestMessage
        {
            public string LocationName { get; set; }
        }

        internal class LocationPuddleResponseMessage
        {
            public string LocationName { get; set; }
            public PuddleTile[,] Puddles { get; set; }
        }

        public MessageManager(IMonitor monitor, IModHelper helper, string modID)
        {
            _monitor = monitor;
            _helper = helper;
            _modID = modID;
        }

        public void HandleIncomingMessage(ModMessageReceivedEventArgs e)
        {
            if (Enum.TryParse<MessageType>(e.Type, out var type) is false)
            {
                _monitor.LogOnce($"Failed to handle incoming message with type {e.Type}", LogLevel.Trace);
                return;
            }

            switch (type)
            {
                case MessageType.LocationPuddleRequest:
                    var requestMessage = e.ReadAs<LocationPuddleRequestMessage>();

                    var requestLocation = Game1.getLocationFromName(requestMessage.LocationName);
                    if (requestLocation is not null)
                    {
                        DynamicReflections.puddleManager.Generate(requestLocation);

                        if (DynamicReflections.puddleManager.locationToPuddleTiles.ContainsKey(requestLocation))
                        {
                            SendPuddleLocationTiles(requestLocation, DynamicReflections.puddleManager.locationToPuddleTiles[requestLocation]);
                        }
                    }
                    return;
                case MessageType.LocationPuddleResponse:
                    var responseMessage = e.ReadAs<LocationPuddleResponseMessage>();

                    var responseLocation = Game1.getLocationFromName(responseMessage.LocationName);
                    if (responseLocation is not null && responseMessage.Puddles is not null)
                    {
                        DynamicReflections.puddleManager.Sync(responseLocation, responseMessage.Puddles);
                    }
                    return;
            }
        }

        public void RequestPuddleLocationTiles(GameLocation location)
        {
            if (location is null)
            {
                return;
            }

            var message = new LocationPuddleResponseMessage()
            {
                LocationName = location.NameOrUniqueName
            };

            _helper.Multiplayer.SendMessage(message, MessageType.LocationPuddleRequest.ToString(), modIDs: new[] { _modID });
        }

        public void SendPuddleLocationTiles(GameLocation location, PuddleTile[,] puddleTiles)
        {
            if (location is null)
            {
                return;
            }

            var message = new LocationPuddleResponseMessage()
            {
                LocationName = location.NameOrUniqueName,
                Puddles = puddleTiles
            };

            _helper.Multiplayer.SendMessage(message, MessageType.LocationPuddleResponse.ToString(), modIDs: new[] { _modID });
        }
    }
}