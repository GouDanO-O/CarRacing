using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MoreMountains.HighroadEngine {

    /// <summary>
    ///  一般大堂经理界面(本地和在线)。
    /// 由比赛经理用来获取玩家的信息，并将命令发送给当前的大厅经理
    /// </summary>
    public interface IGenericLobbyManager 
	{
        /// <summary>
        ///返回玩家的最大数量
        /// </summary>
        /// <value>The max players.</value>
        int MaxPlayers { get; }

        /// <summary>
        /// 将当前场景更改为大厅场景。本地和在线大堂经理会有不同吗
        /// </summary>
        void ReturnToLobby();

        /// <summary>
        /// 将当前场景更改为开始屏幕
        /// </summary>
        void ReturnToStartScreen();
	}
}
