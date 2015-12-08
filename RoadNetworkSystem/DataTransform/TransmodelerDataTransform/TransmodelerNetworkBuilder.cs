using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace RoadNetworkSystem.TransmodelerDataTransform
{
    public class TransmodelerNetworkBuilder
    {
        private XmlDocument _xml;
        private XmlNode _root_node;
        private XmlNode _nodes;
        private XmlNode _links;
        private XmlNode _classes;
        private XmlNode _labels;
        private XmlNode _lines;
        private string _network;
        private string _path;



        public TransmodelerNetworkBuilder(string network, string path)
        {
            _network = network;
            _path = path;
        }

        /// <summary>
        /// 创建仿真路网XML描述文件
        /// </summary>
        /// <param name="network">路网名称</param>
        /// <param name="path">路网文件存放路径</param>
        /// <returns></returns>
        public bool CreateNetworkXML()
        {
            //
            try
            {
                //
                _xml = new XmlDocument();
                //
                XmlDeclaration Declaration = _xml.CreateXmlDeclaration("1.0", "ISO-8859-1", null);
                //
                _root_node = _xml.CreateNode(XmlNodeType.Element, "eizo", null);
                _root_node.Attributes.Append(_xml.CreateAttribute("version")).InnerText = "4.0";
                _xml.AppendChild(_root_node);

                //创建Nodes节点
                _nodes = _xml.CreateNode(XmlNodeType.Element, "Nodes", null);
                _root_node.AppendChild(_nodes);

                //创建Labels节点
                _labels = _xml.CreateNode(XmlNodeType.Element, "Labels", null);
                _root_node.AppendChild(_labels);

                //创建Classes节点
                _classes = _xml.CreateNode(XmlNodeType.Element, "Classes", null);
                _root_node.AppendChild(_classes);

                //创建Lines节点
                _lines = _xml.CreateNode(XmlNodeType.Element, "Lines", null);
                _root_node.AppendChild(_lines);

                //创建Links节点
                _links = _xml.CreateNode(XmlNodeType.Element, "Links", null);
                _root_node.AppendChild(_links);


                _xml.InsertBefore(Declaration, _xml.DocumentElement);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            //
            return true;
        }

        /// <summary>
        /// 保存路网XML文件
        /// </summary>
        public void SaveNetworkXML()
        {
            try
            {
                //
                _xml.Save(_path + "\\" + _network + ".xml");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="type">节点类型</param>
        /// <returns></returns>
        public bool AddNode(string id, string type)
        {
            //
            XmlNode node = _xml.CreateNode(XmlNodeType.Element, "N", null);
            _nodes.AppendChild(node);

            //设置属性
            node.Attributes.Append(_xml.CreateAttribute("id")).InnerText = id;

            if (type != "")
                node.Attributes.Append(_xml.CreateAttribute("type")).InnerText = type;

            return true;
        }

        /// <summary>
        /// 给节点添加标注
        /// </summary>
        /// <param name="node_id"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public bool AddLabel(string node_id, string label)
        {
            //
            //
            return true;
        }

        /// <summary>
        /// 增加道路类型
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="func"></param>
        /// <param name="rank"></param>
        /// <param name="er"></param>
        /// <param name="flow"></param>
        /// <param name="spdlmt"></param>
        /// <param name="freespd"></param>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <returns></returns>
        public bool AddClass(string id, string name, string func, string rank, string type,
            string er, string flow, string spdlmt, string freespd, string alpha, string beta, string content)
        {
            //
            XmlNode road_class = _xml.CreateNode(XmlNodeType.Element, "C", null);
            _classes.AppendChild(road_class);

            //设置属性
            road_class.Attributes.Append(_xml.CreateAttribute("id")).InnerText = id;
            road_class.Attributes.Append(_xml.CreateAttribute("name")).InnerText = name;
            road_class.Attributes.Append(_xml.CreateAttribute("func")).InnerText = func;
            road_class.Attributes.Append(_xml.CreateAttribute("rank")).InnerText = rank;
            road_class.Attributes.Append(_xml.CreateAttribute("type")).InnerText = type;
            road_class.Attributes.Append(_xml.CreateAttribute("er")).InnerText = er;
            road_class.Attributes.Append(_xml.CreateAttribute("flow")).InnerText = flow;
            road_class.Attributes.Append(_xml.CreateAttribute("spdlmt")).InnerText = spdlmt;
            road_class.Attributes.Append(_xml.CreateAttribute("freespd")).InnerText = freespd;
            road_class.Attributes.Append(_xml.CreateAttribute("alpha")).InnerText = alpha;
            road_class.Attributes.Append(_xml.CreateAttribute("beta")).InnerText = beta;
            road_class.InnerText = content;

            //
            return true;
        }

        public bool AddLine(string id, string type, string pt_array)
        {
            //
            //
            XmlNode line = _xml.CreateNode(XmlNodeType.Element, "G", null);
            _lines.AppendChild(line);

            //设置属性
            line.Attributes.Append(_xml.CreateAttribute("id")).InnerText = id;
            line.Attributes.Append(_xml.CreateAttribute("type")).InnerText = type;
            line.InnerText = pt_array;

            //
            return true;
        }

        public XmlNode AddLink(string id, string ups, string dns, string sl)
        {
            XmlNode link = _xml.CreateNode(XmlNodeType.Element, "K", null);

            //
            _links.AppendChild(link);
            link.Attributes.Append(_xml.CreateAttribute("id")).InnerText = id;
            link.Attributes.Append(_xml.CreateAttribute("ups")).InnerText = ups;
            link.Attributes.Append(_xml.CreateAttribute("dns")).InnerText = dns;
            if (sl != null && sl != "")
                link.Attributes.Append(_xml.CreateAttribute("sl")).InnerText = sl;

            //
            return link;
        }

        public XmlNode AddSegment(XmlNode parent_link, string id, string line)
        {
            XmlNode segment = _xml.CreateNode(XmlNodeType.Element, "S", null);
            parent_link.AppendChild(segment);

            //
            segment.Attributes.Append(_xml.CreateAttribute("id")).InnerText = id;
            segment.Attributes.Append(_xml.CreateAttribute("line")).InnerText = line;

            return segment;
        }

        public XmlNode AddLane(XmlNode parent_segment, string c, Dictionary<string, Connector_Entity> rs)
        {
            XmlNode lane = _xml.CreateNode(XmlNodeType.Element, "L", null);
            parent_segment.AppendChild(lane);

            //
            lane.Attributes.Append(_xml.CreateAttribute("c")).InnerText = c;

            //
            if (rs != null)
            {
                foreach (KeyValuePair<string, Connector_Entity> r_info in rs)
                {
                    XmlNode r = _xml.CreateNode(XmlNodeType.Element, "R", null);
                    lane.AppendChild(r);

                    //
                    Connector_Entity ety = (Connector_Entity)r_info.Value;
                    r.Attributes.Append(_xml.CreateAttribute("k")).InnerText = ety.LINK;        //ID
                    r.Attributes.Append(_xml.CreateAttribute("l")).InnerText = ety.LANE_POS;  //所在车道

                }
            }


            return lane;
        }

        public XmlNode AddSurv(XmlNode parent_segment, string type, string pos, string len, Dictionary<string, string> sensors)
        {
            XmlNode surv = _xml.CreateNode(XmlNodeType.Element, "Surv", null);
            parent_segment.AppendChild(surv);

            //
            surv.Attributes.Append(_xml.CreateAttribute("type")).InnerText = type;
            surv.Attributes.Append(_xml.CreateAttribute("pos")).InnerText = pos;
            surv.Attributes.Append(_xml.CreateAttribute("len")).InnerText = len;

            if (sensors != null)
            {
                //
                foreach (KeyValuePair<string, string> sensor_info in sensors)
                {
                    XmlNode sensor = _xml.CreateNode(XmlNodeType.Element, "sensor", null);
                    surv.AppendChild(sensor);

                    //设置检测器属性
                    sensor.Attributes.Append(_xml.CreateAttribute("id")).InnerText = sensor_info.Key;        //ID
                    sensor.Attributes.Append(_xml.CreateAttribute("lane")).InnerText = sensor_info.Value;  //所在车道

                }
            }


            return surv;
        }


        public XmlNode AddCtrl(XmlNode parent_segment, string type, string pos, string vis, Dictionary<string, string> signals)
        {
            XmlNode ctrl = _xml.CreateNode(XmlNodeType.Element, "Ctrl", null);
            parent_segment.AppendChild(ctrl);

            //
            ctrl.Attributes.Append(_xml.CreateAttribute("type")).InnerText = type;
            ctrl.Attributes.Append(_xml.CreateAttribute("pos")).InnerText = pos;
            ctrl.Attributes.Append(_xml.CreateAttribute("vis")).InnerText = vis;

            if (signals != null)
            {
                //
                foreach (KeyValuePair<string, string> signal_id in signals)
                {
                    XmlNode signal = _xml.CreateNode(XmlNodeType.Element, "signal", null);
                    ctrl.AppendChild(signal);

                    //设置检测器属性
                    signal.Attributes.Append(_xml.CreateAttribute("id")).InnerText = signal_id.Value;        //ID
                }
            }


            return ctrl;
        }

        //
        //XmlNode node1 = xml.CreateNode(XmlNodeType.Element, "v", "Game", "www-microsoft-game");
        //RootNode.AppendChild(node1);
        //node1.Attributes.Append(xml.CreateAttribute("name")).InnerText = "文明3";
        //node1.Attributes.Append(xml.CreateAttribute("k")).InnerText = "3";
        //node1.Attributes.Append(xml.CreateAttribute("l")).InnerText = "4";
        //XmlNode childNode = xml.CreateNode(XmlNodeType.Element, "Links", null);
        ////childNode.InnerText = "100";
        //childNode.Attributes.Append(xml.CreateAttribute("v")).InnerText = "100";
        //node1.AppendChild(childNode);
        //childNode.AppendChild(xml.CreateNode(XmlNodeType.Element, "S", null)).InnerText = "300";

        //
        //XmlNode node2 = xml.CreateNode(XmlNodeType.Element, "v", "Game", "www-microsoft-game");
        //RootNode.AppendChild(node2);
        //node2.Attributes.Append(xml.CreateAttribute("name")).InnerText = "帝国时代";
        //node2.AppendChild(xml.CreateNode(XmlNodeType.Element, "Price", null)).InnerText = "300";

    }
}
