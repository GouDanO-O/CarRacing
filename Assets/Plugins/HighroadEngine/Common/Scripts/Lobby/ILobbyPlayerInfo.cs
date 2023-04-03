using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace MoreMountains.HighroadEngine 
{
    /// <summary>
    ///在大厅屏幕上存储有关玩家的数据
    /// </summary>
    public interface IPlayerInfo 
	{
        /// <summary>
        ///玩家位置id(从0开始)
        /// </summary>
        int Position {get; set;}

        /// <summary>
        ///玩家的名字。由玩家位置生成，从服务器接收
        /// </summary>
        string Name {get; set;}
        /// <summary>
        ///玩家的头像。由玩家位置生成，从服务器接收
        /// </summary>
		Texture PlayerImg { get; set; }

        /// <summary>
        ///玩家使用的游戏对象名称。显示在UI中，用于实例化来自资源的预制件
        /// </summary>
        string VehicleName {get; set;}

        /// <summary>
        ///UI中所选车辆的索引。
        /// </summary>
        int VehicleSelectedIndex {get; set;}

        /// <summary>
        ///布尔值，表示车辆是否由AI控制
        /// </summary>
        bool IsBot { get; set; }
        /// <summary>
        ///组队赛玩家的队伍，按照玩家的弹幕输入来选择，如果没输入，默认在人少的队伍中插入(0 蓝队，1红队，2 个人赛）
        /// </summary>
        int WhitchTeam { get; set; }
	}
}
