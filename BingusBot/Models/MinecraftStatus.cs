using System.Collections.Generic;

namespace BingusBot.Models
{
    /// <summary>
    ///     Model Representing DTO for Minecraft Server Status
    /// </summary>
    public class MinecraftStatus
    {
        /// <summary>
        ///     Status Summary
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        ///     If the server is online
        /// </summary>
        public bool Online { get; set; }

        /// <summary>
        ///     MOTD
        /// </summary>
        public string Motd { get; set; }

        /// <summary>
        ///     Favicon
        /// </summary>
        public string Favicon { get; set; }

        /// <summary>
        ///     Any errors occured by the API
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        ///     Player info
        /// </summary>
        public Players Players { get; set; }

        /// <summary>
        ///     Server Info
        /// </summary>
        public Server Server { get; set; }

        /// <summary>
        ///     When info on this server was last updated
        /// </summary>
        public string LastUpdated { get; set; }

        /// <summary>
        ///     When API last saw server online
        /// </summary>
        public string LastOnline { get; set; }

        /// <summary>
        ///     Duration of request
        /// </summary>
        public int Duration { get; set; }
    }

    /// <summary>
    ///     Player info for server
    /// </summary>
    public class Players
    {
        /// <summary>
        ///     Max no of players
        /// </summary>
        public int Max { get; set; }

        /// <summary>
        ///     No. of players online
        /// </summary>
        public int Now { get; set; }

        /// <summary>
        ///     Sample of online players
        /// </summary>
        public List<Player> Sample { get; set; }
    }

    /// <summary>
    ///     Server info from API
    /// </summary>
    public class Server
    {
        /// <summary>
        ///     Version Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Protocol
        /// </summary>
        public int Protocol { get; set; }
    }

    /// <summary>
    ///     Player info for server
    /// </summary>
    public class Player
    {
        /// <summary>
        ///     Player name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Player UUID
        /// </summary>
        public string Id { get; set; }
    }
}