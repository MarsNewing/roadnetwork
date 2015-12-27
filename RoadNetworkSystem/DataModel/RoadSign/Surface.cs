using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.RoadSign
{
    class Surface
    {
        public const string SurfaceIDNm = "SurfaceID";
        public const string ArcIDNm = "ArcID";
        public const string ControlIDsNm = "ControlIDs";
        public const string OtherNm = "Other";

        public const string SurfaceName = "Surface";


        public int SurfaceID { get; set; }
        public int ArcID { get; set; }
        public string ControlIDs { get; set; }


        public int Other { get; set; }

        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public Surface Copy()
        {
            Surface surface = new Surface();

            surface.SurfaceID = this.SurfaceID;
            surface.ControlIDs = this.ControlIDs;
            surface.ArcID = this.ArcID;

            surface.Other = this.Other;

            return surface;
        }
    }
}
