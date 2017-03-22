using System;
using System.Web;
using System.Configuration;
using System.Xml;
using System.Xml.XmlConfiguration;

namespace Parking.Auxiliary
{
    public class Configs
    {
        /// <summary>
        /// 获取配置文件中，key对应的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns>key对应的值</returns>
        public static string GetValue(string key)
        {
            return ConfigurationManager.AppSettings[key].ToString();
        }

        /// <summary>
        /// 根据Key修改Value,如果不存在的，则添加对应键值
        /// </summary>
        /// <param name="key">要修改的Key</param>
        /// <param name="value">要修改为的值</param>
        public static void SetValue(string key, string value)
        {
            System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
            xDoc.Load(HttpContext.Current.Server.MapPath("~/Configs/System.config"));
            System.Xml.XmlNode xNode;
            System.Xml.XmlElement xElem1;
            System.Xml.XmlElement xElem2;
            xNode = xDoc.SelectSingleNode("//appSettings");

            xElem1 = (System.Xml.XmlElement)xNode.SelectSingleNode("//add[@key='" + key + "']");
            if (xElem1 != null)
            {
                xElem1.SetAttribute("value", value);
            }
            else
            {
                xElem2 = xDoc.CreateElement("add");
                xElem2.SetAttribute("key", key);
                xElem2.SetAttribute("value", value);
                xNode.AppendChild(xElem2);
            }
            xDoc.Save(HttpContext.Current.Server.MapPath("~/Configs/System.config"));
        }
    }

    public class XMLHelper
    {
        private static string path;
        static XMLHelper()
        {
            path = HttpContext.Current.Server.MapPath("~/Configs/System.xml");
        }

        /// <summary>
        /// 获取root目录下的某个节点的值
        /// </summary>
        /// <param name="root"></param>
        /// <param name="xname"></param>
        /// <returns></returns>
        public static string GetRootNodeValueByXpath(string root,string nodeName)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlReaderSettings setting = new XmlReaderSettings();
                setting.IgnoreComments = true;
                XmlReader reader = XmlReader.Create(path, setting);
                xmlDoc.Load(reader);

                string xpath = "//" + root + "//" + nodeName;
                XmlNode node = xmlDoc.SelectSingleNode(xpath);
                if (node != null)
                {
                    return node.InnerXml;
                }
            }
            catch (Exception ex)
            {
                Log log = LogFactory.GetLogger("XMLHelper.GetRootNodeValueByXpath");
                log.Error(ex.ToString());
            }
            return null;
        }

        /// <summary>
        /// 获取setting//PLC指定PLC节点下的某个节点值
        /// </summary>
        /// <param name="settingpath">格式：//root//setting</param>
        /// <param name="warehouse">PLC ID="1" 用于匹配 id值</param>
        /// <param name="nodeName">库区内PLC节点下的节点名</param>
        /// <returns></returns>
        public static XmlNode GetPlcNodeByTagName(string settingpath,string warehouse,string nodeName)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlReaderSettings setting = new XmlReaderSettings();
                setting.IgnoreComments = true;
                XmlReader reader = XmlReader.Create(path, setting);
                xmlDoc.Load(reader);

                XmlNode settingNode = xmlDoc.SelectSingleNode(settingpath);
                if (settingpath != null)
                {
                    XmlNodeList nodeList = settingNode.ChildNodes;
                    if (nodeList != null)
                    {
                        XmlNode xnode = GetXmlNodeByAttribute(nodeList, "ID", warehouse);
                        if (xnode != null)
                        {
                            XmlNode element = xnode.SelectSingleNode(nodeName);
                            return element;
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                Log log = LogFactory.GetLogger("XMLHelper.GetPlcNodeByTagName");
                log.Error(ex.ToString());
            }
            return null;
        }

        /// <summary>
        /// 获取setting//PLC(指定)//Halls下指定车厅的节点值
        /// 
        /// </summary>
        /// <param name="settingpath"></param>
        /// <param name="warehouse"></param>
        /// <param name="nodeName"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static XmlNode GetHallNodeByTageName(string settingpath,string warehouse,string hall, string nodeName)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlReaderSettings setting = new XmlReaderSettings();
                setting.IgnoreComments = true;
                XmlReader reader = XmlReader.Create(path, setting);
                xmlDoc.Load(reader);

                XmlNode halls = GetPlcNodeByTagName(settingpath, warehouse, "halls");
                if (halls != null)
                {
                    if (halls.HasChildNodes)
                    {
                        XmlNode hallN = GetXmlNodeHasChildeName(halls.ChildNodes, hall);
                        if (hallN != null)
                        {
                            XmlNode result = hallN.SelectSingleNode(nodeName);
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log log = LogFactory.GetLogger("XMLHelper.GetHallNodeByTageName");
                log.Error(ex.ToString());
            }
            return null;
        }

        /// <summary>
        /// 查找list节点中存在tagname的节点
        /// </summary>
        /// <param name="nodeList"></param>
        /// <param name="tagname"></param>
        /// <returns></returns>
        public static XmlNode GetXmlNodeByTagName(XmlNodeList nodeList,string tagname)
        {
            foreach(XmlNode node in nodeList)
            {
                if (node.Name == tagname)
                {
                    return node;
                }
            }
            return null;
        }

        /// <summary>
        /// 查找节点清单，找出有对应属性的，且属性值一样的节点
        /// </summary>
        /// <param name="nodeList"></param>
        /// <param name="attribute"></param>
        /// <param name="attrValue"></param>
        /// <returns></returns>
        public static XmlNode GetXmlNodeByAttribute(XmlNodeList nodeList, string attri, string attrValue)
        {
            foreach (XmlNode node in nodeList)
            {
                XmlAttribute attribute = node.Attributes[attri];
                if (attribute != null)
                {
                    if (attribute.InnerText == attrValue)
                    {
                        return node;
                    }
                }
            }
            return null;
        }

        public static string GetXmlValueOfAttribute(XmlNode node,string attri)
        {
            XmlAttribute attribute = node.Attributes[attri];
            if (attribute != null)
            {
                return attribute.InnerText;
            }
            return null;
        }

        /// <summary>
        /// 查询list中的节点的子节点是否包含指定值
        /// </summary>
        /// <param name="nodeList"></param>
        /// <param name="innerText"></param>
        /// <returns>返回list中的对应节点</returns>
        public static XmlNode GetXmlNodeHasChildeName(XmlNodeList nodeList,string innerText)
        {
            foreach(XmlNode node in nodeList)
            {
                if (node.HasChildNodes)
                {
                    foreach(XmlNode xnode in node.ChildNodes)
                    {
                        if (xnode.InnerText == innerText)
                        {
                            return node;
                        }
                    }
                }
            }
            return null;
        }


    }
}
