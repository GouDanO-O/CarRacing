using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MoreMountains.HighroadEngine 
{
    /// <summary>
    ///大厅玩家
    /// </summary>
    public class LobbyPlayer : IPlayerInfo 
	{
		#region ILobbyPlayerInfo implementation

		public int Position { get; set; }

		public string Name { get; set; }

		public Texture PlayerImg { get;set;}

		public string VehicleName { get; set; }

		public int VehicleSelectedIndex { get; set; }

		public bool IsBot { get; set; }

		public int WhitchTeam { get; set; }

        #endregion
    }
}