using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.esriSystem;
/*
'================================================================
'广州城市信息研究所有限公司无锡分公司 '版权所有，未经授权不得进行复制、修改、传播、反向工程。 '
'模块名称：   EsriApplicationLicense
'文件名称：  EsriApplicationLicense.cs
'模块说明：   arcgis认证模块
' 
'创建时间/创建者 |(2012-04-23)/陆庆丰  
'修改记录：| 说明(包括原因及涉及内容） |
'=================================================================
*/
namespace EsriApplication
{
    /// <summary>
    /// arcgis认证模块
    /// </summary>
    public class EsriApplicationLicense
    {
       
        #region 获取许可
        /// <summary>
        /// ESRI认证对象
        /// </summary>
        public static IAoInitialize m_pAoInitialize;

        private static esriLicenseProductCode m_LicenseCode;

        /// <summary>
        /// 认证ESRI许可
        /// </summary>
        /// <returns></returns>
        public static Boolean InitializeApplication()
        {
            string sMsg = string.Empty;
            return InitializeApplication(out sMsg);
        }
        /// <summary>
        /// 获取许可,初始化许可
        /// </summary>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        public static Boolean InitializeApplication(out string strMsg)
        {
            Boolean bInitialized = true;
            strMsg = "";

            if (m_pAoInitialize == null)
                m_pAoInitialize = new AoInitialize();

            if (m_pAoInitialize == null)
            {
                strMsg = "无法初始化ArcGIS ! 请检查 ArcGIS (Desktop, Engine or Server) 是否安装？";
                bInitialized = false;
            }
            esriLicenseStatus licenseStatus;
            licenseStatus = esriLicenseStatus.esriLicenseUnavailable;
            licenseStatus = CheckOutLicenses(esriLicenseProductCode.esriLicenseProductCodeEngineGeoDB);
            if (licenseStatus == esriLicenseStatus.esriLicenseCheckedOut)
                return bInitialized;

            licenseStatus = CheckOutLicenses(esriLicenseProductCode.esriLicenseProductCodeEngine);
            if (licenseStatus == esriLicenseStatus.esriLicenseCheckedOut)
                return bInitialized;
          
            licenseStatus = CheckOutLicenses(esriLicenseProductCode.esriLicenseProductCodeEngine);
            if (licenseStatus == esriLicenseStatus.esriLicenseCheckedOut)
                return bInitialized;

            licenseStatus = CheckOutLicenses(esriLicenseProductCode.esriLicenseProductCodeArcServer);
            if (licenseStatus != esriLicenseStatus.esriLicenseCheckedOut)
            {
                strMsg = LicenseMessage(licenseStatus);
                bInitialized = false;
            }

            return bInitialized;
        }
        /// <summary>
        /// 关闭许可
        /// </summary>
        public static void ShutdownApplication()
        {
            try
            {
                if (m_pAoInitialize == null)
                    return;
                m_pAoInitialize.Shutdown();
                m_pAoInitialize = null;
            }
            catch
            {
                //不做任何处理
            }
        }
        /// <summary>
        /// 检查某个产品许可，及许可值
        /// </summary>
        /// <param name="productCode"></param>
        /// <returns></returns>
        private static esriLicenseStatus CheckOutLicenses(esriLicenseProductCode productCode)
        {
            esriLicenseStatus licenseStatus;

            m_LicenseCode = productCode;
            //  'Determine if the product is available
            licenseStatus = m_pAoInitialize.IsProductCodeAvailable(productCode);

            if (licenseStatus == ESRI.ArcGIS.esriSystem.esriLicenseStatus.esriLicenseAvailable)
            {
                licenseStatus = m_pAoInitialize.Initialize(productCode);
                if (licenseStatus == ESRI.ArcGIS.esriSystem.esriLicenseStatus.esriLicenseCheckedOut)
                {
                    //数据格式转换
                    esriLicenseStatus licenseStatusExt = m_pAoInitialize.IsExtensionCodeAvailable(productCode, esriLicenseExtensionCode.esriLicenseExtensionCodeDataInteroperability);
                    if (licenseStatusExt == esriLicenseStatus.esriLicenseAvailable)
                    {
                        licenseStatusExt = m_pAoInitialize.CheckOutExtension(esriLicenseExtensionCode.esriLicenseExtensionCodeDataInteroperability);
                    }

                    //添加3DAnalyst扩展模块,陆庆丰，2009.2.3
                    licenseStatusExt = m_pAoInitialize.IsExtensionCodeAvailable(productCode, esriLicenseExtensionCode.esriLicenseExtensionCode3DAnalyst);
                    if (licenseStatusExt == esriLicenseStatus.esriLicenseAvailable)
                    {
                        licenseStatusExt = m_pAoInitialize.CheckOutExtension(esriLicenseExtensionCode.esriLicenseExtensionCode3DAnalyst);
                    } 
                }
            }
            return licenseStatus;
        }

        /// <summary>
        /// 注册扩展模块
        /// </summary>
        /// <param name="extensionCode">模块编码</param>
        /// <returns>是否成功</returns>
        public static bool CheckOutExtension(esriLicenseExtensionCode extensionCode)
        {
            if (m_pAoInitialize==null)
            {
                return false;
            }
            esriLicenseStatus licenseStatus = m_pAoInitialize.IsExtensionCodeAvailable(m_LicenseCode, extensionCode);
            if (licenseStatus==esriLicenseStatus.esriLicenseAvailable)
            {
                licenseStatus = m_pAoInitialize.CheckOutExtension(extensionCode); 
            }
            if (licenseStatus!=esriLicenseStatus.esriLicenseCheckedOut)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// 中文描述许可值
        /// </summary>
        /// <param name="licenseStatus"></param>
        /// <returns></returns>
        private static string LicenseMessage(esriLicenseStatus licenseStatus)
        {
            if (licenseStatus == ESRI.ArcGIS.esriSystem.esriLicenseStatus.esriLicenseNotLicensed)
                return "没有许可运行程序!";
            else if (licenseStatus == ESRI.ArcGIS.esriSystem.esriLicenseStatus.esriLicenseUnavailable)
                return "许可的权限不足!";
            else if (licenseStatus == ESRI.ArcGIS.esriSystem.esriLicenseStatus.esriLicenseFailure)
                return "检查许可时，无法预知的错误！";
            else if (licenseStatus == ESRI.ArcGIS.esriSystem.esriLicenseStatus.esriLicenseAlreadyInitialized)
                return "许可检查成功，请检查另外的程序！";

            return "未知的检查结果！";
        }

        #endregion
     
    }
}
