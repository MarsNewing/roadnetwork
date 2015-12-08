using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.FileDirectory
{
    class FileHelper
    {
        /// <summary>
        /// 打开文件对话框
        /// </summary>
        /// <param name="initialPath"></param>
        /// <param name="title"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static string openFileDialog(string initialPath,string title, string filter)
        {
            
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = initialPath;
            fileDialog.Multiselect = false;
            fileDialog.Title = title;
            fileDialog.Filter = filter;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string file = fileDialog.FileName;
            }
            return null;
        }


        /// <summary>
        /// 获取选择文件夹的路径
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string OpenFolder(string title)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = title;
            
            string foldPath = Application.StartupPath;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                foldPath = dialog.SelectedPath;
                return foldPath;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 保存文件的对话框
        /// </summary>
        /// <param name="str"></param>"txt files (*.txt)|*.txt|All files (*.*)|*.*"
        /// <returns></returns>
        public static void SaveFile(string str, ref string directory, ref string fileNm)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = str;
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.ShowDialog();
            string filePath = saveFileDialog1.FileName;


            string[] fileDirectoryList = filePath.Split('\\');

            if (fileDirectoryList.Length > 0)
            {
                directory = fileDirectoryList[0];
                for (int i = 1; i < fileDirectoryList.Length - 1; i++)
                {
                    directory = directory + "\\" + fileDirectoryList[i];
                }
                fileNm = fileDirectoryList[fileDirectoryList.Length - 1];
            }
        }

    }
}
