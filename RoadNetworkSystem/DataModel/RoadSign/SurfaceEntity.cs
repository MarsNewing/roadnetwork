using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.RoadSign
{
    class SurfaceEntity
    {
        /// <summary>
        /// 规定了数据模型，请不要在其他类中直接读取数据
        /// </summary>
        //public const string SurfaceIDNm = "SurfaceID";
        //public const string ArcIDNm = "ArcID";
        //public const string ControlIDsNm = "ControlIDs";


        //public const string OtherNm = "Other";
        public const string SurfaceName = "Surface";


        public int SurfaceID { get; set; }
        public int ArcID { get; set; }
        public string ControlIDs { get; set; }


        public int Other { get; set; }

        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public SurfaceEntity Copy()
        {
            SurfaceEntity surface = new SurfaceEntity();

            surface.SurfaceID = this.SurfaceID;
            surface.ControlIDs = this.ControlIDs;
            surface.ArcID = this.ArcID;

            surface.Other = this.Other;

            return surface;
        }
    }
}
